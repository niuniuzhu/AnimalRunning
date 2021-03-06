﻿using System.Collections.Generic;
using Core.Net;
using Core.Net.Protocol;

namespace Protocol.Gen
{
	public class _DTO_double : DTO
	{
		public double value;
		
		public _DTO_double(  )
		{
			
		}
public _DTO_double( double value )
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

			this.value = buffer.ReadDouble();
		}

		public override string ToString()
		{
			return $"value:{this.value}";
		}
	}
}