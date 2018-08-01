using UnityEngine;

namespace View.Graphics
{
	public class GraphicMaterial
	{
		private readonly Material[] _materialSlot = new Material[5];
		private Renderer _renderer;

		private int _slot;
		public int slot
		{
			get => this._slot;
			set
			{
				if ( this._slot == value )
					return;
				this._slot = value;
				this._renderer.material = this.currentMaterial;
			}
		}

		private Material currentMaterial => this._materialSlot[this._slot];

		private float _alpha = 1;
		public float alpha
		{
			get => this._alpha;
			set
			{
				if ( this._alpha == value )
					return;

				this._alpha = value;
				if ( this._alpha > 0.99f )
					this.slot = 0;
				else
				{
					this.slot = 1;
					Color color = this.currentMaterial.color;
					color.a = this._alpha;
					this.currentMaterial.color = color;
				}
			}
		}

		internal void Dispose()
		{
			int count = this._materialSlot.Length;
			for ( int i = 0; i < count; i++ )
			{
				Material material = this._materialSlot[i];
				if ( material != null )
					Object.Destroy( material );
				this._materialSlot[i] = null;
			}
		}

		internal void OnCreate( Renderer renderer )
		{
			this._renderer = renderer;
			this._materialSlot[0] = this._renderer.material;
			this.slot = 0;
		}

		public void OnDestroy()
		{
			this._renderer = null;
		}
	}
}