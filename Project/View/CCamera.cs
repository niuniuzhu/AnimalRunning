using Logic;
using UnityEngine;
using View.Misc;

namespace View
{
	public class CCamera
	{
		private Vector3 _offset;
		public Vector3 offset
		{
			get => this._offset;
			set
			{
				if ( this._offset == value )
					return;
				this._offset = value;
				this.UpdateVisualImmediately();
			}
		}

		public Transform target;

		public bool enable
		{
			get => this._camera.enabled;
			set => this._camera.enabled = value;
		}

		public Camera camera => this._camera;
		public Vector3 position => this._camera.transform.position;
		public Quaternion rotation => this._camera.transform.rotation;
		public float smoothTime;

		private readonly Vector3 _lookAtOffset;
		private Transform _cameraTr;
		private Camera _camera;
		private AudioSource _audioSource;
		private Transform _seeker;
		private Vector3[] _seekerConstraints;
		private Vector3 _velocity;

		public CCamera( MapData data )
		{
			this._seeker = new GameObject( "Seeker" ).transform;
			Object.DontDestroyOnLoad( this._seeker.gameObject );

			this._cameraTr = Object.Instantiate( Resources.Load<GameObject>( "Prefabs/Camera" ) ).transform;
			Object.DontDestroyOnLoad( this._cameraTr.gameObject );
			this._camera = this._cameraTr.GetComponent<Camera>();
			this._audioSource = this._cameraTr.GetComponent<AudioSource>();
			this._audioSource.loop = true;

			this._offset = data.camOffset.ToVector3();
			this.smoothTime = ( float )data.camSmooth;
			this._lookAtOffset = data.camLookAtOffset.ToVector3();
		}

		public void Dispose()
		{
			Object.Destroy( this._seeker.gameObject );
			this._seeker = null;
			this._cameraTr = null;
			this._audioSource = null;
			Object.Destroy( this._camera.gameObject );
			this._camera = null;
			this._seekerConstraints = null;
		}

		public void SetConstraints( Vector3 min, Vector3 max )
		{
			this._seekerConstraints = new Vector3[2];
			this._seekerConstraints[0] = min;
			this._seekerConstraints[1] = max;
		}

		public Vector3 WorldToScreenPoint( Vector3 worldPoint )
		{
			return this._camera.WorldToScreenPoint( worldPoint );
		}

		public Vector3 WorldToViewportPoint( Vector3 worldPoint )
		{
			return this._camera.WorldToViewportPoint( worldPoint );
		}

		public Ray ScreenPointToRay( Vector3 screenPos )
		{
			return this._camera.ScreenPointToRay( screenPos );
		}

		public void Play( string id )
		{
			AudioClip audioClip = Resources.Load<AudioClip>( "Sounds/" + id );
			this._audioSource.clip = audioClip;
			this._audioSource.Play();
		}

		public void PlayDelay( string id, float delay )
		{
			AudioClip audioClip = Resources.Load<AudioClip>( "Sounds/" + id );
			this._audioSource.clip = audioClip;
			this._audioSource.PlayDelayed( delay );
		}

		public void PlayOneShot( string id, float volumeScale )
		{
			this._audioSource.PlayOneShot( Resources.Load<AudioClip>( "Sounds/" + id ), volumeScale );
		}

		public void Stop()
		{
			this._audioSource.Stop();
		}

		public void UpdateVisualImmediately()
		{
			this._seeker.position = this.target.position;
			this._cameraTr.position = this._seeker.position + this.offset;
			this._cameraTr.LookAt( this._seeker.position + this._lookAtOffset );
		}

		private void UpdateVisual( UpdateContext context )
		{
			Vector3 seekerPos = Vector3.SmoothDamp( this._seeker.position, this.target?.position ?? this._seeker.position, ref this._velocity,
														this.smoothTime, Mathf.Infinity, ( float )context.deltaTime );
			if ( this._seekerConstraints != null )
				VectorHelper.Clamp( ref seekerPos, this._seekerConstraints[0], this._seekerConstraints[1] );
			this._seeker.position = seekerPos;
			this._cameraTr.position = this._seeker.position + this.offset;
			this._cameraTr.LookAt( this._seeker.position + this._lookAtOffset );
		}

		public void Update( UpdateContext context )
		{
			this.UpdateVisual( context );
		}
	}
}