using UnityEngine;
using View.Script;

namespace View.Graphics
{
	public class EffectGraphic : Graphic
	{
		public CEffect owner { get; private set; }

		public ParticleScript particleScript { get; private set; }

		public void OnCreate( CEffect owner, string id )
		{
			this.owner = owner;
			this.OnCreate( this.owner.battle, id );
			ParticleSystem[] pss = this.root.GetComponentsInChildren<ParticleSystem>();
			float maxLength = 0f;
			ParticleSystem maxPs = null;
			int count = pss.Length;
			for ( int i = 0; i < count; i++ )
			{
				ParticleSystem ps = pss[i];
				ParticleSystem.MinMaxCurve lifetime = ps.main.startLifetime;
				float len = Mathf.Max( ps.main.duration, lifetime.constantMax, lifetime.constant, lifetime.curveMax?.length ?? 0, lifetime.curve?.length ?? 0 );
				if ( len <= maxLength )
					continue;
				maxLength = len;
				maxPs = ps;
			}
			if ( maxPs != null )
				this.particleScript = maxPs.gameObject.AddComponent<ParticleScript>();
		}

		public void OnDestroy()
		{
			if ( this.particleScript != null )
			{
				Object.Destroy( this.particleScript );
				this.particleScript = null;
			}
			this.OnDestroy( this.owner.battle );
			this.owner = null;
		}
	}
}