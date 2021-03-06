﻿using Core.Net;
using Core.Net.Protocol;

namespace Protocol.Gen
{
	[Packet( 103, 32000 )]
	public class _PACKET_ROOM_ACMD_LEAVE_ROOM : Packet
	{
		

		public _PACKET_ROOM_ACMD_LEAVE_ROOM() : base( 103, 32000 )
		{
		}

		

		protected override void InternalSerialize( StreamBuffer buffer )
		{
			base.InternalSerialize( buffer );
			
		}

		protected override void InternalDeserialize( StreamBuffer buffer )
		{
			base.InternalDeserialize( buffer );
			
		}

		public override string ToString()
		{
			return $"module:{this.module}, cmd:{this.command}";
		}
	}
}