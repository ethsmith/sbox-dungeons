
// Jake:
//	1. Generate 1x1 grid
//	2. Randomly pick a cell
//	3. Merge it with a neighboring cell of the same size
//	4. Repeat steps 2 and 3 many times
//	5. Insert Nodes at start, end, and other points of interest
//	6. Pathfind between each node, only keeping cells along the paths

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

using Architect;
using Dungeons.Utility;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons;

internal partial class DungeonEntity : Entity
{

	public static DungeonEntity Current;

	[Net]
	public int Seed { get; set; } = 1;

	public IReadOnlyList<DungeonCell> Cells => cells;
	public IReadOnlyList<DungeonRoute> Routes => routes;
	public IReadOnlyList<DungeonRoom> Rooms => rooms;

	private List<Entity> entities;
	private List<DungeonCell> cells = new();
	private List<DungeonRoute> routes = new();
	private List<DungeonRoom> rooms = new();
	private WallObject WallGeometry;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Current = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Current = this;

		Generate();
	}

	// Customizable parameters, =>'s for now to test
	public int DungeonWidth => 8;
	public int DungeonHeight => 8;
	private int MaxCellWidth => 4;
	private int MaxCellHeight => 4;
	public int CellScale => 384;
	private int MergeIterations => 1024;
	private bool MergeHuggers => false;
	public Rect WorldRect => new Rect( 0, 0, DungeonWidth, DungeonHeight ) * CellScale;

	[Event.Hotload]
	public void Generate()
	{
		Rand.SetSeed( Seed );

		entities?.ForEach( x => x.Delete() );
		entities = new();


		var mult = CellScale / 32;
		var gridSize = new Vector2( DungeonWidth * mult, DungeonHeight * mult );
		var mapBounds = new Vector3( DungeonWidth * CellScale, DungeonHeight * CellScale, 128f );

		WallGeometry?.Destroy();
		WallGeometry = new( Map.Scene, Map.Physics, gridSize, mapBounds );

		WallGeometry.HEMesh.CreateGrid( DungeonWidth * mult, DungeonHeight * mult );
		WallGeometry.RebuildMesh();

		rooms = new();
		routes = new();
		cells = CreateCells( DungeonWidth - 1, DungeonHeight - 1 );

		for ( int i = 0; i < MergeIterations; i++ )
		{
			var randomCell = Rand.FromList( cells );

			foreach ( var cell in cells )
			{
				if ( !CompatibleForMerge( randomCell, cell ) ) continue;

				randomCell.Rect.Add( cell.Rect );
				cells.RemoveAll( x => x != randomCell && randomCell.Rect.IsInside( x.Rect.Center ) );
				break;
			}
		}

		// add a few random routes for testing

		var current = Cells
			.Where( x => x.Node == null )
			.OrderBy( x => Rand.Int( 9999 ) )
			.FirstOrDefault();

		for( int i = 0; i < 8; i++ )
		{
			var next = Cells
				.Where( x => x.Node == null && x != current )
				.OrderBy( x => Rand.Int( 9999 ) )
				.FirstOrDefault();

			if ( next == null ) break;

			var name1 = i == 0 ? "start" : (i == 1 ? "loot" : "empty");
			var name2 = i == 0 ? "end" : (i == 1 ? "boss" : "empty");
			current.SetNode<DungeonNode>( name1 );
			next.SetNode<DungeonNode>( name2 );
			routes.Add( new( this, new DungeonEdge( current, next ) ) );

			current = next;
		}

		foreach ( var route in routes )
		{
			route.Calculate();
			foreach ( var cell in route.Cells )
			{
				if ( rooms.Any( x => x.Cell == cell ) )
					continue;

				rooms.Add( new( this, cell ) );
			}
		}

		foreach ( var room in rooms )
		{
			room.GenerateMesh( WallGeometry );
			WallGeometry.RebuildMesh();
		}

		if ( Host.IsServer )
		{
			SpawnEntities();
			AdjustFog();
		}

		Event.Run( "dungeon.postgen", this );
	}

	private void SpawnEntities()
	{
		foreach ( var room in Rooms )
		{
			var center = room.WorldRect.Center;
			var light = new PointLightEntity()
			{
				Color = Color.Orange * .18f,
				Range = 300,
				Position = new Vector3( center, 150 ),
				DynamicShadows = true,
			};

			var monster = new Monster()
			{
				Position = new Vector3( center, 1 )
			};

			entities.Add( light );
			entities.Add( monster );
		}
	}

	public List<DungeonCell> NeighborsOf( DungeonCell cell, bool orthogonal = true )
	{
		var result = new List<DungeonCell>();

		foreach ( var c in cells )
		{
			if ( !c.Rect.IsInside( cell.Rect ) ) continue;
			if ( orthogonal )
			{
				if ( c.Rect.BottomLeft == cell.Rect.TopRight ) continue;
				if ( c.Rect.BottomRight == cell.Rect.TopLeft ) continue;
				if ( c.Rect.TopLeft == cell.Rect.BottomRight ) continue;
				if ( c.Rect.TopRight == cell.Rect.BottomLeft ) continue;
			}
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

	private List<DungeonCell> CreateCells( int width, int height )
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

	public DungeonRoom FindRoom( string name )
	{
		var cell = cells.FirstOrDefault( x => x.Node?.Name?.Equals( name, System.StringComparison.InvariantCultureIgnoreCase ) ?? false );

		if ( cell == null )
			return null;

		return Rooms.FirstOrDefault( x => x.Cell == cell );
	}

	public bool IsPointWalkable( Vector2 worldpos )
	{
		foreach ( var r in Routes )
		{
			if ( r.Doors.Any( x => x.WorldRect.IsInside( worldpos ) ) )
				return true;
		}

		if ( Rooms.Any( x => x.WorldRect.Contract( 16.1f ).IsInside( worldpos ) ) )
			return true;

		return false;
	}

	private void AdjustFog()
	{
		var fogstr = Rand.Float( .75f, 1f );
		Map.Scene.GradientFog.Color = Color.Black;
		Map.Scene.GradientFog.MaximumOpacity = fogstr;
	}

}
