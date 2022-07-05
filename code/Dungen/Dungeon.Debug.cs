
using Sandbox;
using SandboxEditor;
using System.Linq;

namespace Dungeons;

internal partial class DungeonEntity
{

	[DebugOverlay( "dungeon", "Dungeon", "map" )]
	public static void DungeonDebugOverlay()
	{
		if ( !Host.IsClient ) return;
		if ( !Current.IsValid() ) return;

		var nodes = Current.Rooms.Where( x => x.Cell.Node != null ).Select( x => x.Cell.Node.Name );
		var nodeStr = string.Join( ',', nodes );
		var txt = $@"Seed: {Current.Seed}
Rooms: {Current.Rooms.Count}
Nodes: {nodeStr}
";

		var rect = new Rect( 16, 9999 );
		{
			Render.Draw2D.SetFont( "Consolas", 15 );

			Render.Draw2D.Color = Color.Black;
			Render.Draw2D.DrawText( rect.Expand( -1 ), txt, TextFlag.LeftTop );

			Render.Draw2D.Color = Color.White.Darken( .25f );
			Render.Draw2D.DrawText( rect, txt, TextFlag.LeftTop );
		}

		Current?.DebugDrawRooms();
	}

	void DebugDrawRooms()
	{
		if ( cells == null ) return;

		//WallGeometry?.DebugDraw();

		foreach ( var cell in cells )
		{
			var color = cell.Node != null ? Color.Green : Color.Black;
			var isroute = routes.Any( x => x.Cells.Contains( cell ) );
			var mins = new Vector3( cell.Rect.BottomLeft * CellScale, 1 );
			var maxs = new Vector3( cell.Rect.TopRight * CellScale, 1 );

			DebugOverlay.Box( mins, maxs, isroute ? Color.White : color.WithAlpha( .1f ) );

			if ( cell.Node == null ) continue;

			var center = new Vector3( cell.Rect.Center, 0 ) * CellScale;
			DebugOverlay.Text( cell.Node.Name, center, 0, 6000 );
			//DebugOverlay.Box( mins, maxs.WithZ( 256 ), color );
		}

		foreach ( var route in routes )
		{
			foreach ( var door in route.Doors )
			{
				var rect = door.CalculateRect();
				var mins = new Vector3( rect.BottomLeft * CellScale, 1 );
				var maxs = new Vector3( rect.TopRight * CellScale, 96 );
				//DebugOverlay.Circle( rect.Center * CellScale, Rotation.LookAt( Vector3.Up ), .15f * CellScale, Color.Cyan, 0, false );
			}

			for ( int i = 0; i < route.Doors.Count - 1; i++ )
			{
				var centera = route.Doors[i].CalculateRect().Center;
				var centerb = route.Doors[i + 1].CalculateRect().Center;
				DebugOverlay.Line( centera * CellScale, centerb * CellScale, 0, false );
			}
		}

		foreach ( var room in rooms )
		{
			DebugOverlay.Sphere( room.WorldRect.Center, 20f, Color.Red );
		}

	}

}
