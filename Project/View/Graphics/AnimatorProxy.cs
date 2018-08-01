using System.Collections.Generic;
using Core.Math;
using UnityEngine;

namespace View.Graphics
{
	public class AnimatorProxy
	{
		private Animator _animator;

		private readonly Dictionary<string, AnimationClip> _animationClips = new Dictionary<string, AnimationClip>();

		private float _speed;
		public float speed
		{
			get => this._speed;
			set
			{
				if ( MathUtils.Approximately( this._speed, value ) )
					return;
				this._speed = value;
				if ( this._animator == null )
					return;
				this._animator.speed = value;
			}
		}

		public void Dispose()
		{
		}

		public void OnCreate( Animator animator )
		{
			this._animator = animator;
			if ( this._animator == null )
				return;
			foreach ( AnimationClip animationClip in this._animator.runtimeAnimatorController.animationClips )
				this._animationClips[animationClip.name] = animationClip;
		}

		public void OnDestroy()
		{
			this._animationClips.Clear();
			this._animator = null;
		}

		public float GetClipLength( string name )
		{
			if ( this._animator == null )
				return 0;

			if ( !this._animationClips.TryGetValue( name, out AnimationClip animationClip ) )
				return 0;

			return animationClip.length;
		}

		public void SetBool( string name, bool value )
		{
			if ( this._animator == null )
				return;
			this._animator.SetBool( name, value );
		}

		public void SetFloat( string name, float value )
		{
			if ( this._animator == null )
				return;
			this._animator.SetFloat( name, value );
		}

		public void SetInteger( string name, int value )
		{
			if ( this._animator == null )
				return;
			this._animator.SetInteger( name, value );
		}

		public void CrossFade( string stateName, float normalizedTransitionDuration )
		{
			if ( this._animator == null )
				return;
			this._animator.CrossFade( stateName, normalizedTransitionDuration );
		}
	}
}