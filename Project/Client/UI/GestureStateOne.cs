using DG.Tweening;
using FairyUGUI.Event;
using FairyUGUI.UI;
using UnityEngine;

namespace Client.UI
{
	public class GestureStateOne
	{
		private const float TIME_TO_SHOW_JOYSTICK = 0.5f;

		private Joystick _joystick;
		public Joystick joystick
		{
			get => this._joystick;
			set
			{
				if ( this._joystick == value )
					return;

				this._joystick = value;
				if ( this._joystick != null )
				{
					this._joystick.touchable = false;
					this._joystick.radius = this._joystick.size.x * 0.5f;
					this._joystick.resetDuration = 0.2f;
					this._joystick.coreName = "core";
					this._joystick.onChanged.Add( this.OnJoystickChanged );
				}
			}
		}

		private bool _active;
		private float _touchTime;
		private Vector2 _touchPosition;
		private UIBattle _owner;

		public GestureStateOne( UIBattle uiBattle )
		{
			this._owner = uiBattle;
		}

		public void Dispose()
		{
			DOTween.Kill( this._joystick );
			this._joystick = null;
			this._owner = null;
		}

		public void OnTouchBegin( Vector2 point )
		{
			this._active = true;
			this._touchTime = 0f;
			this._touchPosition = point;
		}

		public void OnTouchEnd( Vector2 point )
		{
			this._active = false;
			this.HideJoystick();
		}

		public void OnDrag( Vector2 point )
		{
			this._active = true;
			this.ShowJoystick( this._touchPosition );
			this._joystick.touchPosition = this._joystick.ScreenToLocal( point );
		}

		private void ShowJoystick( Vector2 point )
		{
			this._joystick.visible = true;
			point = this._joystick.parent.ScreenToLocal( point );
			this._joystick.position = new Vector2( point.x - this._joystick.size.x * 0.5f, point.y - this._joystick.size.y * 0.5f );
			this._joystick.TweenFade( 1f, 0.2f ).SetTarget( this._joystick );
		}

		private void HideJoystick()
		{
			this._joystick.Reset( true );
			this._joystick.TweenFade( 0f, 0.2f ).SetTarget( this._joystick ).OnComplete( this.OnJoystickHideComplete );
		}

		private void OnJoystickHideComplete()
		{
			this._joystick.visible = false;
		}

		public void Update()
		{
			if ( !this._active )
				return;

			if ( !this._joystick.visible )
			{
				this._touchTime += Time.deltaTime;
				if ( this._touchTime >= TIME_TO_SHOW_JOYSTICK )
				{
					this.ShowJoystick( this._touchPosition );
					this._joystick.Reset();
				}
			}
		}

		private void OnJoystickChanged( EventContext context )
		{
			this._owner.HandleAxisInput( this._joystick.worldAxis );
		}
	}
}