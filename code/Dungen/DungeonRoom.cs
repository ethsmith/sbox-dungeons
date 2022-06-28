
using Architect;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons;

internal class DungeonRoom
{

	public readonly DungeonCell Cell;
	public readonly DungeonEntity Dungeon;
	public readonly Rect WorldRect;

	public DungeonRoom( DungeonEntity dungeon, DungeonCell cell )
	{
		Dungeon = dungeon;
		Cell = cell;

		var position = cell.Rect.Position * dungeon.CellScale;
		var size = cell.Rect.Size * dungeon.CellScale;
		WorldRect = new( position, size );
	}

	public void GenerateMesh( WallObject wallgeometry )
	{
		var my = (int)(wallgeometry.GridSize.x / Dungeon.DungeonWidth);
		var mx = (int)(wallgeometry.GridSize.y / Dungeon.DungeonHeight);

		var left = (int)(Cell.Rect.left * mx);
		var right = (int)(Cell.Rect.right * mx);
		var bottom = (int)(Cell.Rect.bottom * my);
		var top = (int)(Cell.Rect.top * my);

		wallgeometry.AddEdge( left, bottom, left, top, 1 );
		wallgeometry.AddEdge( right, bottom, right, top, 1 );
		wallgeometry.AddEdge( left, bottom, right, bottom, 1 );
		wallgeometry.AddEdge( left, top, right, top, 1 );

		var doors = new List<DungeonDoor>();
		foreach( var r in Dungeon.Routes )
		{
			doors.AddRange( r.Doors.Where( x => x.A == Cell || x.B == Cell ) );
		}

		foreach( var door in doors )
		{
			var rect = door.CalculateRect();
			var centerx = (int)(rect.Center.x * mx);
			var centery = (int)(rect.Center.y * mx);

			wallgeometry.AddEdge( centerx, centery - 2, centerx, centery + 2, 0 );
			wallgeometry.AddEdge( centerx - 2, centery, centerx + 2, centery, 0 );
		}
	}

}
