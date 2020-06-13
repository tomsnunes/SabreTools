using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Reader settings patterned off of ClrMameProReaderSettings
    /// </summary>
    /// <see cref="https://referencesource.microsoft.com/#System.Xml/System/Xml/Core/ClrMameProReader.cs"/>
    public class ClrMameProReader : IDisposable
    {
        // TODO: Add comments
        internal const int DefaultBufferSize = 4096;
        internal const int BiggerBufferSize = 8192;
        internal const int MaxStreamLengthForDefaultBufferSize = 64 * 1024; // 64kB

        internal const int AsyncBufferSize = 64 * 1024; //64KB

        public ClrMameProReaderSettings Settings
        {
            get { return null; }
        }

        // Get the type of the current node.
        public ClrMameProNodeType NodeType { get; }

        // Gets the name of the current node, including the namespace prefix.
        public string Name
        {
            get
            {
                return LocalName;
            }
        }

        // Gets the name of the current node without the namespace prefix.
        public string LocalName { get; }

        // Gets a value indicating whether
        public bool HasValue
        {
            get
            {
                return HasValueInternal(this.NodeType);
            }
        }

        // Gets the text value of the current node.
        public string Value { get; }

        // Gets the depth of the current node in the element stack.
        public int Depth { get; }

        // Gets a value indicating whether the current node is an empty element (for example, <MyElement/>).
        public bool IsEmptyElement { get; }

        // Gets the quotation mark character used to enclose the value of an attribute node.
        public char QuoteChar
        {
            get
            {
                return '"';
            }
        }

        // returns the type of the current node
        public Type ValueType
        {
            get
            {
                return typeof(string);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and returns the content as the most appropriate type (by default as string). Stops at start tags and end tags.
        public object ReadContentAsObject()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsObject");

            return InternalReadContentAsString();
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to a boolean. Stops at start tags and end tags.
        public bool ReadContentAsBoolean()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsBoolean");

            try
            {
                return bool.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "Boolean", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to a DateTime. Stops at start tags and end tags.
        public DateTime ReadContentAsDateTime()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsDateTime");

            try
            {
                return DateTime.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "DateTime", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to a DateTimeOffset. Stops at start tags and end tags.
        public DateTimeOffset ReadContentAsDateTimeOffset()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsDateTimeOffset");

            try
            {
                return DateTimeOffset.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "DateTimeOffset", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to a double. Stops at start tags and end tags.
        public double ReadContentAsDouble()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsDouble");

            try
            {
                return double.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "Double", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to a float. Stops at start tags and end tags.
        public float ReadContentAsFloat()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsFloat");

            try
            {
                return float.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "Float", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to a decimal. Stops at start tags and end tags.
        public decimal ReadContentAsDecimal()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsDecimal");

            try
            {
                decimal.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "Decimal", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to an int. Stops at start tags and end tags.
        public int ReadContentAsInt()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsInt");

            try
            {
                int.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "Int", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to a long. Stops at start tags and end tags.
        public long ReadContentAsLong()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsLong");

            try
            {
                long.Parse(InternalReadContentAsString());
            }
            catch (FormatException e)
            {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "Long", e, this as IXmlLineInfo);
            }
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and returns the content as a string. Stops at start tags and end tags.
        public string ReadContentAsString()
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAsString");

            return InternalReadContentAsString();
        }

        // Concatenates values of textual nodes of the current content, ignoring comments and PIs, expanding entity references, 
        // and converts the content to the requested type. Stops at start tags and end tags.
        public object ReadContentAs(Type returnType)
        {
            if (!CanReadContentAs())
                throw CreateReadContentAsException("ReadContentAs");

            string strContentValue = InternalReadContentAsString();
            if (returnType == typeof(string))
            {
                return strContentValue;
            }
            else
            {
                try
                {
                    return XmlUntypedConverter.Untyped.ChangeType(strContentValue, returnType, this as IXmlNamespaceResolver);
                }
                catch (FormatException e)
                {
                    throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
                }
                catch (InvalidCastException e)
                {
                    throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
                }
            }
        }

        // Returns the content of the current element as the most appropriate type. Moves to the node following the element's end tag.
        public object ReadElementContentAsObject()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsObject"))
            {
                object value = ReadContentAsObject();
                FinishReadElementContentAsXxx();
                return value;
            }

            return string.Empty;
        }

        // Returns the content of the current element as a boolean. Moves to the node following the element's end tag.
        public bool ReadElementContentAsBoolean()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsBoolean"))
            {
                bool value = ReadContentAsBoolean();
                FinishReadElementContentAsXxx();
                return value;
            }

            return bool.Parse(string.Empty);
        }

        // Returns the content of the current element as a DateTime. Moves to the node following the element's end tag.
        public DateTime ReadElementContentAsDateTime()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsDateTime"))
            {
                DateTime value = ReadContentAsDateTime();
                FinishReadElementContentAsXxx();
                return value;
            }

            return DateTime.Parse(string.Empty);
        }

        // Returns the content of the current element as a double. Moves to the node following the element's end tag.
        public double ReadElementContentAsDouble()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsDouble"))
            {
                double value = ReadContentAsDouble();
                FinishReadElementContentAsXxx();
                return value;
            }

            return double.Parse(string.Empty);
        }

        // Returns the content of the current element as a float. Moves to the node following the element's end tag.
        public float ReadElementContentAsFloat()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsFloat"))
            {
                float value = ReadContentAsFloat();
                FinishReadElementContentAsXxx();
                return value;
            }

            return float.Parse(string.Empty);
        }

        // Returns the content of the current element as a decimal. Moves to the node following the element's end tag.
        public decimal ReadElementContentAsDecimal()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsDecimal"))
            {
                decimal value = ReadContentAsDecimal();
                FinishReadElementContentAsXxx();
                return value;
            }

            return decimal.Parse(string.Empty);
        }

        // Returns the content of the current element as an int. Moves to the node following the element's end tag.
        public int ReadElementContentAsInt()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsInt"))
            {
                int value = ReadContentAsInt();
                FinishReadElementContentAsXxx();
                return value;
            }

            return int.Parse(string.Empty);
        }

        // Returns the content of the current element as a long. Moves to the node following the element's end tag.
        public long ReadElementContentAsLong()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsLong"))
            {
                long value = ReadContentAsLong();
                FinishReadElementContentAsXxx();
                return value;
            }

            return long.Parse(string.Empty);
        }

        // Returns the content of the current element as a string. Moves to the node following the element's end tag.
        public string ReadElementContentAsString()
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAsString"))
            {
                string value = ReadContentAsString();
                FinishReadElementContentAsXxx();
                return value;
            }

            return string.Empty;
        }

        // Returns the content of the current element as the requested type. Moves to the node following the element's end tag.
        public object ReadElementContentAs(Type returnType)
        {
            if (SetupReadElementContentAsXxx("ReadElementContentAs"))
            {
                object value = ReadContentAs(returnType);
                FinishReadElementContentAsXxx();
                return value;
            }

            return (returnType == typeof(string)) ? string.Empty : XmlUntypedConverter.Untyped.ChangeType(string.Empty, returnType);
        }

        // The number of attributes on the current node.
        public abstract int AttributeCount { get; }

        // Gets the value of the attribute with the specified Name
        public abstract string GetAttribute(string name);

        // Gets the value of the attribute with the LocalName and NamespaceURI
        public abstract string GetAttribute(string name, string namespaceURI);

        // Gets the value of the attribute with the specified index.
        public abstract string GetAttribute(int i);

        // Gets the value of the attribute with the specified index.
        public string this[int i]
        {
            get
            {
                return GetAttribute(i);
            }
        }

        // Gets the value of the attribute with the specified Name.
        public string this[string name]
        {
            get
            {
                return GetAttribute(name);
            }
        }

        // Gets the value of the attribute with the LocalName and NamespaceURI
        public string this[string name, string namespaceURI]
        {
            get
            {
                return GetAttribute(name, namespaceURI);
            }
        }

        // Moves to the attribute with the specified Name.
        public abstract bool MoveToAttribute(string name);

        // Moves to the attribute with the specified LocalName and NamespaceURI.
        public abstract bool MoveToAttribute(string name, string ns);

        // Moves to the attribute with the specified index.
        public void MoveToAttribute(int i)
        {
            if (i < 0 || i >= AttributeCount)
                throw new ArgumentOutOfRangeException("i");

            MoveToElement();
            MoveToFirstAttribute();
            int j = 0;
            while (j < i)
            {
                MoveToNextAttribute();
                j++;
            }
        }

        // Moves to the first attribute of the current node.
        public abstract bool MoveToFirstAttribute();

        // Moves to the next attribute.
        public abstract bool MoveToNextAttribute();

        // Moves to the element that contains the current attribute node.
        public abstract bool MoveToElement();

        // Parses the attribute value into one or more Text and/or EntityReference node types.
        public abstract bool ReadAttributeValue();

        // Reads the next node from the stream.
        public abstract bool Read();

        // Returns true when the ClrMameProReader is positioned at the end of the stream.
        public abstract bool EOF { get; }

        // Closes the stream/TextReader (if CloseInput==true), changes the ReadState to Closed, and sets all the properties back to zero/empty string.
        public void Close() { }

        // Returns the read state of the ClrMameProReader.
        public abstract ReadState ReadState { get; }

        // Skips to the end tag of the current element.
        public void Skip()
        {
            if (ReadState != ReadState.Interactive)
            {
                return;
            }

            SkipSubtree();
        }

        // Binary content access methods
        // Returns true if the reader supports call to ReadContentAsBase64, ReadElementContentAsBase64, ReadContentAsBinHex and ReadElementContentAsBinHex.
        public bool CanReadBinaryContent
        {
            get
            {
                return false;
            }
        }

        // Returns decoded bytes of the current base64 text content. Call this methods until it returns 0 to get all the data.
        public int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadContentAsBase64"));
        }

        // Returns decoded bytes of the current base64 element content. Call this methods until it returns 0 to get all the data.
        public int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadElementContentAsBase64"));
        }

        // Returns decoded bytes of the current binhex text content. Call this methods until it returns 0 to get all the data.
        public int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadContentAsBinHex"));
        }

        // Returns decoded bytes of the current binhex element content. Call this methods until it returns 0 to get all the data.
        public int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString(Res.Xml_ReadBinaryContentNotSupported, "ReadElementContentAsBinHex"));
        }

        // Returns true if the ClrMameProReader supports calls to ReadValueChunk.
        public bool CanReadValueChunk
        {
            get
            {
                return false;
            }
        }

        // Returns a chunk of the value of the current node. Call this method in a loop to get all the data. 
        // Use this method to get a streaming access to the value of the current node.
        public int ReadValueChunk(char[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString(Res.Xml_ReadValueChunkNotSupported));
        }

        // Reads the contents of an element as a string. Stops of comments, PIs or entity references.
        public string ReadString()
        {
            if (this.ReadState != ReadState.Interactive)
                return string.Empty;

            this.MoveToElement();
            if (this.NodeType == ClrMameProNodeType.Element)
            {
                if (this.IsEmptyElement)
                    return string.Empty;
                else if (!this.Read())
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));

                if (this.NodeType == ClrMameProNodeType.EndElement)
                    return string.Empty;
            }

            string result = string.Empty;

            while (IsTextualNode(this.NodeType))
            {
                result += this.Value;
                if (!this.Read())
                    break;
            }

            return result;
        }

        // Checks whether the current node is a content (non-whitespace text, Standalone, Element, EndElement, EntityReference
        // or EndEntity) node. If the node is not a content node, then the method skips ahead to the next content node or 
        // end of file. Skips over nodes of type ProcessingInstruction, DocumentType, Comment, Whitespace and SignificantWhitespace.
        public ClrMameProNodeType MoveToContent()
        {
            do
            {
                switch (this.NodeType)
                {
                    case ClrMameProNodeType.Attribute:
                        MoveToElement();
                        goto case ClrMameProNodeType.Element;
                    case ClrMameProNodeType.Element:
                    case ClrMameProNodeType.EndElement:
                    case ClrMameProNodeType.Standalone:
                    case ClrMameProNodeType.Text:
                        return this.NodeType;
                }
            } while (Read());

            return this.NodeType;
        }

        // Checks that the current node is an element and advances the reader to the next node.
        public void ReadStartElement()
        {
            if (MoveToContent() != ClrMameProNodeType.Element)
            {
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);
            }

            Read();
        }

        // Checks that the current content node is an element with the given Name and advances the reader to the next node.
        public void ReadStartElement(string name)
        {
            if (MoveToContent() != ClrMameProNodeType.Element)
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);

            if (this.Name == name)
                Read();
            else
                throw new XmlException(Res.Xml_ElementNotFound, name, this as IXmlLineInfo);
        }

        // Checks that the current content node is an element with the given LocalName and NamespaceURI
        // and advances the reader to the next node.
        public void ReadStartElement(string localname, string ns)
        {
            if (MoveToContent() != ClrMameProNodeType.Element)
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);

            if (this.LocalName == localname && this.NamespaceURI == ns)
                Read();
            else
                throw new XmlException(Res.Xml_ElementNotFoundNs, new string[2] { localname, ns }, this as IXmlLineInfo);

        }

        // Reads a text-only element.
        public string ReadElementString()
        {
            string result = string.Empty;

            if (MoveToContent() != ClrMameProNodeType.Element)
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);

            if (!this.IsEmptyElement)
            {
                Read();
                result = ReadString();
                if (this.NodeType != ClrMameProNodeType.EndElement)
                    throw new XmlException(Res.Xml_UnexpectedNodeInSimpleContent, new string[] { this.NodeType.ToString(), "ReadElementString" }, this as IXmlLineInfo);

                Read();
            }
            else
            {
                Read();
            }

            return result;
        }

        // Checks that the Name property of the element found matches the given string before reading a text-only element.
        public string ReadElementString(string name)
        {
            string result = string.Empty;

            if (MoveToContent() != ClrMameProNodeType.Element)
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);

            if (this.Name != name)
                throw new XmlException(Res.Xml_ElementNotFound, name, this as IXmlLineInfo);

            if (!this.IsEmptyElement)
            {
                //Read();
                result = ReadString();
                if (this.NodeType != ClrMameProNodeType.EndElement)
                    throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);

                Read();
            }
            else
            {
                Read();
            }
            return result;
        }

        // Checks that the current content node is an end tag and advances the reader to the next node.
        public void ReadEndElement()
        {
            if (MoveToContent() != ClrMameProNodeType.EndElement)
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString(), this as IXmlLineInfo);

            Read();
        }

        // Calls MoveToContent and tests if the current content node is a start tag or empty element tag (ClrMameProNodeType.Element).
        public bool IsStartElement()
        {
            return MoveToContent() == ClrMameProNodeType.Element;
        }

        // Calls MoveToContentand tests if the current content node is a start tag or empty element tag (ClrMameProNodeType.Element) and if the
        // Name property of the element found matches the given argument.
        public bool IsStartElement(string name)
        {
            return (MoveToContent() == ClrMameProNodeType.Element) &&
                   (this.Name == name);
        }

        // Reads to the following element with the given Name.
        public bool ReadToFollowing(string name)
        {
            if (name == null || name.Length == 0)
                throw XmlConvert.CreateInvalidNameArgumentException(name, "name");

            // find following element with that name
            while (Read())
            {
                if (NodeType == ClrMameProNodeType.Element && string.Equals(name, Name))
                    return true;
            }

            return false;
        }

        // Reads to the first descendant of the current element with the given Name.
        public bool ReadToDescendant(string name)
        {
            if (name == null || name.Length == 0)
                throw XmlConvert.CreateInvalidNameArgumentException(name, "name");

            // save the element or root depth
            int parentDepth = Depth;
            if (NodeType != ClrMameProNodeType.Element)
            {
                // adjust the depth if we are on root node
                if (ReadState == ReadState.Initial)
                    parentDepth--;
                else
                    return false;
            }
            else if (IsEmptyElement)
            {
                return false;
            }

            // find the descendant
            while (Read() && Depth > parentDepth)
            {
                if (NodeType == ClrMameProNodeType.Element && string.Equals(name, Name))
                {
                    return true;
                }
            }

            return false;
        }

        // Reads to the next sibling of the current element with the given Name.
        public bool ReadToNextSibling(string name)
        {
            if (name == null || name.Length == 0)
                throw XmlConvert.CreateInvalidNameArgumentException(name, "name");

            // find the next sibling
            ClrMameProNodeType nt;
            do
            {
                if (!SkipSubtree())
                    break;

                nt = NodeType;
                if (nt == ClrMameProNodeType.Element && string.Equals(name, Name))
                    return true;

            } while (nt != ClrMameProNodeType.EndElement && !EOF);

            return false;
        }

        // Returns the inner content (including markup) of an element or attribute as a string.
        public string ReadInner()
        {
            if (ReadState != ReadState.Interactive)
                return string.Empty;

            if ((this.NodeType != ClrMameProNodeType.Attribute) && (this.NodeType != ClrMameProNodeType.Element))
            {
                Read();
                return string.Empty;
            }

            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            ClrMameProWriter cmpw = CreateWriterForInnerOuterXml(sw);

            try
            {
                if (this.NodeType == ClrMameProNodeType.Attribute)
                {
                    cmpw.Quotes = this.QuoteChar == default(char);
                    WriteAttributeValue(cmpw);
                }

                if (this.NodeType == ClrMameProNodeType.Element)
                    this.WriteNode(cmpw, false);
            }
            finally
            {
                cmpw.Close();
            }

            return sw.ToString();
        }

        // Writes the content (inner) of the current node into the provided ClrMameProWriter.
        private void WriteNode(ClrMameProWriter cmpw)
        {
            int d = this.NodeType == ClrMameProNodeType.None ? -1 : this.Depth;
            while (this.Read() && (d < this.Depth))
            {
                switch (this.NodeType)
                {
                    case ClrMameProNodeType.Element:
                        cmpw.WriteStartElement(this.LocalName);
                        cmpw.Quotes = this.QuoteChar != default(char);
                        if (this.IsEmptyElement)
                            cmpw.WriteEndElement();

                        break;

                    case ClrMameProNodeType.Text:
                        cmpw.WriteString(this.Value);
                        break;

                    case ClrMameProNodeType.Standalone:
                        cmpw.WriteStandalone(this.LocalName, this.Value, this.QuoteChar != default(char));
                        break;

                    case ClrMameProNodeType.EndElement:
                        cmpw.WriteFullEndElement();
                        break;
                }
            }

            if (d == this.Depth && this.NodeType == ClrMameProNodeType.EndElement)
                Read();
        }

        // Writes the attribute into the provided ClrMameProWriter.
        private void WriteAttributeValue(ClrMameProWriter cmpw)
        {
            string attrName = this.Name;
            while (ReadAttributeValue())
            {
                cmpw.WriteString(this.Value);
            }

            this.MoveToAttribute(attrName);
        }

        // Returns the current element and its descendants or an attribute as a string.
        public virtual string ReadOuterXml()
        {
            if (ReadState != ReadState.Interactive)
                return string.Empty;

            if ((this.NodeType != ClrMameProNodeType.Attribute) && (this.NodeType != ClrMameProNodeType.Element))
            {
                Read();
                return string.Empty;
            }

            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            ClrMameProWriter cmpw = CreateWriterForInnerOuterXml(sw);

            try
            {
                if (this.NodeType == ClrMameProNodeType.Attribute)
                {
                    cmpw.WriteStartAttribute(this.LocalName);
                    WriteAttributeValue(cmpw);
                    cmpw.WriteEndAttribute();
                }
            }
            finally
            {
                cmpw.Close();
            }

            return sw.ToString();
        }

        private ClrMameProWriter CreateWriterForInnerOuterXml(StringWriter sw)
        {
            ClrMameProWriter w = new ClrMameProWriter();
            return w;
        }

        // Returns an ClrMameProReader that will read only the current element and its descendants and then go to EOF state.
        public ClrMameProReader ReadSubtree()
        {
            if (NodeType != ClrMameProNodeType.Element)
                throw new InvalidOperationException(Res.GetString(Res.Xml_ReadSubtreeNotOnElement));

            return new ClrMameProSubtreeReader(this);
        }

        // Returns true when the current node has any attributes.
        public bool HasAttributes
        {
            get
            {
                return AttributeCount > 0;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            //the boolean flag may be used by subclasses to differentiate between disposing and finalizing
            if (disposing && ReadState != ReadState.Closed)
                Close();
        }

        // TODO: Figure this one out
        private static internal bool IsTextualNode(ClrMameProNodeType nodeType)
        {
            return 0 != (IsTextualNodeBitmap & (1 << (int)nodeType));
        }

        // TODO: Figure this one out
        private static internal bool CanReadContentAs(ClrMameProNodeType nodeType)
        {
            return 0 != (CanReadContentAsBitmap & (1 << (int)nodeType));
        }

        /// <summary>
        /// TODO: Figure this one out
        /// </summary>
        private static internal bool HasValueInternal(ClrMameProNodeType nodeType)
        {
            return 0 != (HasValueBitmap & (1 << (int)nodeType));
        }

        //SkipSubTree is called whenever validation of the skipped subtree is required on a reader with XsdValidation
        private bool SkipSubtree()
        {
            MoveToElement();
            if (NodeType == ClrMameProNodeType.Element && !IsEmptyElement)
            {
                int depth = Depth;

                // Nothing, just read on
                while (Read() && depth < Depth) ;

                // consume end tag
                if (NodeType == ClrMameProNodeType.EndElement)
                    return Read();
            }
            else
            {
                return Read();
            }

            return false;
        }

        internal Exception CreateReadContentAsException(string methodName)
        {
            return CreateReadContentAsException(methodName, NodeType, this as IXmlLineInfo);
        }

        internal Exception CreateReadElementContentAsException(string methodName)
        {
            return CreateReadElementContentAsException(methodName, NodeType, this as IXmlLineInfo);
        }

        internal bool CanReadContentAs()
        {
            return CanReadContentAs(this.NodeType);
        }

        static internal Exception CreateReadContentAsException(string methodName, ClrMameProNodeType nodeType, IXmlLineInfo lineInfo)
        {
            return new InvalidOperationException(AddLineInfo(Res.GetString(Res.Xml_InvalidReadContentAs, new string[] { methodName, nodeType.ToString() }), lineInfo));
        }

        static internal Exception CreateReadElementContentAsException(string methodName, ClrMameProNodeType nodeType, IXmlLineInfo lineInfo)
        {
            return new InvalidOperationException(AddLineInfo(Res.GetString(Res.Xml_InvalidReadElementContentAs, new string[] { methodName, nodeType.ToString() }), lineInfo));
        }

        static string AddLineInfo(string message, IXmlLineInfo lineInfo)
        {
            if (lineInfo != null)
            {
                string[] lineArgs = new string[2];
                lineArgs[0] = lineInfo.LineNumber.ToString(CultureInfo.InvariantCulture);
                lineArgs[1] = lineInfo.LinePosition.ToString(CultureInfo.InvariantCulture);
                message += " " + Res.GetString(Res.Xml_ErrorPosition, lineArgs);
            }
            return message;
        }

        internal string InternalReadContentAsString()
        {
            string value = string.Empty;
            StringBuilder sb = null;
            do
            {
                switch (this.NodeType)
                {
                    case ClrMameProNodeType.Attribute:
                        return this.Value;

                    case ClrMameProNodeType.Text:
                    case ClrMameProNodeType.Standalone:
                        // merge text content
                        if (value.Length == 0)
                        {
                            value = this.Value;
                        }
                        else
                        {
                            if (sb == null)
                            {
                                sb = new StringBuilder();
                                sb.Append(value);
                            }
                            sb.Append(this.Value);
                        }
                        break;

                    case ClrMameProNodeType.EndElement:
                    default:
                        goto ReturnContent;
                }
            } while ((this.AttributeCount != 0) ? this.ReadAttributeValue() : this.Read());

        ReturnContent:
            return (sb == null) ? value : sb.ToString();
        }

        private bool SetupReadElementContentAsXxx(string methodName)
        {
            if (this.NodeType != ClrMameProNodeType.Element)
                throw CreateReadElementContentAsException(methodName);

            bool isEmptyElement = this.IsEmptyElement;

            // move to content or beyond the empty element
            this.Read();

            if (isEmptyElement)
                return false;

            ClrMameProNodeType nodeType = this.NodeType;
            if (nodeType == ClrMameProNodeType.EndElement)
            {
                this.Read();
                return false;
            }
            else if (nodeType == ClrMameProNodeType.Element)
            {
                throw new XmlException(Res.Xml_MixedReadElementContentAs, string.Empty, this as IXmlLineInfo);
            }

            return true;
        }

        private void FinishReadElementContentAsXxx()
        {
            if (this.NodeType != ClrMameProNodeType.EndElement)
                throw new XmlException(Res.Xml_InvalidNodeType, this.NodeType.ToString());

            this.Read();
        }

        internal static Encoding GetEncoding(ClrMameProReader reader)
        {
            ClrMameProReaderImpl tri = GetXmlTextReaderImpl(reader);
            return tri != null ? tri.Encoding : null;
        }

        private static ClrMameProReaderImpl GetXmlTextReaderImpl(ClrMameProReader reader)
        {
            ClrMameProReaderImpl tri = reader as ClrMameProReaderImpl;
            if (tri != null)
                return tri;

            if (tri != null)
                return tri.Impl;

            return null;
        }

        // Creates an ClrMameProReader according for parsing XML from the given stream.
        public static ClrMameProReader Create(Stream input)
        {
            return Create(input, (ClrMameProReaderSettings)null);
        }

        // Creates an ClrMameProReader according to the settings for parsing XML from the given stream.
        public static ClrMameProReader Create(Stream input, ClrMameProReaderSettings settings)
        {
            if (settings == null)
                settings = new ClrMameProReaderSettings();

            return settings.CreateReader(input);
        }

        // Creates an ClrMameProReader according for parsing XML from the given TextReader.
        public static ClrMameProReader Create(TextReader input)
        {
            return Create(input, (ClrMameProReaderSettings)null);
        }

        // Creates an ClrMameProReader according to the settings for parsing XML from the given TextReader.
        public static ClrMameProReader Create(TextReader input, ClrMameProReaderSettings settings)
        {
            if (settings == null)
                settings = new ClrMameProReaderSettings();

            return settings.CreateReader(input);
        }

        // Creates an ClrMameProReader according to the settings wrapped over the given reader.
        public static ClrMameProReader Create(ClrMameProReader reader, ClrMameProReaderSettings settings)
        {
            if (settings == null)
                settings = new ClrMameProReaderSettings();

            return reader;
        }

        internal static int CalcBufferSize(Stream input)
        {
            // determine the size of byte buffer
            int bufferSize = DefaultBufferSize;
            if (input.CanSeek)
            {
                long len = input.Length;
                if (len < bufferSize)
                    bufferSize = checked((int)len);
                else if (len > MaxStreamLengthForDefaultBufferSize)
                    bufferSize = BiggerBufferSize;
            }

            // return the byte buffer size
            return bufferSize;
        }
    }
}
