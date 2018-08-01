using Client.UI;
using Game.Task;
using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client
{
	public static class Standalone
	{
		private static string _cid;
		private static byte _image;
		private static string _map;

		public static void Init( string cid, byte image, string map )
		{
			_cid = cid;
			_image = image;
			_map = map;
		}

		public static void Dispose()
		{
		}

		public static void Load()
		{
			SyncTask.Create( StartLoad( _cid, _image, _map ) );
		}

		private static IEnumerator StartLoad( string cid, byte skin, string map )
		{
			if ( string.IsNullOrEmpty( cid ) )
				cid = "c" + UnityEngine.Random.Range( 1, 6 );
			if ( skin == 0xff )
				skin = ( byte )UnityEngine.Random.Range( 0, 2 );
			if ( string.IsNullOrEmpty( map ) )
				map = "m" + UnityEngine.Random.Range( 1, 12 );

			AsyncOperation ao = SceneManager.LoadSceneAsync( "maze", LoadSceneMode.Single );
			while ( !ao.isDone )
			{
				yield return 0;
			}

			BattleParams param;
			param.framesPerKeyFrame = 4;
			param.frameRate = 20;
			param.uid = "user";
			param.id = map;
			param.rndSeed = DateTime.Now.Millisecond;

			List<BattleParams.Player> players = new List<BattleParams.Player>();
			BattleParams.Player player = new BattleParams.Player
			{
				id = "user",
				cid = cid,
				skin = skin,
				team = 0
			};
			players.Add( player );

			for ( int i = 0; i < 7; i++ )
			{
				player = new BattleParams.Player
				{
					id = "xxx0" + i,
					cid = "c" + UnityEngine.Random.Range( 1, 6 ),
					skin = ( byte )UnityEngine.Random.Range( 0, 3 ),
					team = 1
				};
				players.Add( player );
			}

			param.players = players.ToArray();

			UIManager.EnterBattle( param );
		}
	}
}