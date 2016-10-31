using UnityEngine;
using System.Collections;

public class CsvReader 
{
	private static readonly string[] LINE_SEPERATOR = new string[]{ "\r\n" };
	private const char FIELD_SEPERATOR = ',';

	public static string[,] GetGrid( string csvdata )
	{
		string[] rows = csvdata.Split( LINE_SEPERATOR, System.StringSplitOptions.None );
		string[,] grid = new string[ rows.Length, rows[0].Split( FIELD_SEPERATOR ).Length ];

		for( int i=0; i<rows.Length; i++ )
		{
			string[] columns = rows[i].Split( FIELD_SEPERATOR );

			for( int j=0; j<columns.Length; j++ )
			{
				grid[i,j] = columns[j];
			}
		}

		return grid;
	}
}
