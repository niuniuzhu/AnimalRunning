using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Client.Editor
{
	public class ModelFormator
	{
		static readonly Regex TEXTURE_REGEX = new Regex( @"(Assets/Sources/Models/Characters/[^/]*/)Textures/([^\.]*)\.(tga|png|jpg|jpeg|bmp|psd|tif|tiff)", RegexOptions.IgnoreCase );
		static readonly Regex MODEL_REGEX = new Regex( @"(Assets/Sources/Models/Characters/[^/]*/)([^\.]*)\.fbx", RegexOptions.IgnoreCase );

		[MenuItem( "Assets/Format Model &t" )]
		public static void FormatModel()
		{
			GameObject[] selectedObjects = Selection.gameObjects;
			if ( selectedObjects.Length == 0 )
				return;

			string assetPath = AssetDatabase.GetAssetPath( selectedObjects[0] );
			Match match = MODEL_REGEX.Match( assetPath );
			if ( match.Length == 0 )
				return;
			string path = match.Groups[1].Value;

			string supportedExtensions = "*.tga,*.psd,*.jpg,*.jpeg,*.png,*.bmp,*.tif,*.tiff";
			IEnumerable<string> fileEntries = Directory.GetFiles( GetAbsPath( path + "Textures" ), "*.*" ).Where( s => supportedExtensions.Contains( Path.GetExtension( s ).ToLower() ) );
			int prefixPos = ApplicationPath().Length;
			try
			{
				AssetDatabase.StartAssetEditing();
				foreach ( string fileEntry in fileEntries )
				{
					string texturePath = fileEntry.Substring( prefixPos, fileEntry.Length - prefixPos ).Replace( "\\", "/" );
					FormatTexture( texturePath );
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				AssetDatabase.SaveAssets();
			}
			foreach ( GameObject go in selectedObjects )
				FormatModel( AssetDatabase.GetAssetPath( go ) );
			AssetDatabase.SaveAssets();
		}

		static void FormatTexture( string assetPath )
		{
			Match match = TEXTURE_REGEX.Match( assetPath );
			if ( match.Length == 0 )
				return;
			string name = match.Groups[2].Value;
			Material material = new Material( Shader.Find( "Legacy Shaders/Diffuse" ) );
			material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>( assetPath );
			AssetDatabase.CreateAsset( material, "Assets/Resources/Materials/Skin/" + name + ".mat" );
		}

		static void FormatModel( string assetPath )
		{
			Match match = MODEL_REGEX.Match( assetPath );
			if ( match.Length == 0 )
				return;
			string path = match.Groups[1].Value;
			string name = match.Groups[2].Value;

			Object[] assets = AssetDatabase.LoadAllAssetsAtPath( assetPath );
			List<AnimationClip> animationClips = new List<AnimationClip>();
			foreach ( Object asset in assets )
			{
				if ( asset is AnimationClip clip )
					animationClips.Add( clip );
			}

			AssetDatabase.DeleteAsset( path + name + ".controller" );
			AssetDatabase.CopyAsset( "Assets/Sources/Models/Other/template.controller", path + name + ".controller" );

			AnimatorController controller =
				AssetDatabase.LoadAssetAtPath<AnimatorController>( path + name + ".controller" );

			SetupAnimatorController( controller, animationClips );
			MakePrefab( assetPath, name, controller );
		}

		static void SetupAnimatorController( AnimatorController controller, List<AnimationClip> animationClips )
		{
			Dictionary<string, AnimationClip> nameToAnimationClips = new Dictionary<string, AnimationClip>();
			int count = animationClips.Count;
			for ( int i = 0; i < count; i++ )
			{
				AnimationClip animationClip = animationClips[i];
				nameToAnimationClips[animationClip.name] = animationClip;
			}

			AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
			ChildAnimatorState[] states = rootStateMachine.states;
			count = states.Length;
			for ( int i = 0; i < count; i++ )
			{
				AnimatorState animatorState = rootStateMachine.states[i].state;
				AnimationClip animationClip = nameToAnimationClips[animatorState.name];
				animatorState.motion = animationClip;
			}
		}

		private static void MakePrefab( string assetPath, string name, AnimatorController controller )
		{
			GameObject go = Object.Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>( assetPath ) );
			GameObject blobShadow = Object.Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/Sources/Models/Other/BlobShadow.prefab" ) );
			blobShadow.transform.SetParent( go.transform, false );
			SkinnedMeshRenderer[] smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach ( SkinnedMeshRenderer smr in smrs )
			{
				smr.gameObject.name = "Skin";
			}
			go.GetComponent<Animator>().runtimeAnimatorController = controller;
			PrefabUtility.CreatePrefab( "Assets/Resources/Prefabs/" + name + ".prefab", go );
			Object.DestroyImmediate( go );
		}

		static string ApplicationPath()
		{
			return Application.dataPath.Replace( "Assets", string.Empty );
		}

		static string GetAbsPath( string assetPath )
		{
			return ApplicationPath() + assetPath;
		}
	}
}