using System;
using System.IO;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Reader settings patterned off of XmlReaderSettings
    /// </summary>
    /// <see cref="https://referencesource.microsoft.com/#System.Xml/System/Xml/Core/XmlReaderSettings.cs"/>
    public class ClrMameProReaderSettings
    {
        // TODO: Add comments
        private int lineNumberOffset;
        private int linePositionOffset;

        private long maxCharactersInDocument;
        private long maxCharactersFromEntities;

        private bool ignoreWhitespace;
        private bool ignoreComments;

        private bool closeInput;

        private bool isReadOnly;

        public ClrMameProReaderSettings()
        {
            Initialize();
        }

        public int LineNumberOffset
        {
            get
            {
                return lineNumberOffset;
            }
            set
            {
                CheckReadOnly("LineNumberOffset");
                lineNumberOffset = value;
            }
        }

        public int LinePositionOffset
        {
            get
            {
                return linePositionOffset;
            }
            set
            {
                CheckReadOnly("LinePositionOffset");
                linePositionOffset = value;
            }
        }

        public long MaxCharactersInDocument
        {
            get
            {
                return maxCharactersInDocument;
            }
            set
            {
                CheckReadOnly("MaxCharactersInDocument");
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                maxCharactersInDocument = value;
            }
        }

        public long MaxCharactersFromEntities
        {
            get
            {
                return maxCharactersFromEntities;
            }
            set
            {
                CheckReadOnly("MaxCharactersFromEntities");
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                maxCharactersFromEntities = value;
            }
        }

        public bool IgnoreWhitespace
        {
            get
            {
                return ignoreWhitespace;
            }
            set
            {
                CheckReadOnly("IgnoreWhitespace");
                ignoreWhitespace = value;
            }
        }

        public bool IgnoreComments
        {
            get
            {
                return ignoreComments;
            }
            set
            {
                CheckReadOnly("IgnoreComments");
                ignoreComments = value;
            }
        }

        public bool CloseInput
        {
            get
            {
                return closeInput;
            }
            set
            {
                CheckReadOnly("CloseInput");
                closeInput = value;
            }
        }

        public void Reset()
        {
            CheckReadOnly("Reset");
            Initialize();
        }

        public ClrMameProReaderSettings Clone()
        {
            ClrMameProReaderSettings clonedSettings = this.MemberwiseClone() as ClrMameProReaderSettings;
            clonedSettings.ReadOnly = false;
            return clonedSettings;
        }

        internal ClrMameProReader CreateReader(Stream input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // create text XML reader
            ClrMameProReader reader = new ClrMameProReaderImpl(input, null, 0, this, closeInput);
            return reader;
        }

        internal ClrMameProReader CreateReader(TextReader input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // create xml text reader
            ClrMameProReader reader = new ClrMameProReaderImpl(input, this);
            return reader;
        }

        internal bool ReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        internal void CheckReadOnly(string propertyName)
        {
            if (isReadOnly)
                throw new ArgumentException(propertyName);
        }

        private void Initialize()
        {
            maxCharactersFromEntities = (long)1e7;
            lineNumberOffset = 0;
            linePositionOffset = 0;

            ignoreWhitespace = false;
            ignoreComments = false;
            closeInput = false;

            maxCharactersInDocument = 0;

            isReadOnly = false;
        }
    }
}
