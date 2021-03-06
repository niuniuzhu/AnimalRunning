﻿using System.Collections.Generic;
using Core.Net;
using Core.Net.Protocol;

namespace Protocol.Gen
{
	public class _DTO_ulong : DTO
	{
		public ulong value;
		
		public _DTO_ulong(  )
		{
			
		}
public _DTO_ulong( ulong value )
		{
			this.value = value;
		}

		protected override void InternalSerialize( StreamBuffer buffer )
		{
			base.InternalSerialize( buffer );

			buffer.Write( this.value );
		}

		protected override void InternalDeserialize( StreamBuffer buffer )
		{
			base.InternalDeserialize( buffer );

			this.value = buffer.ReadULong();
		}

		public override string ToString()
		{
			return $"value:{this.value}";
		}
	}
}