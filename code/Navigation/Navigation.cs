
using Sandbox;
using System;
using System.Collections.Generic;

namespace Dungeons;

// todo: lots of work to be done here in reducing memory and cpu and improving readability
// todo: we don't have to guarantee a full path, we just have to get you moving in the right direction.
//		 we can limit the search quite drastically and have no performance concerns despite using
//		 a giant square grid

internal partial class NavigationEntity : Entity
{

	public static NavigationEntity Current;

	public int CellSize => 16;

	private Vector2 GridSize;
	private int[] Grid;
	private int[] Neighbors = new int[8];
	private HashSet<int> OpenSet = new();
	private HashSet<int> ClosedSet = new();
	private Dictionary<int, int> CameFrom;
	private Dictionary<int, float> GScore;
	private Dictionary<int, float> FScore;

	public NavigationEntity()
	{
		Current = this;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[Event( "dungeon.postgen" )]
	public void Generate( DungeonEntity dungeon )
	{
		var bounds = dungeon.WorldRect;
		var gridx = (int)(bounds.width / CellSize);
		var gridy = (int)(bounds.height / CellSize);

		GridSize = new Vector2( gridx, gridy );
		Grid = new int[gridx * gridy];

		for ( int x = 0; x < gridx; x++ )
			for ( int y = 0; y < gridy; y++ )
			{
				var idx = GetIndex( x, y );
				var walkable = dungeon.IsPointWalkable( ToWorld( idx ) );
				Grid[idx] = walkable ? 1 : 0;
			}
	}

	private int GetIndex( Vector2 point ) => GetIndex( (int)point.x, (int)point.y );
	private int GetIndex( int x, int y )
	{
		var result = x * (int)GridSize.y + y;
		return IsOnMap( result ) ? result : -1;
	}

	private Vector2 GetPosition( int index )
	{
		int x = (int)(index / GridSize.y);
		int y = (int)(index % GridSize.y);
		return new Vector2( x, y );
	}

	private bool IsOnMap( int index )
	{
		var pos = GetPosition( index );
		return IsOnMap( (int)pos.x, (int)pos.y );
	}
	private bool IsOnMap( int x, int y ) => x >= 0 && y >= 0 && x < GridSize.x && y < GridSize.y;

	private void FillNeighborsArray( int index )
	{
		var point = GetPosition( index );

		Neighbors[0] = GetIndex( point + Vector2.Left );
		Neighbors[1] = GetIndex( point + Vector2.Right );
		Neighbors[2] = GetIndex( point + Vector2.Up );
		Neighbors[3] = GetIndex( point + Vector2.Down );
		Neighbors[4] = GetIndex( point + Vector2.Down + Vector2.Left );
		Neighbors[5] = GetIndex( point + Vector2.Down + Vector2.Right );
		Neighbors[6] = GetIndex( point + Vector2.Up + Vector2.Left );
		Neighbors[7] = GetIndex( point + Vector2.Up + Vector2.Right );
	}

	public List<Vector3> CalculatePath( Vector3 start, Vector3 end )
	{
		var result = new List<Vector3>();

		var startidx = GetIndex( (int)start.x / CellSize, (int)start.y / CellSize );
		var endidx = GetIndex( (int)end.x / CellSize, (int)end.y / CellSize );

		var idxpath = CalculatePath( startidx, endidx );
		foreach ( var idx in idxpath )
		{
			result.Add( ToWorld( idx ) );
		}

		return result;
	}

	private Vector3 ToWorld( int idx )
	{
		var pos = GetPosition( idx );
		pos *= CellSize;
		return pos;
	}

	private void ResetCollections()
	{
		OpenSet.Clear();
		ClosedSet.Clear();

		CameFrom ??= new Dictionary<int, int>( Grid.Length );
		GScore ??= new Dictionary<int, float>( Grid.Length );
		FScore ??= new Dictionary<int, float>( Grid.Length );

		for ( int i = 0; i < Grid.Length; i++ )
		{
			GScore[i] = float.MaxValue;
			FScore[i] = float.MaxValue;
		}
	}

	private List<int> CalculatePath( int start, int end )
	{
		var result = new List<int>();

		if ( !IsWalkable( start ) || !IsWalkable( end ) )
			return result;

		ResetCollections();

		GScore[start] = 0;
		FScore[start] = Heuristic( start, end );
		OpenSet.Add( start );

		int current = 0;

		while ( OpenSet.Count > 0 )
		{
			current = LowestF();

			if ( current == end )
			{
				break;
			}

			OpenSet.Remove( current );
			ClosedSet.Add( current );

			var currentpos = GetPosition( current );

			FillNeighborsArray( current );
			foreach ( var neighbor in Neighbors )
			{
				if ( !IsWalkable( neighbor ) ) continue;
				if ( ClosedSet.Contains( neighbor ) ) continue;

				var neighborpos = GetPosition( neighbor );
				var dir = neighborpos - currentpos;
				var straight = dir.x == 0 || dir.y == 0;
				var newscore = GScore[current] + (straight ? 1f : 1.4142f);
				var opened = OpenSet.Contains( neighbor );
				var bettercost = newscore < GScore[neighbor];

				if ( !opened || bettercost )
				{
					CameFrom[neighbor] = current;
					GScore[neighbor] = newscore;

					if ( !opened )
					{
						OpenSet.Add( neighbor );
						FScore[neighbor] = Heuristic( start, neighbor );
					}
				}
			}
		}

		while ( current != start )
		{
			result.Add( current );
			current = CameFrom[current];
		}
		result.Reverse();

		return result;
	}

	private bool IsWalkable( int index )
	{
		if ( index < 0 || index >= Grid.Length )
			return false;

		if ( Grid[index] == 0 )
			return false;

		return true;
	}

	private float Heuristic( int from, int to )
	{
		var fromv = GetPosition( from );
		var tov = GetPosition( to );
		return MathF.Sqrt( MathF.Pow( fromv.x - tov.x, 2 ) + MathF.Pow( fromv.y - tov.y, 2 ) );
	}

	private int LowestF()
	{
		var lowscore = float.MaxValue;
		var lowidx = -1;

		foreach ( var idx in OpenSet )
		{
			if ( FScore[idx] >= lowscore ) continue;
			lowscore = FScore[idx];
			lowidx = idx;
		}

		return lowidx;
	}

}
