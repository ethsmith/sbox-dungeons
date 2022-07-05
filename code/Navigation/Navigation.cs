
using Dungeons.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dungeons;

internal partial class NavigationEntity : Entity
{

	public static NavigationEntity Current;

	public int CellSize => 16;

	private Vector2 GridSize;
	private int[] Grid;

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
				var walkable = dungeon.IsPointWalkable( new Vector2( x, y ) * CellSize );
				Grid[GetIndex( x, y )] = walkable ? 1 : 0;
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

	private List<int> GetNeighbors( int index )
	{
		var point = GetPosition( index );

		return new List<int>()
		{
			GetIndex(point + Vector2.Left),
			GetIndex(point + Vector2.Right),
			GetIndex(point + Vector2.Up),
			GetIndex(point + Vector2.Down),
			GetIndex(point + Vector2.Down + Vector2.Left),
			GetIndex(point + Vector2.Down + Vector2.Right),
			GetIndex(point + Vector2.Up + Vector2.Left),
			GetIndex(point + Vector2.Up + Vector2.Right),
		};
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

	private List<int> CalculatePath( int start, int end )
	{
		// todo: lots of optimization to be done here

		var result = new List<int>();

		if ( !IsWalkable( start ) || !IsWalkable( end ) )
			return result;

		var open = new HashSet<int>() { start };
		var closed = new HashSet<int>();
		var cameFrom = new Dictionary<int, int>();
		var gscore = new Dictionary<int, float>();
		for ( int i = 0; i < Grid.Length; i++ )
			gscore[i] = float.MaxValue;
		gscore[start] = 0;

		var fscore = new Dictionary<int, float>();
		for ( int i = 0; i < Grid.Length; i++ )
			fscore[i] = float.MaxValue;
		fscore[start] = Heuristic( start, end );

		var current = 0;

		while ( open.Count > 0 )
		{
			current = LowestF( open, fscore );

			if ( current == end )
			{
				break;
			}

			open.Remove( current );
			closed.Add( current );

			var neighbors = GetNeighbors( current );

			foreach ( var neighbor in neighbors )
			{
				if ( !IsWalkable( neighbor ) ) continue;
				if ( closed.Contains( neighbor ) ) continue;

				var newscore = gscore[current] + Distance( current, neighbor );
				var opened = open.Contains( neighbor );
				var bettercost = newscore < gscore[neighbor];

				if ( !opened || bettercost )
				{
					cameFrom[neighbor] = current;
					gscore[neighbor] = newscore;

					if ( !opened )
					{
						open.Add( neighbor );
						fscore[neighbor] = Heuristic( start, neighbor );
					}
				}
			}
		}

		while ( current != start )
		{
			result.Add( current );
			current = cameFrom[current];
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

	private float Distance( int from, int to )
	{
		var fromv = GetPosition( from );
		var tov = GetPosition( to );
		return fromv.Distance( tov );
	}

	private float Heuristic( int from, int to ) => Distance( from, to );

	private int LowestF( IEnumerable<int> openset, IDictionary<int, float> scores )
	{
		var lowscore = float.MaxValue;
		var lowidx = -1;

		foreach ( var idx in openset )
		{
			if ( scores[idx] >= lowscore ) continue;
			lowscore = scores[idx];
			lowidx = idx;
		}

		return lowidx;
	}

}
