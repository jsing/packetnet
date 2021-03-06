﻿/*
This file is part of PacketDotNet

PacketDotNet is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PacketDotNet is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with PacketDotNet.  If not, see <http://www.gnu.org/licenses/>.
*/
/*
 *  Copyright 2017 Andrew <pandipd@outlook.com>
 */

using System;
using System.Text;
using System.Collections.Generic;
using MiscUtil.Conversion;
using PacketDotNet.Tcp;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    /// DrdaPacket
    /// See: https://en.wikipedia.org/wiki/DRDA
    /// </summary>
    public class DrdaDDMPacket : Packet
    {
#if DEBUG
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
        // NOTE: No need to warn about lack of use, the compiler won't
        //       put any calls to 'log' here but we need 'log' to exist to compile
#pragma warning disable 0169, 0649
        private static readonly ILogInactive log;
#pragma warning restore 0169, 0649
#endif

        /// <summary>
        /// The Length field
        /// </summary>
        public ushort Length
        {
            get
            {
                return BigEndianBitConverter.Big.ToUInt16(header.Bytes, header.Offset + DrdaDDMFields.LengthPosition);
            }
        }

        /// <summary>
        /// The Magic field
        /// </summary>
        public byte Magic
        {
            get
            {
                return header.Bytes[header.Offset + DrdaDDMFields.MagicPosition];
            }
        }

        /// <summary>
        /// The Format field
        /// </summary>
        public byte Format
        {
            get
            {
                return header.Bytes[header.Offset + DrdaDDMFields.FormatPosition];
            }
        }

        /// <summary>
        /// The CorrelId field
        /// </summary>
        public ushort CorrelId
        {
            get
            {
                return BigEndianBitConverter.Big.ToUInt16(header.Bytes, header.Offset + DrdaDDMFields.CorrelIdPosition);
            }
        }

        /// <summary>
        /// The Length2 field
        /// </summary>
        public ushort Length2
        {
            get
            {
                return BigEndianBitConverter.Big.ToUInt16(header.Bytes, header.Offset + DrdaDDMFields.Length2Position);
            }
        }

        /// <summary>
        /// The Code Point field
        /// </summary>
        public DrdaCodepointType CodePoint
        {
            get
            {
                return (DrdaCodepointType)BigEndianBitConverter.Big.ToUInt16(header.Bytes, header.Offset + DrdaDDMFields.CodePointPosition);
            }
        }

        private List<DrdaDDMParameter> paramters;

        /// <summary>
        /// Decode Parameters field
        /// </summary>
        public List<DrdaDDMParameter> Parameters
        {
            get
            {
                if (paramters == null) paramters = new List<DrdaDDMParameter>();
                if (paramters.Count > 0) return paramters;
                var offset = header.Offset + DrdaDDMFields.DDMHeadTotalLength;
                var ddmTotalLength = this.Length;
                while (offset < header.Offset+ ddmTotalLength)
                {
                    int length = BigEndianBitConverter.Big.ToUInt16(header.Bytes, offset);
                    if (length == 0)
                    {
                        length = header.Offset + ddmTotalLength - offset;
                    }
                    if (offset + length <= header.Offset + ddmTotalLength)
                    {
                        var parameter = new DrdaDDMParameter()
                        {
                            Length = length,
                            DrdaCodepoint = (DrdaCodepointType)BigEndianBitConverter.Big.ToUInt16(header.Bytes, offset + DrdaDDMFields.ParameterLengthLength)
                        };

                        var startIndex = offset + DrdaDDMFields.ParameterLengthLength + DrdaDDMFields.ParameterCodePointLength;
                        var strLength = length - 4;
                        //For Type=Data or Type=QryDta,Decode bytes as utf-8 ascii string
                        if (parameter.DrdaCodepoint== DrdaCodepointType.DATA|| parameter.DrdaCodepoint == DrdaCodepointType.QRYDTA)
                        {
                            startIndex++;
                            strLength-=2;
                            parameter.Data = ASCIIEncoding.UTF8.GetString(header.Bytes, startIndex,strLength).Trim();
                        }
                        else
                        {
                            parameter.Data = StringConverter.EbcdicToAscii(header.Bytes, startIndex, strLength).Trim();
                        }
                        paramters.Add(parameter);
                    }
                    offset += length;
                }
                return paramters;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bas">Payload Bytes</param>
        public DrdaDDMPacket(ByteArraySegment bas)
        {
            log.Debug("");

            // set the header field, header field values are retrieved from this byte array
            header = new ByteArraySegment(bas);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bas">Payload Bytes</param>
        /// <param name="ParentPacket">Parent Packet</param>
        public DrdaDDMPacket(ByteArraySegment bas, Packet ParentPacket) : this(bas)
        {
            log.DebugFormat("ParentPacket.GetType() {0}", ParentPacket.GetType());

            this.ParentPacket = ParentPacket;
        }

        /// <summary cref="Packet.ToString(StringOutputType)" />
        public override string ToString(StringOutputType outputFormat)
        {
            var buffer = new StringBuilder();
            string color = "";
            string colorEscape = "";

            if (outputFormat == StringOutputType.Colored || outputFormat == StringOutputType.VerboseColored)
            {
                color = Color;
                colorEscape = AnsiEscapeSequences.Reset;
            }
            if (outputFormat == StringOutputType.Normal || outputFormat == StringOutputType.Colored)
            {
                // build the output string
                buffer.AppendFormat("{0}[DrdaDDMPacket: Length={2}, Magic=0x{3:x2}, Format=0x{4:x2}, CorrelId={5}, Length2={6}, CodePoint={7}]{1}",
                    color,
                    colorEscape,
                    Length,
                    Magic,
                    Format,
                    CorrelId,
                    Length2,
                    CodePoint);
                buffer.Append(" Paramters:{");
                foreach(var paramter in Parameters)
                {
                    buffer.AppendFormat("{0}[DrdaDDMParameter: Length={2}, CodePoint={3}, Data='{4}']{1}",
                    color,
                    colorEscape,
                    paramter.Length,
                    paramter.DrdaCodepoint,
                    paramter.Data);
                }
                buffer.Append("}");
            }

            return buffer.ToString();
        }
    }
}