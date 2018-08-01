using System;

namespace Logic
{
	[AttributeUsage( AttributeTargets.Property )]
	public class SyncAttribute : Attribute
	{
		public Attr attr { get; private set; }

		public SyncAttribute( Attr attr )
		{
			this.attr = attr;
		}
	}
}