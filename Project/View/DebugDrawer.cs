using System.Collections.Generic;
using Logic.Event;
using UnityEngine;

namespace View
{
	public class DebugDrawer
	{
		private struct DrawInfo
		{
			public SyncEvent.DebugDrawType type;
			public Vector3 v0;
			public Vector3 v1;
			public float f;
			public Color c;
		}

		private readonly Queue<DrawInfo> _drawInfos = new Queue<DrawInfo>();

		public void Update()
		{
			while ( this._drawInfos.Count > 0 )
			{
				DrawInfo drawInfo = this._drawInfos.Dequeue();
				Color c = Gizmos.color;
				Gizmos.color = drawInfo.c;
				switch ( drawInfo.type )
				{
					case SyncEvent.DebugDrawType.Ray:
						Gizmos.DrawRay( drawInfo.v0, drawInfo.v1 );
						break;
					case SyncEvent.DebugDrawType.Line:
						Gizmos.DrawLine( drawInfo.v0, drawInfo.v1 );
						break;
					case SyncEvent.DebugDrawType.Cube:
						Gizmos.DrawCube( drawInfo.v0, drawInfo.v1 );
						break;
					case SyncEvent.DebugDrawType.Sphere:
						Gizmos.DrawSphere( drawInfo.v0, drawInfo.f );
						break;
					case SyncEvent.DebugDrawType.WireCube:
						Gizmos.DrawWireCube( drawInfo.v0, drawInfo.v1 );
						break;
					case SyncEvent.DebugDrawType.WireSphere:
						Gizmos.DrawWireSphere( drawInfo.v0, drawInfo.f );
						break;
				}
				Gizmos.color = c;
			}
		}

		public void HandleDebugDraw( SyncEvent.DebugDrawType type, Vector3 v0, Vector3 v1, float f, Color color )
		{
			DrawInfo drawInfo;
			drawInfo.type = type;
			drawInfo.v0 = v0;
			drawInfo.v1 = v1;
			drawInfo.f = f;
			drawInfo.c = color;
			this._drawInfos.Enqueue( drawInfo );
		}
	}
}