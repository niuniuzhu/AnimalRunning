using UnityEngine;

namespace View.Script
{
	public class ParticleScript : MonoBehaviour, IEffectHolder
	{
		public EffectHolderDestroied destroyNotifier { private get; set; }

		public void OnParticleSystemStopped()
		{
			this.destroyNotifier?.Invoke( this );
		}
	}
}