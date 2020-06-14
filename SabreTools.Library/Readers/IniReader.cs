using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SabreTools.Library.Data;

namespace SabreTools.Library.Readers
{
    public class IniReader : IDisposable
    {
        /// <summary>
        /// Internal stream reader for inputting
        /// </summary>
        private StreamReader sr;

        /// <summary>
        /// Get if at end of stream
        /// </summary>
        public bool EndOfStream
        {
            get
            {
                return sr?.EndOfStream ?? true;
            }
        }

        /// <summary>
        /// Contents of the currently read line as a key value pair
        /// </summary>
        public KeyValuePair<string, string>? KeyValuePair { get; private set; } = null;

        /// <summary>
        /// Contents of the currently read line
        /// </summary>
        public string Line { get; private set; } = string.Empty;

        /// <summary>
        /// Current row type
        /// </summary>
        public IniRowType RowType { get; private set; } = IniRowType.None;

        /// <summary>
        /// Current section being read
        /// </summary>
        public string Section { get; private set; } = string.Empty;

        /// <summary>
        /// Validate that rows are in key=value format
        /// </summary>
        public bool ValidateRows { get; set; } = true;

        /// <summary>
        /// Constructor for reading from a file
        /// </summary>
        public IniReader(string filename)
        {
            sr = new StreamReader(filename);
        }

        /// <summary>
        /// Constructor for reading from a stream
        /// </summary>
        public IniReader(Stream stream, Encoding encoding)
        {
            sr = new StreamReader(stream, encoding);
        }

        /// <summary>
        /// Read the next line in the INI file
        /// </summary>
        public bool ReadNextLine()
        {
            if (!(sr.BaseStream?.CanRead ?? false) || sr.EndOfStream)
                return false;

            Line = sr.ReadLine().Trim();
            ProcessLine();
            return true;
        }

        /// <summary>
        /// Process the current line and extract out values
        /// </summary>
        private void ProcessLine()
        {
            // Comment
            if (Line.StartsWith(";"))
            {
                KeyValuePair = null;
                RowType = IniRowType.Comment;
            }

            // Section
            else if (Line.StartsWith("[") && Line.EndsWith("]"))
            {
                KeyValuePair = null;
                RowType = IniRowType.SectionHeader;
                Section = Line.TrimStart('[').TrimEnd(']');
            }

            // KeyValuePair
            else if (Line.Contains("="))
            {
                // Split the line by '=' for key-value pairs
                string[] data = Line.Split('=');

                // If the value field contains an '=', we need to put them back in
                string key = data[0].Trim();
                string value = string.Join("=", data.Skip(1)).Trim();

                KeyValuePair = new KeyValuePair<string, string>(key, value);
                RowType = IniRowType.KeyValue;
            }

            // Empty
            else if (string.IsNullOrEmpty(Line))
            {
                KeyValuePair = null;
                Line = string.Empty;
                RowType = IniRowType.None;
            }

            // Invalid
            else
            {
                KeyValuePair = null;
                RowType = IniRowType.Invalid;

                if (ValidateRows)
                    throw new InvalidDataException($"Invalid INI row found, cannot continue: {Line}");
            }
        }

        /// <summary>
        /// Dispose of the reader
        /// </summary>
        public void Dispose()
        {
            sr.Dispose();
        }
    }
}
