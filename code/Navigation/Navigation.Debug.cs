
using Sandbox;
using SandboxEditor;

namespace Dungeons;

internal partial class NavigationEntity
{

	[DebugOverlay( "navigation", "Navigation", "square" )]
	public static void NavigationDebugOverlay()
	{
		if ( !Current.IsValid() )
			return;

		for ( int x = 0; x < Current.Grid.Length; x++ )
		{
			var pos = Current.GetPosition( x ) * Current.CellSize;
			var worldpos = new Vector3( pos.x, pos.y, 1 );
			var color = Current.Grid[x] == 1 ? Color.Green : Color.White;
			DebugOverlay.Sphere( worldpos, 1f, color );
		}

		foreach ( var pos in Current.GetAgentPositions() )
		{
			DebugOverlay.Text( pos.Item1.ToString(), pos.Item2 );
			DebugOverlay.Sphere( pos.Item2, 3f, Color.White, 0, false );
		}

		if ( Local.Pawn is Player pl )
		{
			var agent = pl.Agent;

			if ( agent.TotalWaypoints <= 1 )
				return;

			for ( int i = 0; i < agent.TotalWaypoints - 1; i++ )
			{
				var p1 = agent.Waypoints[i];
				var p2 = agent.Waypoints[i + 1];
				DebugOverlay.Sphere( p1.WithZ( 1 ), 2f, Color.Red );
				DebugOverlay.Sphere( p2.WithZ( 1 ), 2f, Color.Red );
				DebugOverlay.Line( p1.WithZ( 1 ), p2.WithZ( 1 ), Color.White );
			}
		}
	}

}
