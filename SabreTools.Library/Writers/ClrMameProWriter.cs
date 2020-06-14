using System;
using System.IO;
using System.Text;

namespace SabreTools.Library.Writers
{
    /// <summary>
    /// ClrMamePro writer patterned heavily off of XmlTextWriter
    /// </summary>
    /// <see cref="https://referencesource.microsoft.com/#System.Xml/System/Xml/Core/XmlTextWriter.cs"/>
    public class ClrMameProWriter : IDisposable
    {
        /// <summary>
        /// State machine state for use in the table
        /// </summary>
        private enum State
        {
            Start,
            Prolog,
            Element,
            Attribute,
            Content,
            AttrOnly,
            Epilog,
            Error,
            Closed,
        }

        /// <summary>
        /// Potential token types
        /// </summary>
        private enum Token
        {
            None,
            Standalone,
            StartElement,
            EndElement,
            LongEndElement,
            StartAttribute,
            EndAttribute,
            Content,
        }

        /// <summary>
        /// Tag information for the stack
        /// </summary>
        private struct TagInfo
        {
            public string Name;
            public bool Mixed;

            public void Init()
            {
                Name = null;
                Mixed = false;
            }
        }

        /// <summary>
        /// Internal stream writer
        /// </summary>
        private StreamWriter textWriter;

        /// <summary>
        /// Stack for tracking current node
        /// </summary>
        private TagInfo[] stack;
        
        /// <summary>
        /// Pointer to current top element in the stack
        /// </summary>
        private int top;

        /// <summary>
        /// State table for determining the state machine
        /// </summary>
        private readonly State[] stateTable = {
            //                         State.Start      State.Prolog     State.Element    State.Attribute  State.Content   State.AttrOnly   State.Epilog
            //
            /* Token.None           */ State.Prolog,    State.Prolog,    State.Content,   State.Content,   State.Content,  State.Error,     State.Epilog,
            /* Token.Standalone     */ State.Prolog,    State.Prolog,    State.Content,   State.Content,   State.Content,  State.Error,     State.Epilog,
            /* Token.StartElement   */ State.Element,   State.Element,   State.Element,   State.Element,   State.Element,  State.Error,     State.Element,
            /* Token.EndElement     */ State.Error,     State.Error,     State.Content,   State.Content,   State.Content,  State.Error,     State.Error,
            /* Token.LongEndElement */ State.Error,     State.Error,     State.Content,   State.Content,   State.Content,  State.Error,     State.Error,
            /* Token.StartAttribute */ State.AttrOnly,  State.Error,     State.Attribute, State.Attribute, State.Error,    State.Error,     State.Error,
            /* Token.EndAttribute   */ State.Error,     State.Error,     State.Error,     State.Element,   State.Error,    State.Epilog,    State.Error,
            /* Token.Content        */ State.Content,   State.Content,   State.Content,   State.Attribute, State.Content,  State.Attribute, State.Epilog,
        };

        /// <summary>
        /// Current state in the machine
        /// </summary>
        private State currentState;

        /// <summary>
        /// Last seen token
        /// </summary>
        private Token lastToken;

        /// <summary>
        /// Get if quotes should surround attribute values
        /// </summary>
        public bool Quotes { get; set; }

        /// <summary>
        /// Constructor for opening a write from a file
        /// </summary>
        public ClrMameProWriter(string filename)
        {
            textWriter = new StreamWriter(filename);
            Quotes = true;
            stack = new TagInfo[10];
            top = 0;
        }

        /// <summary>
        /// Constructor for opening a write from a stream and encoding
        /// </summary>
        public ClrMameProWriter(Stream stream, Encoding encoding)
        {
            textWriter = new StreamWriter(stream, encoding);
            Quotes = true;
            
            // Element stack
            stack = new TagInfo[10];
            top = 0;
            stack[top].Init();
        }

        /// <summary>
        /// Base stream for easy access
        /// </summary>
        public Stream BaseStream
        {
            get { return textWriter?.BaseStream ?? null; }
        }

