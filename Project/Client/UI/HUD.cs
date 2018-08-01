using FairyUGUI.UI;
using Logic;
using UnityEngine;
using View;

namespace Client.UI
{
	public class HUD
	{
		private const float OFFSET_HEIGHT = -120f;

		private CChampion _owner;
		public CChampion owner
		{
			get => this._owner;
			set
			{
				this._owner = value;
				this.root.sortingOrder = CPlayer.instance == this._owner ? 999 : 0;
			}
		}

		private bool _visible = true;
		public bool visible
		{
			get => this._visible;
			set
			{
				if ( this._visible == value )
					return;
				this._visible = value;
				this.root.visible = this._visible;
			}
		}

		public Vector2 position { get; private set; }

		public GComponent root { get; private set; }
		public GComponent arrow { get; private set; }

		public HUD()
		{
			this.root = UIPackage.CreateObject( "battle", "HUD" ).asCom;
			this.arrow = UIPackage.CreateObject( "battle", "Arrow" ).asCom;
			this.arrow.pivot = new Vector2( 0.5f, 0.5f );
		}

		public void Dispose()
		{
			this.arrow.Dispose();
			this.arrow = null;

			this.root.Dispose();
			this.root = null;
		}

		public void Release()
		{
			this._owner = null;
		}

		public void Update()
		{
			Vector3 ownerPos = this.owner.position;
			this.position = this.root.parent.ScreenToLocal(
				this.owner.battle.camera.WorldToScreenPoint(
					new Vector3( ownerPos.x, ownerPos.y + ( float ) this.owner.worldBounds.size.y * 0.5f,
					             ownerPos.z ) ) );
			this.root.position = new Vector2( this.position.x, this.position.y + OFFSET_HEIGHT );
		}

		public void OnEntityAttrChanged( Attr attr, object value )
		{
		}
	}
}