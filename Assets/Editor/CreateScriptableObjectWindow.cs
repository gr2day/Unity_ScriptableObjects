using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;

public class CreateScriptableObjectWindow : EditorWindow 
{
	private const string SCRIPT_PATH 		= "Assets/ScriptableObjects/Scripts/";
	private const string OBJECT_PATH 		= "Assets/ScriptableObjects/Objects/";
	private const string DATA_PATH 			= "Assets/ScriptableObjects/Datas/";
	private const string FILE_EXTENSION 	= ".cs";
	private const string ASSET_EXTENSION 	= ".asset";

	[MenuItem ("Custom/ScriptableObjectWindow")]
	private static void Init () 
	{
		CreateScriptableObjectWindow window = (CreateScriptableObjectWindow)EditorWindow.GetWindow( typeof( CreateScriptableObjectWindow ) );
		window.Show();
	}

	void OnGUI () 
	{
		string[] fileList = Directory.GetFiles( SCRIPT_PATH );

		for( int i=0; i<fileList.Length; i++ )
		{
			string filePath = fileList[i];
			if( Path.GetExtension( filePath ).CompareTo( FILE_EXTENSION ) != 0 ) continue;

			string fileName = Path.GetFileNameWithoutExtension( filePath );

			if( IsDeriveScriptableObject( fileName ) )
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Label( fileName, EditorStyles.boldLabel, GUILayout.Width( 200 ) );
			
				if( GUILayout.Button( "Create", GUILayout.Width( 100 ) ) )
				{
					CreateAsset( fileName );
				};
				if( GUILayout.Button( "Delete", GUILayout.Width( 100 ) ) )
				{
					DeleteAsset( fileName );
				};
				if( GUILayout.Button( "Set", GUILayout.Width( 100 ) ) )
				{
					SetAsset( fileName );
				};

				GUILayout.EndHorizontal();
			}
		}
	}

	private static void CreateAsset( string className )
	{
		string filePath = OBJECT_PATH + className + ASSET_EXTENSION;

		if( File.Exists( filePath ) )
		{
			return;
		}

		AssetDatabase.CreateAsset( 
			ScriptableObject.CreateInstance( className ), 
			AssetDatabase.GenerateUniqueAssetPath( filePath ) 
		);
		AssetDatabase.SaveAssets();
	}

	private static void DeleteAsset( string className )
	{
		string filePath = OBJECT_PATH + className + ASSET_EXTENSION;
		AssetDatabase.DeleteAsset( filePath );
	}

	private static void SetAsset( string name )
	{
		UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath( OBJECT_PATH + name + ASSET_EXTENSION, GetTypeByName( name ) );

		FieldInfo listInfo = obj.GetType().GetFields()[0];

		if( listInfo.FieldType.IsGenericType )
		{
			Type itemType 			= listInfo.FieldType.GetGenericArguments()[0];
			Type listType 			= typeof( List<> ).MakeGenericType( itemType );
		
			IList listObject 		= (IList)Activator.CreateInstance( listType );

			MethodInfo addMethod 	= listObject.GetType().GetMethod( "Add", new[]{ itemType } );

			string filePath = DATA_PATH + name + ".csv";
			TextAsset testAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( filePath );

			if( testAsset == null )
			{
				Debug.LogError( "Not Exist Data : " + filePath );
				return;
			}

			string[,] grid = CsvReader.GetGrid( testAsset.text );
			string[] csvFields = new string[ grid.GetLength(1) ];

			for( int i=0; i<csvFields.Length; i++ )
			{
				csvFields[i] = grid[ 0, i ].Trim( '\r', '\n' );
			}

			// rows
			for( int i=1; i<grid.GetLength(0); i++ )
			{
				object listItem = Activator.CreateInstance( itemType );
				FieldInfo[] fieldInfos = listItem.GetType().GetFields();

				// column
				for( int j=0; j<grid.GetLength(1); j++ )
				{	
					for( int k=0; k<fieldInfos.Length; k++ )
					{
						if( csvFields[j] == fieldInfos[k].Name )
						{		
							fieldInfos[k].SetValue( listItem, Convert.ChangeType( grid[ i, j ], fieldInfos[k].FieldType ) );
							break;
						}
					}
				}

				addMethod.Invoke( listObject, new object[]{ listItem } );
			}

			listInfo.SetValue( obj, Convert.ChangeType( listObject, listInfo.FieldType ) );
		
			EditorUtility.SetDirty( obj );
			AssetDatabase.SaveAssets();

			Debug.Log( "Success : " + filePath );
		}
	}

	private static bool IsDeriveScriptableObject( string name )
	{
		return GetTypeByName( name ).BaseType == typeof( ScriptableObject );
	}

	private static Type GetTypeByName( string name )
	{
		foreach( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
		{
			foreach( Type type in assembly.GetTypes() )
			{
				if( type.Name.CompareTo(name) == 0 ) return type;
			}
		}

		return null;
	}
}


