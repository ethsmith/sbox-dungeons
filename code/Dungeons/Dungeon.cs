
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

internal class Cell
{

	//public bool Void;
	public Rect Rect;

	public Cell( Rect rect )
	{
		Rect = rect;
	}

}

internal partial class Dungeon : Entity
{

	[Net]
	public int Seed { get; set; } = 1;
	[Net]
	public Vector2 Size { get; set; } = new Vector2( 2000, 2000 );

	private List<Cell> Cells = new();

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		Generate();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Generate();
	}

	// Customizable parameters, =>'s for now to test
	private int MaxCells => 16;
	private int MaxCellWidth => 7;
	private int MaxCellHeight => 7;
	private int CellScale => 128;
	private int CellOffset => 32;
	private int MergeIterations => 512 * 512;

	[Event.Hotload]
	private void Generate()
	{
		if ( IsServer ) return;

		Cells ??= new();
		Cells.Clear();

		for ( int x = 0; x < MaxCells; x++ )
		{
			for ( int y = 0; y < MaxCells; y++ )
			{
				var pos = new Vector2( x, y );
				var rect = new Rect( pos, 1 );
				Cells.Add( new( rect ) );
			}
		}

		for ( int i = 0; i < MergeIterations; i++ )
		{
			var cell = Rand.FromList( Cells );

			var neighbors = Cells.Where( x => x.Rect.Position.DistanceOrtho( cell.Rect.Position ) == 1 ).ToList();
			neighbors.RemoveAll( x => x.Rect.width != cell.Rect.width || x.Rect.height != x.Rect.height );

			foreach ( var n in neighbors )
			{
				var newRect = cell.Rect;
				newRect.Add( n.Rect );

				if ( newRect.width > MaxCellWidth ) continue;
				if ( newRect.height > MaxCellHeight ) continue;

				cell.Rect = newRect;

				for( int j = Cells.Count - 1; j >= 0; j-- )
				{
					if ( Cells[j] == cell ) continue;
					if ( !cell.Rect.IsInside( Cells[j].Rect.Center ) ) continue;
					Cells.RemoveAt( j );
				}

				break;
			}
		}
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( Cells == null ) return;

		foreach ( var cell in Cells )
		{
			var mins = new Vector3( cell.Rect.BottomLeft * CellScale, 1 );
			var maxs = new Vector3( cell.Rect.TopRight * CellScale, 1 );
			var offsetv = new Vector3( CellOffset * cell.Rect.Position, 0 );

			DebugOverlay.Box( mins + offsetv, maxs + offsetv, Color.Green );
		}
	}

}
