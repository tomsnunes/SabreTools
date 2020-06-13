using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Internal reader patterned off of ClrMameProReaderImpl
    /// </summary>
    /// <see cref="https://referencesource.microsoft.com/#System.Xml/System/Xml/Core/ClrMameProReaderImpl.cs"/>
    public class ClrMameProReaderImpl : ClrMameProReader
    {
        // ParsingFunction = what should the reader do when the next Read() is called
        private enum ParsingFunction
        {
            ElementContent = 0,
            NoData,
            OpenUrl,
            SwitchToInteractive,
            SwitchToInteractiveXmlDecl,
            DocumentContent,
            MoveToElementContent,
            PopElementContext,
            PopEmptyElementContext,
            ResetAttributesRootLevel,
            Error,
            Eof,
            ReaderClosed,
            EntityReference,
            InIncrementalRead,
            FragmentAttribute,
            ReportEndEntity,
            AfterResolveEntityInContent,
            AfterResolveEmptyEntityInContent,
            XmlDeclarationFragment,
            GoToEof,
            PartialTextValue,

            // these two states must be last; see InAttributeValueIterator property
            InReadAttributeValue,
            InReadValueChunk,
            InReadContentAsBinary,
            InReadElementContentAsBinary,
        }

        private enum ParsingMode
        {
            Full,
            SkipNode,
            SkipContent,
        }

        private enum EntityType
        {
            CharacterDec,
            CharacterHex,
            CharacterNamed,
            Expanded,
            Skipped,
            FakeExpanded,
            Unexpanded,
            ExpandedInAttribute,
        }

        private enum EntityExpandType
        {
            All,
            OnlyGeneral,
            OnlyCharacter,
        }

        private enum IncrementalReadState
        {
            // Following values are used in ReadText, ReadBase64 and ReadBinHex (V1 streaming methods)
            Text,
            StartTag,
            PI,
            CDATA,
            Comment,
            Attributes,
            AttributeValue,
            ReadData,
            EndElement,
            End,

            // Following values are used in ReadTextChunk, ReadContentAsBase64 and ReadBinHexChunk (V2 streaming methods)
            ReadValueChunk_OnCachedValue,
            ReadValueChunk_OnPartialValue,

            ReadContentAsBinary_OnCachedValue,
            ReadContentAsBinary_OnPartialValue,
            ReadContentAsBinary_End,
        }

        #region Later Init Fields

        //later init means in the construction stage, do not opend filestream and do not read any data from Stream/TextReader
        //the purpose is to make the Create of ClrMameProReader do not block on IO.
        private class LaterInitParam
        {
            public bool useAsync = false;

            public Stream inputStream;
            public byte[] inputBytes;
            public int inputByteCount;
            public Uri inputbaseUri;
            public string inputUriStr;
            public TextReader inputTextReader;

            public InitInputType initType = InitInputType.Invalid;
        }

        LaterInitParam laterInitParam = null;

        enum InitInputType
        {
            UriString,
            Stream,
            TextReader,
            Invalid
        }

        #endregion

        // current parsing state (aka. scanner data) 
        ParsingState ps;

        // parsing function = what to do in the next Read() (3-items-long stack, usually used just 2 level)
        ParsingFunction parsingFunction;
        ParsingFunction nextParsingFunction;
        ParsingFunction nextNextParsingFunction;

        // stack of nodes
        NodeData[] nodes;

        // current node
        NodeData curNode;

        // current index
        int index = 0;

        // attributes info
        int curAttrIndex = -1;
        int attrCount;
        int attrHashtable;
        int attrDuplWalkCount;
        bool attrNeedNamespaceLookup;
        bool fullAttrCleanup;
        NodeData[] attrDuplSortingArray;

        // this is only for constructors that takes url 
        string url = string.Empty;
        CompressedStack compressedStack;

        // settings
        bool normalize;
        int lineNumberOffset;
        int linePositionOffset;
        bool closeInput;
        long maxCharactersInDocument;
        long maxCharactersFromEntities;

        // this flag enables ClrMameProReader backwards compatibility; 
        // when false, the reader has been created via ClrMameProReader.Create
        bool v1Compat;

        // stack of parsing states (=stack of entities)
        private ParsingState[] parsingStatesStack;
        private int parsingStatesStackTop = -1;

        // current node base uri and encoding
        string reportedBaseUri;
        Encoding reportedEncoding;

        // fragment parsing
        ClrMameProNodeType fragmentType = ClrMameProNodeType.None;
        bool fragment;

        // incremental read
        IncrementalReadDecoder incReadDecoder;
        IncrementalReadState incReadState;
        LineInfo incReadLineInfo;
        BinHexDecoder binHexDecoder;
        Base64Decoder base64Decoder;
        int incReadDepth;
        int incReadLeftStartPos;
        int incReadLeftEndPos;
        IncrementalReadCharsDecoder readCharsDecoder;

        // ReadAttributeValue helpers
        int attributeValueBaseEntityId;
        bool emptyEntityInAttributeResolved;

        // misc
        bool addDefaultAttributesAndNormalize;
        StringBuilder stringBuilder;
        bool rootElementParsed;
        bool standalone;
        int nextEntityId = 1;
        ParsingMode parsingMode;
        ReadState readState = ReadState.Initial;
        bool afterResetState;
        int documentStartBytePos;
        int readValueOffset;

        // Counters for security settings
        long charactersInDocument;
        long charactersFromEntities;

        // DOM helpers
        bool disableUndeclaredEntityCheck;

        // Outer ClrMameProReader exposed to the user - either ClrMameProReader or ClrMameProReaderImpl (when created via ClrMameProReader.Create).
        // Virtual methods called from within ClrMameProReaderImpl must be called on the outer reader so in case the user overrides
        // some of the ClrMameProReader methods we will call the overriden version.
        ClrMameProReader outerReader;

        private const int MaxBytesToMove = 128;
        private const int NodesInitialSize = 8;
        private const int InitialAttributesCount = 4;
        private const int InitialParsingStateStackSize = 2;
        private const int InitialParsingStatesDepth = 2;
        private const int MaxByteSequenceLen = 6;  // max bytes per character
        private const int MaxAttrDuplWalkCount = 250;
        private const int MinWhitespaceLookahedCount = 4096;

        internal ClrMameProReaderImpl()
        {
            curNode = new NodeData();
            parsingFunction = ParsingFunction.NoData;
        }

        // This constructor is used when creating ClrMameProReaderImpl reader via "ClrMameProReader.Create(..)"
        private ClrMameProReaderImpl(ClrMameProReaderSettings settings)
        {
            v1Compat = false;
            outerReader = this;

            nodes = new NodeData[NodesInitialSize];
            nodes[0] = new NodeData();
            curNode = nodes[0];

            stringBuilder = new StringBuilder();

            normalize = true;
            lineNumberOffset = settings.LineNumberOffset;
            linePositionOffset = settings.LinePositionOffset;
            ps.lineNo = lineNumberOffset + 1;
            ps.lineStartPos = -linePositionOffset - 1;
            curNode.SetLineInfo(ps.LineNo - 1, ps.LinePos - 1);
            maxCharactersInDocument = settings.MaxCharactersInDocument;
            maxCharactersFromEntities = settings.MaxCharactersFromEntities;

            charactersInDocument = 0;
            charactersFromEntities = 0;

            parsingFunction = ParsingFunction.SwitchToInteractiveXmlDecl;
            nextParsingFunction = ParsingFunction.DocumentContent;
        }

        // Initializes a new instance of the XmlTextReaderImpl class with the specified stream, baseUri and nametable
        // This constructor is used when creating XmlTextReaderImpl for V1 XmlTextReader
        internal ClrMameProReaderImpl(Stream input) : this(string.Empty, input, new NameTable())
        {
            InitStreamInput(input, null);
        }
        internal ClrMameProReaderImpl(Stream input, XmlNameTable nt) : this(string.Empty, input, nt)
        {
        }
        internal ClrMameProReaderImpl(string url, Stream input) : this(url, input, new NameTable())
        {
        }
        internal ClrMameProReaderImpl(string url, Stream input, XmlNameTable nt) : this(nt)
        {
            namespaceManager = new XmlNamespaceManager(nt);
            if (url == null || url.Length == 0)
            {
                
            }
            else
            {
                InitStreamInput(url, input, null);
            }
            reportedBaseUri = ps.baseUriStr;
            reportedEncoding = ps.encoding;
        }
    }
}
