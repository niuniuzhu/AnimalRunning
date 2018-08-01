using UnityEngine;

namespace View.Graphics
{
	public sealed class EntityGraphic : Graphic
	{
		private const string MATERIAL_PATH = "Materials/Skin/";

		public CEntity owner { get; private set; }
		public AnimatorProxy animator { get; private set; }
		public AudioSourceProxy audioSource { get; private set; }
		public GraphicMaterial material { get; private set; }

		public EntityGraphic()
		{
			this.animator = new AnimatorProxy();
			this.audioSource = new AudioSourceProxy();
			this.material = new GraphicMaterial();
		}

		internal override void Dispose()
		{
			this.animator.Dispose();
			this.animator = null;
			this.audioSource.Dispose();
			this.audioSource = null;
			this.material.Dispose();
			this.material = null;
			base.Dispose();
		}

		public void OnCreate( CEntity owner, string id, byte skin )
		{
			if ( skin != 0xff )
			{
				int actionId = skin >> 4;
				id = $"{id}_{actionId}";
			}
			this.owner = owner;
			this.OnCreate( this.owner.battle, id );
			this.animator.OnCreate( this.model.GetComponent<Animator>() );
			this.audioSource.OnCreate( this.model.GetComponent<AudioSource>() );

			Renderer renderer;
			Transform skinTr = this.model.Find( "Skin" );
			if ( skinTr != null )
				renderer = skinTr.GetComponent<Renderer>();
			else
				renderer = this.model.GetComponentInChildren<Renderer>();
			if ( skin != 0xff )
			{
				int imageId = skin & 0xF;
				Material resources = Resources.Load<Material>( $"{MATERIAL_PATH}{id}_{imageId}" );
				if ( resources == null )
				{
					resources = Resources.Load<Material>( $"{MATERIAL_PATH}failed" );
				}
				renderer.material = Object.Instantiate( resources );
			}
			this.material.OnCreate( renderer );
		}

		public void OnDestroy()
		{
			this.OnDestroy( this.owner.battle );
			this.animator.OnDestroy();
			this.audioSource.OnDestroy();
			this.material.OnDestroy();
			this.owner = null;
		}
	}
}