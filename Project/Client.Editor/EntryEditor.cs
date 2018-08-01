using Game.Misc;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.Editor
{
	[CustomEditor( typeof( Entry ) )]
	public class EntryEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			this.serializedObject.Update();

			Entry script = ( Entry )this.target;

			script.useNetwork = EditorGUILayout.Toggle( "Use Network", script.useNetwork );
			if ( script.useNetwork )
			{
				script.useKCP = EditorGUILayout.Toggle( "Use KCP", script.useKCP );
				script.ip = EditorGUILayout.TextField( "Ip", script.ip );
				script.port = EditorGUILayout.IntField( "Port", script.port );
			}
			else
			{
				script.cid = EditorGUILayout.TextField( "Cid", script.cid );
				script.image = EditorGUILayout.IntField( "Image", script.image );
				script.map = EditorGUILayout.TextField( "Map", script.map );
			}

			EditorGUILayout.BeginVertical( "box" );
			EditorGUILayout.LabelField( "Log:" );
			script.logServerIp = EditorGUILayout.TextField( "Ip", script.logServerIp );
			script.logServerPort = EditorGUILayout.IntField( "Port", script.logServerPort );
			EditorGUILayout.LabelField( "Log level:" );
			LoggerProxy.LogLevel logLevel = ( LoggerProxy.LogLevel )EditorGUILayout.EnumFlagsField( "Level", script.logLevel );

			EditorGUILayout.EndVertical();

			if ( GUI.changed )
				EditorSceneManager.MarkSceneDirty( SceneManager.GetActiveScene() );

			this.serializedObject.FindProperty( "logLevel" ).intValue = ( int )logLevel;
			this.serializedObject.ApplyModifiedProperties();
		}
	}
}