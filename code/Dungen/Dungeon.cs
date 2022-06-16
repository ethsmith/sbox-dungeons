
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
	private int CellOffset => 8;
	private int MergeIterations => 512;
	private bool MergeHuggers => true;

	[Event.BuildInput]
	private void OnBuildInput(InputBuilder b )
	{
		if ( b.Pressed( InputButton.SecondaryAttack ) )
		{
			Generate();
		}
	}

	[Event.Hotload]
	private void Generate()
	{
		//Rand.SetSeed( Rand.Int( 99999 ) );
		Rand.SetSeed( Seed );

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

		Cells[0].SetNode<DungeonNode>( "start" );
		Cells[1].SetNode<DungeonNode>( "end" );
	}

	private bool CompatibleForMerge( DungeonCell a, DungeonCell b )
	{
		var newrect = a.Rect;
		newrect.Add( b.Rect );

		if ( newrect.width > MaxCellWidth ) return false;
		if ( newrect.height > MaxCellHeight ) return false;

		if( MergeHuggers )
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
			var mins = new Vector3( cell.Rect.BottomLeft * CellScale, 1 );
			var maxs = new Vector3( cell.Rect.TopRight * CellScale, 1 );
			var offsetv = new Vector3( cell.Rect.Position, 0 );

			DebugOverlay.Box( mins + offsetv, maxs + offsetv, Color.Black );

			if ( cell.Node == null ) continue;

			var center = new Vector3( cell.Rect.Center, 0 ) * CellScale;
			DebugOverlay.Text( cell.Node.Name, center, 0, 3000 );

			maxs = maxs.WithZ( 256 );
			DebugOverlay.Box( mins + offsetv, maxs + offsetv, Color.Green );
		}
	}

}
