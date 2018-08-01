using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Client.Editor
{
	public class AssetProcessor : AssetPostprocessor
	{
		static readonly Regex MODEL_REGEX = new Regex( @"(Assets/Sources/Models/Characters/[^/]*/)([^\.]*)\.fbx", RegexOptions.IgnoreCase );

		void OnPreprocessModel()
		{
			Match match = MODEL_REGEX.Match( this.assetPath );
			if ( match.Length == 0 )
				return;
			string path = match.Groups[1].Value;

			ModelImporter importer = ( ModelImporter )this.assetImporter;
			importer.isReadable = false;
			importer.importVisibility = false;
			importer.importCameras = false;
			importer.importLights = false;
			importer.importMaterials = false;

			TextAsset frame = AssetDatabase.LoadAssetAtPath<TextAsset>( path + "frame.txt" );
			if ( frame == null )
				return;

			List<ModelImporterClipAnimation> micas = new List<ModelImporterClipAnimation>();
			using ( StringReader reader = new StringReader( frame.text ) )
			{
				while ( reader.Peek() > -1 )
				{
					string line = reader.ReadLine();
					if ( string.IsNullOrEmpty( line ) )
						continue;
					string[] args = line.Split( ' ' );
					string aniName = args[0];
					args = args[1].Split( '-' );
					string startFrame = args[0];
					string endFrame = args[1];
					ModelImporterClipAnimation mica = new ModelImporterClipAnimation();
					mica.name = aniName;
					mica.firstFrame = float.Parse( startFrame );
					mica.lastFrame = float.Parse( endFrame );
					mica.loopTime = true;
					micas.Add( mica );
				}
			}
			importer.clipAnimations = micas.ToArray();
		}
	}
}