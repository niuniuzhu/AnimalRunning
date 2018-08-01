using Game.Misc;
using UnityEngine;

namespace View.Graphics
{
	public abstract class Graphic
	{
		public string name { get => this.root.name; set => this.root.name = value; }

		public Vector3 position { set => this.root.position = value; get => this.root.position; }

		public Quaternion rotation { set => this.root.rotation = value; get => this.root.rotation; }

		private Vector3 _initScale = Vector3.one;
		public Vector3 initScale
		{
			get => this._initScale;
			set
			{
				if ( this._initScale == value )
					return;
				this._initScale = value;
				this.root.localScale = Vector3.Scale( this._initScale, this._scale );
			}
		}

		private Vector3 _scale = Vector3.one;
		public Vector3 scale
		{
			get => this._scale;
			set
			{
				if ( this._scale == value )
					return;
				this._scale = value;
				this.root.localScale = Vector3.Scale( this._initScale, this._scale );
			}
		}

		private bool _visible;
		public bool visible
		{
			get => this._visible;
			set
			{
				if ( this._visible == value )
					return;

				this._visible = value;
				this.UpdateVisible();
			}
		}

		public Transform root { get; private set; }
		public Transform model { get; private set; }

		protected Graphic()
		{
			this.root = new GameObject().transform;
			Object.DontDestroyOnLoad( this.root.gameObject );
		}

		internal virtual void Dispose()
		{
			Object.Destroy( this.root.gameObject );
			this.root = null;
		}

		private void UpdateVisible()
		{
			this.root.gameObject.SetActive( this._visible );

			//if ( this._visible )
			//	this.root.gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
			//else
			//	this.root.gameObject.hideFlags |= HideFlags.HideInHierarchy;
		}

		protected void OnCreate( CBattle battle, string id )
		{
			this.model = battle.GetModel( id ).transform;
			Utils.AddChild( this.root, this.model, false, true, true );
			this.visible = true;
		}

		protected void OnDestroy( CBattle battle )
		{
			this.model.SetParent( null, false );
			battle.PushModel( this.model.gameObject );
			this.model = null;
			this.scale = Vector3.one;
			this.visible = false;
		}
	}
}