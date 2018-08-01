using FairyUGUI.UI;
using Logic;
using System.Collections.Generic;
using UnityEngine;
using View;

namespace Client.UI
{
	public class HUDManager
	{
		private static readonly Stack<HUD> HUD_POOL = new Stack<HUD>();

		private static HUD PopHUD()
		{
			if ( HUD_POOL.Count > 0 )
				return HUD_POOL.Pop();
			return new HUD();
		}

		private static void PushHUD( HUD hud )
		{
			HUD_POOL.Push( hud );
		}

		private readonly Dictionary<string, HUD> _idToHud = new Dictionary<string, HUD>();

		private readonly Queue<CChampion> _delayQueue = new Queue<CChampion>();

		public GComponent root { get; private set; }

		public HUDManager()
		{
			this.root = new GComponent();
			GRoot.inst.AddChild( this.root );
			this.root.displayObject.name = "Hud";
			this.root.touchable = false;
			this.root.size = GRoot.inst.size;
		}

		public void Dispose()
		{
			this._delayQueue.Clear();
			foreach ( KeyValuePair<string, HUD> kv in this._idToHud )
				kv.Value.Dispose();
			this._idToHud.Clear();
			foreach ( HUD hud in HUD_POOL )
				hud.Dispose();
			HUD_POOL.Clear();
			this.root.Dispose();
			this.root = null;
		}

		public void OnEntityCreated( CEntity entity )
		{
			if ( !( entity is CChampion champion ) )
				return;

			//先处理玩家自己，其他实体先放入队列
			if ( CPlayer.instance == null )
				this._delayQueue.Enqueue( champion );
			else
			{
				this.InternalCreate( champion );
				while ( this._delayQueue.Count > 0 )
				{
					champion = this._delayQueue.Dequeue();
					this.InternalCreate( champion );
				}
			}
		}

		private void InternalCreate( CChampion entity )
		{
			HUD hud = PopHUD();
			hud.owner = entity;
			hud.visible = entity.graphic.visible;
			this._idToHud[entity.rid] = hud;
			this.root.AddChild( hud.root );
			this.root.AddChild( hud.arrow );
			hud.arrow.visible = false;
			hud.Update();
		}

		public void OnEntityDestroied( CEntity entity )
		{
			if ( !this._idToHud.TryGetValue( entity.rid, out HUD hud ) )
				return;
			this._idToHud.Remove( entity.rid );
			this.root.RemoveChild( hud.root );
			hud.Release();
			PushHUD( hud );
		}

		public void OnEntityAttrChanged( CEntity target, Attr attr, object value )
		{
			if ( !this._idToHud.TryGetValue( target.rid, out HUD hud ) )
				return;

			hud.OnEntityAttrChanged( attr, value );
		}

		public void Update()
		{
			foreach ( KeyValuePair<string, HUD> kv in this._idToHud )
				kv.Value.Update();

			this.UpdateTeammatePosition();
		}

		private void UpdateTeammatePosition()
		{
			if ( CPlayer.instance == null )
				return;
			Vector2 p0 = this._idToHud[CPlayer.instance.rid].root.position;
			float sizeX = this.root.size.x;
			float sizeY = this.root.size.y;
			foreach ( KeyValuePair<string, HUD> kv in this._idToHud )
			{
				HUD hud = kv.Value;
				CChampion entity = hud.owner;
				if ( entity == CPlayer.instance || entity.team != CPlayer.instance.team )
					continue;
				Vector2 p1 = hud.position;
				if ( p1.x >= 0 && p1.y >= 0 && p1.x <= sizeX && p1.y <= sizeY )
				{
					hud.arrow.visible = false;
					continue;
				}
				hud.arrow.visible = true;
				Vector2 dir = ( p1 - p0 ).normalized;
				float angle = Mathf.Rad2Deg * Mathf.Acos( Vector2.Dot( dir, Vector2.right ) );
				if ( dir.y < 0 )
					angle = -angle;
				hud.arrow.rotation = angle;
				Vector2 cross1 = dir.x > 0
									 ? new Vector2( sizeX , ( sizeX  - p0.x ) * ( p1.y - p0.y ) / ( p1.x - p0.x ) + p0.y )
									 : new Vector2( 0, ( 0 - p0.x ) * ( p1.y - p0.y ) / ( p1.x - p0.x ) + p0.y );
				Vector2 cross2 = dir.y > 0
									 ? new Vector2( ( sizeY - p0.y ) * ( p1.x - p0.x ) / ( p1.y - p0.y ) + p0.x, sizeY )
									 : new Vector2( ( 0 - p0.y ) * ( p1.x - p0.x ) / ( p1.y - p0.y ) + p0.x, 0 );
				float d1 = ( cross1 - p0 ).sqrMagnitude;
				float d2 = ( cross2 - p0 ).sqrMagnitude;
				Vector2 p = d1 < d2 ? cross1 : cross2;
				Vector2 s = hud.arrow.size * 0.5f;
				if ( p.x > sizeX  - s.x )
					p.x = sizeX  - s.x;
				else if ( p.x < s.x )
					p.x = s.x;
				if ( p.y > sizeY - s.y )
					p.y = sizeY - s.y;
				else if ( p.y < s.y )
					p.y = s.y;
				hud.arrow.position = p;
			}
		}
	}
}