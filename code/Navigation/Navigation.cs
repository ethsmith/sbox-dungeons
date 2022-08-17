
using Dungeons.Utility;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons;

// todo: lots of work to be done here in reducing memory and cpu and improving readability
// todo: we don't have to guarantee a full path, we just have to get you moving in the right direction.
//		 we can limit the search quite drastically and have no performance concerns despite using
//		 a giant square grid
// todo: anywhere a monster or player is standing should be marked as unwalkable

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
	private List<int> CalculatedPath = new();
	private Dictionary<int, int> AgentToIndex = new();
	private Dictionary<int, int> IndexToAgent = new();

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

	public int CalculatePath( Vector3 start, Vector3 end, Vector3[] points, int agent = -1 )
	{
		if ( !CalculatePath( FromWorld( start ), FromWorld( end ), agent ) )
		{
			return 0;
		}

		var length = Math.Min( CalculatedPath.Count, points.Length );

		for ( int i = 0; i < length; i++ )
		{
			points[i] = ToWorld( CalculatedPath[i] );
		}

		return length;
	}

	public List<Vector3> CalculatePath( Vector3 start, Vector3 end, int agent = -1 )
	{
		if ( CalculatePath( FromWorld( start ), FromWorld( end ), agent ) )
		{
			return CalculatedPath.Select( x => ToWorld( x ) ).ToList();
		}

		return new();
	}

	public IEnumerable<(int, Vector3)> GetAgentPositions()
	{
		foreach ( var kvp in AgentToIndex )
		{
			if ( kvp.Value == -1 ) continue;
			yield return (kvp.Key, ToWorld( kvp.Value ));
		}
	}

	public void RemoveAgent( int agentId ) => UpdateAgent( agentId, -1 );
	public void UpdateAgent( int agentId, Vector3 position ) => UpdateAgent( agentId, FromWorld( position ) );
	private void UpdateAgent( int agentId, int idx )
	{
		if ( AgentToIndex.ContainsKey( agentId ) )
		{
			var currentidx = AgentToIndex[agentId];
			IndexToAgent[currentidx] = -1;
		}

		AgentToIndex[agentId] = idx;
		IndexToAgent[idx] = agentId;
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

		return pos.x >= 0 && pos.x < GridSize.x && pos.y >= 0 && pos.y < GridSize.y;
	}

	private void FillNeighborsArray( int index )
	{
		var point = GetPosition( index );

		Neighbors[0] = GetIndex( point + Vector2.Left );
		Neighbors[1] = GetIndex( point + Vector2.Right );
		Neighbors[2] = GetIndex( point + Vector2.Up );
		Neighbors[3] = GetIndex( point + Vector2.Down );
		//Neighbors[4] = GetIndex( point + Vector2.Down + Vector2.Left );
		//Neighbors[5] = GetIndex( point + Vector2.Down + Vector2.Right );
		//Neighbors[6] = GetIndex( point + Vector2.Up + Vector2.Left );
		//Neighbors[7] = GetIndex( point + Vector2.Up + Vector2.Right );
	}

	private Vector3 ToWorld( int idx ) => GetPosition( idx ) * CellSize;
	private int FromWorld( Vector3 world ) => GetIndex( (int)MathF.Round( world.x / CellSize ), (int)MathF.Round( world.y / CellSize ) );

	private void ResetCollections()
	{
		CalculatedPath.Clear();
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

	private bool FindNearestWalkable( int from, int to, out int result )
	{
		result = -1;
		var line = GetStraightLine( to, from, 3, LineCache );
		for ( int i = 0; i < line; i++ )
		{
			if ( IsWalkable( LineCache[i] ) )
			{
				result = LineCache[i];
				return true;
			}
		}
		return false;
	}

	private bool CalculatePath( int start, int end, int agent = -1 )
	{
		var agentPosition = -1;

		try
		{
			if ( agent != -1 )
			{
				agentPosition = AgentToIndex[agent];
				RemoveAgent( agent );
			}

			if ( IsOccupied( start ) )
			{
				Log.Warning( "FIXME: two agents should never occupy the same space" );
				return false;
			}

			if ( !IsWalkable( end ) && !FindNearestWalkable( start, end, out end ) )
				return false;

			ResetCollections();

			if ( LineOfSight( start, end ) )
			{
				CalculatedPath.Add( start );
				CalculatedPath.Add( end );
				return true;
			}

			GScore[start] = 0;
			FScore[start] = Heuristic( start, end );
			OpenSet.Add( start );

			bool discovered = false;
			int current = 0;

			while ( OpenSet.Count > 0 )
			{
				current = LowestF();

				if ( current == end )
				{
					discovered = true;
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

			if ( !discovered ) return false;

			while ( current != start )
			{
				CalculatedPath.Add( current );
				current = CameFrom[current];
			}

			CalculatedPath.Add( start );
			CalculatedPath.Reverse();
			SimplifyCalculatedPath();

			return true;
		}
		finally
		{
			if ( agent != -1 )
				UpdateAgent( agent, agentPosition );
		}
	}

	private bool IsWalkable( int index )
	{
		if ( index < 0 || index >= Grid.Length )
			return false;

		if ( Grid[index] == 0 )
			return false;

		if ( IsOccupied( index ) )
			return false;

		return true;
	}

	private bool IsOccupied( int index )
	{
		if ( !IndexToAgent.ContainsKey( index ) )
			return false;

		if ( IndexToAgent[index] == -1 )
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

	private void SimplifyCalculatedPath()
	{
		if ( CalculatedPath.Count <= 2 )
			return;

		var start = 0;
		var next = 1;

		while ( start < next && next < CalculatedPath.Count - 1 )
		{
			var point1 = CalculatedPath[start];
			var point2 = CalculatedPath[next + 1];

			if ( LineOfSight( point1, point2 ) )
			{
				CalculatedPath[next] = -1;
			}
			else
			{
				start = next;
			}
			next++;
		}

		CalculatedPath.RemoveAll( x => x == -1 );
	}

	int[] LineCache = new int[1024];
	private bool LineOfSight( int from, int to )
	{
		var lineCount = GetStraightLine( from, to, 3, LineCache );
		for ( int i = 0; i < lineCount; i++ )
		{
			if ( !IsWalkable( LineCache[i] ) )
				return false;
		}
		return true;
	}

	private int GetStraightLine( int idx0, int idx1, int width, int[] cache )
	{
		var p0 = GetPosition( idx0 );
		var p1 = GetPosition( idx1 );
		var x0 = (int)p0.x;
		var x1 = (int)p1.x;
		var y0 = (int)p0.y;
		var y1 = (int)p1.y;

		int dx = (int)Math.Abs( x1 - x0 ), sx = x0 < x1 ? 1 : -1;
		int dy = (int)Math.Abs( y1 - y0 ), sy = y0 < y1 ? 1 : -1;
		int err = dx - dy, e2, x2, y2;                          /* error value e_xy */
		float ed = dx + dy == 0 ? 1 : (float)Math.Sqrt( (float)dx * dx + (float)dy * dy );

		var result = 0;

		for ( width = (width + 1) / 2; ; )
		{
			cache[result] = GetIndex( x0, y0 );
			result++;
			e2 = err; x2 = x0;
			if ( 2 * e2 >= -dx )
			{
				for ( e2 += dy, y2 = y0; e2 < ed * width && (y1 != y2 || dx > dy); e2 += dx )
				{
					cache[result] = GetIndex( x0, y2 += sy );
					result++;
				}
				if ( x0 == x1 ) break;
				e2 = err; err -= dy; x0 += sx;
			}
			if ( 2 * e2 <= dy )
			{
				for ( e2 = dx - e2; e2 < ed * width && (x1 != x2 || dx < dy); e2 += dy )
				{
					cache[result] = GetIndex( x2 += sx, y0 );
					result++;
				}
				if ( y0 == y1 ) break;
				err += dx; y0 += sy;
			}
		}
		return result;
	}

}
