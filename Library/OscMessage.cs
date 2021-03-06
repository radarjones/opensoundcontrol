﻿// Copyright (C) Mark Alan Jones 2010
// This code is published under the Microsoft Public License (Ms-Pl)
// A copy of the Ms-Pl license is included with the source and 
// binary distributions or online at
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenSoundControl
{
    /// <summary>
    ///   Encapsulates a message.
    /// </summary>
    public class OscMessage : IOscElement
    {
        private List<IOscElement> _arguments;

        ///<summary>
        ///  Creates an empty message.
        ///</summary>
        public OscMessage()
        {
            Arguments = new List<IOscElement>();
        }

        /// <summary>
        ///   Creates a message from the input string
        /// </summary>
        public OscMessage(string value)
        {
            Parse(value.Trim());
        }

        /// <summary>
        ///   Gets the message address.
        /// </summary>
        public OscAddress Address { get; set; }

        /// <summary>
        ///   Gets the message type tag string
        /// </summary>
        public OscTypeTagString TypeTagString
        {
            get
            {
                var elementTypes = _arguments.Select(i => i.ElementType);
                return new OscTypeTagString(elementTypes);
            }
        }

        /// <summary>
        ///   Gets and sets the argument list
        /// </summary>
        public List<IOscElement> Arguments
        {
            get { return _arguments; }
            set
            {
                _arguments = value;
                // filter out non argument elements
                _arguments = _arguments.Where(i => i.IsArgument).ToList();
            }
        }

        /// <summary>
        ///   Parses the input string to build an OSC message.
        /// </summary>
        private void Parse(string value)
        {
            Address = ParseAddress(value);
            Arguments = ParseArguments(value);
        }

        /// <summary>
        ///   Parses out an OSC address from the input string
        /// </summary>
        private static OscAddress ParseAddress(string value)
        {
            Match match = Regex.Match(value, @"^(/[\w/]*)");
            if (match.Captures.Count != 1)
            {
                throw new ArgumentException("OSC address not found in input string");
            }
            return new OscAddress(match.Captures[0].ToString());
        }


        /// <summary>
        ///   Parses the input string for arguments.
        /// </summary>
        private static List<IOscElement> ParseArguments(string value)
        {
            var list = new List<IOscElement>();
            MatchCollection matches = Regex.Matches(value, @"\s+(\w+)");
            foreach (string input in
                from Match match in matches
                where match.Captures.Count > 0
                select match.Captures[0].ToString())
            {
                float f;
                if (TryParseFloat32(input, out f))
                {
                    list.Add(new OscFloat32(f));
                    continue;
                }

                int i;
                if (TryParseInt32(input, out i))
                {
                    list.Add(new OscInt32(i));
                    continue;
                }

                uint ui;
                if (TryParseUInt32(input, out ui))
                {
                    list.Add(new OscUInt32(ui));
                    continue;
                }

                // all else has failed add it as a string
                list.Add(new OscString(input));
            }
            return list;
        }

        private static bool TryParseFloat32(string input,
                                            out float output)
        {
            Match match = Regex.Match(input, @"(\d*[.]\d+)");
            if (match.Captures.Count > 0)
            {
                return float.TryParse(match.Captures[0].ToString(), out output);
            }
            output = float.NaN;
            return false;
        }

        private static bool TryParseInt32(string input,
                                          out int output)
        {
            MatchCollection matches = Regex.Matches(input, @"([-]*\d+)");
            if (matches.Count > 0 && matches[0].Captures.Count > 0)
            {
                return int.TryParse(matches[0].Captures[0].ToString(), out output);
            }
            output = 0;
            return false;
        }

        private static bool TryParseUInt32(string input,
                                           out uint output)
        {
            MatchCollection matches = Regex.Matches(input, @"(\d+)");
            if (matches.Count > 0 && matches[0].Captures.Count > 0)
            {
                return uint.TryParse(matches[0].Captures[0].ToString(), out output);
            }
            output = 0;
            return false;
        }

        /// <summary>
        ///   Returns a &lt;see cref = "T:System.String" /&gt; that represents the current &lt;see cref = "T:System.Object" /&gt;.
        /// </summary>
        /// <returns>
        ///   A <see cref = "T:System.String" /> that represents the current <see cref = "T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Address.ToString());
            foreach (IOscElement argument in _arguments)
            {
                sb.Append(' ');
                sb.Append(argument.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        ///   Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///   A hash code for the current <see cref = "T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            int hash = Address.GetHashCode();
            hash ^= TypeTagString.GetHashCode();
            Arguments.ForEach(i => hash ^= i.GetHashCode());
            return hash;
        }

        /// <summary>
        ///   Determines whether the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />.
        /// </summary>
        /// <returns>
        ///   true if the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />; otherwise, false.
        /// </returns>
        /// <param name = "obj">The <see cref = "T:System.Object" /> to compare with the current <see cref = "T:System.Object" />. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj != null && obj is OscMessage)
            {
                OscMessage other = obj as OscMessage;

                if (Address != other.Address || TypeTagString != other.TypeTagString)
                    return false;

                return !Arguments.Where((t,
                                         i) => !t.Equals(other.Arguments[i])).Any();
            }
            return false;
        }

        /// <summary>
        ///   Compares the value of two OscMessage and determines if they are equal
        /// </summary>
        /// <param name = "m1">message one</param>
        /// <param name = "m2">message two</param>
        /// <returns></returns>
        public static bool operator ==(OscMessage m1,
                                       OscMessage m2)
        {
            if (ReferenceEquals(m1, null))
                throw new ArgumentNullException("m1");

            if (ReferenceEquals(m2, null))
                throw new ArgumentNullException("m2");

            return m1.Equals(m2);
        }

        /// <summary>
        ///   Compares the value of two OscMessage and determines if they are not equal
        /// </summary>
        /// <param name = "m1">message one</param>
        /// <param name = "m2">message two</param>
        /// <returns></returns>
        public static bool operator !=(OscMessage m1,
                                       OscMessage m2)
        {
            if (ReferenceEquals(m1, null))
                throw new ArgumentNullException("m1");

            if (ReferenceEquals(m2, null))
                throw new ArgumentNullException("m2");

            return !m1.Equals(m2);
        }

        #region Implementation of IOscElement

        /// <summary>
        ///   Gets the element type.
        /// </summary>
        public OscElementType ElementType
        {
            get { return OscElementType.Message; }
        }

        /// <summary>
        ///   True if the element is also an argument
        /// </summary>
        public bool IsArgument
        {
            get { return false; }
        }

        /// <summary>
        ///   Gets the packet array data for the element.
        /// </summary>
        public byte[] ToOscPacketArray()
        {
            List<byte> buffer = new List<byte>();

            buffer.AddRange(Address.ToOscPacketArray());
            buffer.AddRange(TypeTagString.ToOscPacketArray());
            _arguments.ForEach(i => buffer.AddRange(i.ToOscPacketArray()));
            return buffer.ToArray();
        }

        #endregion
    }
}