        /// <summary>
        /// Write the start of an element node
        /// </summary>
        public void WriteStartElement(string name)
        {
            try
            {
                AutoComplete(Token.StartElement);
                PushStack();
                stack[top].Name = name;
                textWriter.Write(name);
                textWriter.Write(" (");
            }
            catch
            {
                currentState = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Write the end of an element node
        /// </summary>
        public void WriteEndElement()
        {
            InternalWriteEndElement(false);
        }

        /// <summary>
        /// Write the end of a mixed element node
        /// </summary>
        public void WriteFullEndElement()
        {
            InternalWriteEndElement(true);
        }

        /// <summary>
        /// Write a complete element with content
        /// </summary>
        public void WriteElementString(string name, string value)
        {
            WriteStartElement(name);
            WriteString(value);
            WriteEndElement();
        }

        /// <summary>
        /// Write the start of an attribute node
        /// </summary>
        public void WriteStartAttribute(string name)
        {
            try
            {
                AutoComplete(Token.StartAttribute);
                textWriter.Write(name);
                textWriter.Write(" ");
                if (Quotes)
                    textWriter.Write("\"");
            }
            catch
            {
                currentState = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Write the end of an attribute node
        /// </summary>
        public void WriteEndAttribute()
        {
            try
            {
                AutoComplete(Token.EndAttribute);
            }
            catch
            {
                currentState = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Write a complete attribute with content
        /// </summary>
        public void WriteAttributeString(string name, string value)
        {
            WriteStartAttribute(name);
            WriteString(value);
            WriteEndAttribute();
        }

        /// <summary>
        /// Write a standalone attribute
        /// </summary>
        public void WriteStandalone(string name, string value, bool? quoteOverride = null)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException();

                AutoComplete(Token.Standalone);
                textWriter.Write(name);
                textWriter.Write(" ");
                if ((quoteOverride == null && Quotes)
                    || (quoteOverride == true))
                {
                    textWriter.Write("\"");
                }
                textWriter.Write(value);
                if ((quoteOverride == null && Quotes)
                    || (quoteOverride == true))
                {
                    textWriter.Write("\"");
                }
            }
            catch
            {
                currentState = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Write a string content value
        /// </summary>
        public void WriteString(string value)
        {
            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    AutoComplete(Token.Content);
                    textWriter.Write(value);
                }
            }
            catch
            {
                currentState = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Close the writer
        /// </summary>
        public void Close()
        {
            try
            {
                AutoCompleteAll();
            }
            catch
            {
                // Don't fail at this step
            }
            finally
            {
                currentState = State.Closed;
                textWriter.Close();
            }
        }

        /// <summary>
        /// Close and dispose
        /// </summary>
        public void Dispose()
        {
            Close();
            textWriter.Dispose();
        }

        /// <summary>
        /// Flush the base TextWriter
        /// </summary>
        public void Flush()
        {
            textWriter.Flush();
        }

        /// <summary>
        /// Prepare for the next token to be written
        /// </summary>
        private void AutoComplete(Token token)
        {
            // Handle the error cases
            if (currentState == State.Closed)
                throw new InvalidOperationException();
            else if (currentState == State.Error)
                throw new InvalidOperationException();

            State newState = stateTable[(int)token * 7 + (int)currentState];
            if (newState == State.Error)
                throw new InvalidOperationException();

            // TODO: Figure out how to get attributes on their own lines ONLY if an element contains both attributes and elements
            switch (token)
            {
                case Token.StartElement:
                case Token.Standalone:
                    if (currentState == State.Attribute)
                    {
                        WriteEndAttributeQuote();
                        WriteEndStartTag(false);
                    }
                    else if (currentState == State.Element)
                    {
                        WriteEndStartTag(false);
                    }

                    if (currentState != State.Start)
                        Indent(false);

                    break;

                case Token.EndElement:
                case Token.LongEndElement:
                    if (currentState == State.Attribute)
                        WriteEndAttributeQuote();

                    if (currentState == State.Content)
                        token = Token.LongEndElement;
                    else
                        WriteEndStartTag(token == Token.EndElement);

                    break;

                case Token.StartAttribute:
                    if (currentState == State.Attribute)
                    {
                        WriteEndAttributeQuote();
                        textWriter.Write(' ');
                    }
                    else if (currentState == State.Element)
                    {
                        textWriter.Write(' ');
                    }

                    break;

                case Token.EndAttribute:
                    WriteEndAttributeQuote();
                    break;

                case Token.Content:
                    if (currentState == State.Element && lastToken != Token.Content)
                        WriteEndStartTag(false);

                    if (newState == State.Content)
                        stack[top].Mixed = true;

                    break;

                default:
                    throw new InvalidOperationException();
            }

            currentState = newState;
            lastToken = token;
        }

        /// <summary>
        /// Autocomplete all open element nodes
        /// </summary>
        private void AutoCompleteAll()
        {
            while (top > 0)
            {
                WriteEndElement();
            }
        }

        /// <summary>
        /// Internal helper to write the end of an element
        /// </summary>
        private void InternalWriteEndElement(bool longFormat)
        {
            try
            {
                if (top <= 0)
                    throw new InvalidOperationException();

                AutoComplete(longFormat ? Token.LongEndElement : Token.EndElement);
                if (this.lastToken == Token.LongEndElement)
                {
                    Indent(true);
                    textWriter.Write(')');
                }

                top--;
            }
            catch
            {
                currentState = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Internal helper to write the end of a tag
        /// </summary>
        private void WriteEndStartTag(bool empty)
        {
            if (empty)
                textWriter.Write(" )");
        }

        /// <summary>
        /// Internal helper to write the end of an attribute
        /// </summary>
        private void WriteEndAttributeQuote()
        {
            if (Quotes)
                textWriter.Write("\"");
        }

        /// <summary>
        /// Internal helper to indent a node, if necessary
        /// </summary>
        private void Indent(bool beforeEndElement)
        {
            if (top == 0)
            {
                textWriter.WriteLine();
            }
            else if (!stack[top].Mixed)
            {
                textWriter.WriteLine();
                int i = beforeEndElement ? top - 1 : top;
                for (; i > 0; i--)
                {
                    textWriter.Write('\t');
                }
            }
        }

        /// <summary>
        /// Move up one element in the stack
        /// </summary>
        private void PushStack()
        {
            if (top == stack.Length - 1)
            {
                TagInfo[] na = new TagInfo[stack.Length + 10];
                if (top > 0) Array.Copy(stack, na, top + 1);
                stack = na;
            }

            top++; // Move up stack
            stack[top].Init();
        }
    }
}
