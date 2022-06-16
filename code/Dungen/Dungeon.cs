
// Jake:
//	1. Generate 1x1 grid
//	2. Randomly pick a cell
//	3. Merge it with a neighboring cell of the same size
//	4. Repeat steps 2 and 3 many times
//	5. Insert Nodes at start, end, and other points of interest
//	6. A* between each node, only keeping cells along the paths

// Why not BSP tree?
//	This way makes it easy to parameterize dungeons with a graph
//	Give players a similar but randomized layout each playthrough
//	Anybody can create and adjust levels with a simple graphing tool

//	* = Node
//	
//	 * (START)
//	  \                    *--------*--------* (BOSS)
//	   \                  /         |
//      *----------------*          |
//                                  |
//                         *--------*
//                        /         |
//                       /          |
//           (DEAD END) *           * (SICK LOOT)

// Something I wanna experiment with:
//	Instead of merging cells randomly.. spawn a bunch of randomly sized
//	cells at the same position and use 2d physics to separate them

using Dungeons.Utility;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons;

internal partial class Dungeon : Entity
{

	[Net]
	public int Seed { get; set; } = 1;
	[Net]
	public Vector2 Size { get; set; } = new Vector2( 2000, 2000 );

	private List<DungeonCell> Cells = new();
	private List<DungeonCellEdge> Edges = new();

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		//Generate();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Generate();
	}

	// Customizable parameters, =>'s for now to test
	private int DungeonWidth => 32;
	private int DungeonHeight => 16;
	private int MaxCellWidth => 7;
	private int MaxCellHeight => 7;
	private int CellScale => 128;
	private int MergeIterations => 512;
	private bool MergeHuggers => true;

	[Event.BuildInput]
	private void OnBuildInput( InputBuilder b )
	{
		if ( b.Pressed( InputButton.SecondaryAttack ) )
		{
			Generate();
		}
	}

	[Event.Hotload]
	private void Generate()
	{
		Rand.SetSeed( Rand.Int( 99999 ) );
		//Rand.SetSeed( Seed );

		Edges = new();
		Cells = CreateGrid( DungeonWidth, DungeonHeight );

		for ( int i = 0; i < MergeIterations; i++ )
		{
			var randomCell = Rand.FromList( Cells );

			foreach ( var cell in Cells )
			{
				if ( !CompatibleForMerge( randomCell, cell ) ) continue;

				randomCell.Rect.Add( cell.Rect );
				Cells.RemoveAll( x => x != randomCell && randomCell.Rect.IsInside( x.Rect.Center ) );
				break;
			}
		}

		// add a few random dummy nodes for testing
		for( int i = 0; i < 4; i++ )
		{
			var idx1 = Rand.Int( Cells.Count - 1 );
			var idx2 = Rand.Int( Cells.Count - 4 );
			Cells[idx1].SetNode<DungeonNode>( "N" );
			Cells[idx2].SetNode<DungeonNode>( "L" );
			Edges.Add( new DungeonCellEdge( Cells[idx1], Cells[idx2] ) );
		}

		var cellsToKeep = new List<DungeonCell>();
		foreach ( var edge in Edges )
		{
			cellsToKeep.AddRange( RouteEdge( edge ) );
		}

		Cells.RemoveAll( x => !cellsToKeep.Contains( x ) );
	}

	private List<DungeonCell> RouteEdge( DungeonCellEdge edge )
	{
		var result = new List<DungeonCell>();
		var unexplored = new List<DungeonCell>( Cells );

		foreach( var c in unexplored )
		{
			c.Distance = float.PositiveInfinity;
			c.Parent = null;
		}

		edge.A.Distance = 0;

		while ( unexplored.Count > 0 )
		{
			var current = unexplored.OrderBy( x => x.Distance ).First();
			unexplored.Remove( current );

			if ( current == edge.B )
			{
				while( current != null )
				{
					result.Add( current );
					current = current.Parent;
				}
				break;
			}

			var neighbors = NeighborsOf( current );
			foreach ( var n in neighbors )
			{
				var dist = current.Distance + n.Rect.Position.DistanceOrtho( current.Rect.Position );

				if ( !unexplored.Contains( n ) ) continue;
				if ( dist >= n.Distance ) continue;

				n.Distance = dist;
				n.Parent = current;
			}
		}

		result.Reverse();
		return result;
	}

	private List<DungeonCell> NeighborsOf( DungeonCell cell )
	{
		var result = new List<DungeonCell>();

		foreach ( var c in Cells )
		{
			if ( c.Rect.IsInside( cell.Rect ) )
				result.Add( c );
		}

		return result;
	}

	private bool CompatibleForMerge( DungeonCell a, DungeonCell b )
	{
		var newrect = a.Rect;
		newrect.Add( b.Rect );

		if ( newrect.width > MaxCellWidth ) return false;
		if ( newrect.height > MaxCellHeight ) return false;

		if ( MergeHuggers )
		{
			// early true for rects that are same width or height AND hugging each other
			if ( a.Rect.left == b.Rect.left && a.Rect.width == b.Rect.width )
				if ( a.Rect.IsInside( b.Rect ) )
					return true;

			if ( a.Rect.bottom == b.Rect.bottom && a.Rect.height == b.Rect.height )
				if ( a.Rect.IsInside( b.Rect ) )
					return true;
		}

		if ( a.Rect.width != b.Rect.width ) return false;
		if ( a.Rect.height != b.Rect.height ) return false;
		if ( a.Rect.Position.DistanceOrtho( b.Rect.Position ) != 1 ) return false;

		return true;
	}

	private List<DungeonCell> CreateGrid( int width, int height )
	{
		var result = new List<DungeonCell>();

		for ( int x = 0; x < width; x++ )
		{
			for ( int y = 0; y < height; y++ )
			{
				var pos = new Vector2( x, y );
				var rect = new Rect( pos, 1 );
				result.Add( new( rect ) );
			}
		}

		return result.OrderBy( x => Rand.Int( 999 ) ).ToList();
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( Cells == null ) return;

		foreach ( var cell in Cells )
		{
			var color = cell.Node != null ? Color.Green : Color.Black;
			var mins = new Vector3( cell.Rect.BottomLeft * CellScale, 1 );
			var maxs = new Vector3( cell.Rect.TopRight * CellScale, 1 );
			var offsetv = new Vector3( cell.Rect.Position, 0 );

			DebugOverlay.Box( mins + offsetv, maxs + offsetv, color );

			if ( cell.Node == null ) continue;

			var center = new Vector3( cell.Rect.Center, 0 ) * CellScale;
			DebugOverlay.Text( cell.Node.Name, center, 0, 6000 );
			DebugOverlay.Box( mins + offsetv, maxs.WithZ( 256 ) + offsetv, color );
		}
	}

}
