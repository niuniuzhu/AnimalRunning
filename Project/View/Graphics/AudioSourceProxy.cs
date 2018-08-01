using UnityEngine;

namespace View.Graphics
{
	public class AudioSourceProxy
	{
		private AudioSource _audioSource;

		public void Dispose()
		{
		}

		public void OnCreate( AudioSource audioSource )
		{
			this._audioSource = audioSource;
		}

		public void OnDestroy()
		{
			this._audioSource = null;
		}

		public void Play( ulong delay )
		{
			if ( this._audioSource != null )
				this._audioSource.Play( delay );
		}

		public void PlayOneShot( string id, float volumeScale )
		{
			if ( this._audioSource != null )
				this._audioSource.PlayOneShot( Resources.Load<AudioClip>( "Sounds/" + id ), volumeScale );
		}

		public void Stop()
		{
			if ( this._audioSource != null )
				this._audioSource.Stop();
		}
	}
}