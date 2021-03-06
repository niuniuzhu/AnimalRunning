﻿using System.Collections.Generic;
using Core.Net;
using Core.Net.Protocol;

namespace Protocol.Gen
{
	public class _DTO_float : DTO
	{
		public float value;
		
		public _DTO_float(  )
		{
			
		}
public _DTO_float( float value )
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

			this.value = buffer.ReadFloat();
		}

		public override string ToString()
		{
			return $"value:{this.value}";
		}
	}
}