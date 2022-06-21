
using HalfEdgeMesh;
using Architect;
using Sandbox;

namespace Dungeons;

internal class DungeonRoom
{

	public readonly DungeonCell Cell;
	public readonly Dungeon Dungeon;

	public DungeonRoom( Dungeon dungeon, DungeonCell cell )
	{
		Dungeon = dungeon;
		Cell = cell;
	}

	public void GenerateMesh( WallGeometry wallgeometry )
	{
		if ( !Host.IsClient ) return;

		var my = (int)(wallgeometry.GridSize.x / 32);
		var mx = (int)(wallgeometry.GridSize.y / 32);

		var left = (int)(Cell.Rect.left * mx);
		var right = (int)(Cell.Rect.right * mx);
		var bottom = (int)(Cell.Rect.bottom * my);
		var top = (int)(Cell.Rect.top * my);

		wallgeometry.AddEdge( left, bottom, left, top, 1 );
		wallgeometry.AddEdge( right, bottom, right, top, 1 );
		wallgeometry.AddEdge( left, bottom, right, bottom, 1 );
		wallgeometry.AddEdge( left, top, right, top, 1 );
	}

}
