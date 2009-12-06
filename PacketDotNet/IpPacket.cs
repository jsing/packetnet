/*
This file is part of Packet.Net

Packet.Net is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Packet.Net is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with Packet.Net.  If not, see <http://www.gnu.org/licenses/>.
*/

﻿using System;
using System.Net;

namespace Packet.Net
{
    //TODO: what layer is this at?
    public abstract class IpPacket : Packet
    {
        protected IPAddress source_ip, dest_ip;

        public abstract IPAddress DestinationAddress
        {
            get;
            set;
        }

        public abstract IPAddress SourceAddress
        {
            get;
            set;
        }
    }

    public class IpV4Packet : IpPacket
    {
        public override byte[] Bytes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IPAddress DestinationAddress
        {
            get
            {
                if (this.dest_ip == null)
                {
                    this.DestinationAddress = new IPAddress(new byte[] { this.Header[16], this.Header[17], this.Header[18], this.Header[19] });
                }
                return this.dest_ip;
            }
            set
            {
                this.dest_ip = value;
            }
        }

        public override IPAddress SourceAddress
        {
            get
            {
                if (this.source_ip == null)
                {
                    this.source_ip = new IPAddress(new byte[] { this.Header[12], this.Header[13], this.Header[14], this.Header[15] }); // Commented these out so I don't forget them
                }
                return this.source_ip;
            }
            set
            {
                this.source_ip = value;
            }
        }

        new internal static Packet Parse(byte[] bytes)
        {
            byte[] header = new byte[20];
            byte[] payload = new byte[bytes.Length - 20];

            Array.Copy(bytes, header, 20);
            Array.Copy(bytes, 20, payload, 0, payload.Length);

            byte protocol = header[9];

            IpV4Packet ipv4_packet = new IpV4Packet();
            throw new System.NotImplementedException();
            //ipv4_packet.PacketHeader(header);
//            ipv4_packet.PayloadData = payload;
//            ipv4_packet.PayloadPacket = (TransportPacket)TransportPacket.Parse((TransportProtocols)protocol, payload);

            return ipv4_packet;
        }
    }
}
