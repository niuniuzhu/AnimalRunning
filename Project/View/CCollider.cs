using Logic;

namespace View
{
	public class CCollider : CEntity
	{
		public Core.FMath.FBounds bounds => this._data.bounds;

		public Core.FMath.FBounds worldBounds { get; private set; }

		protected override void OnAttrChanged( Attr attr, object value )
		{
			switch ( attr )
			{
				case Attr.Bounds:
					this.worldBounds = ( Core.FMath.FBounds )value;
					break;
			}
			base.OnAttrChanged( attr, value );
		}
	}
}