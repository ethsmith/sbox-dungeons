
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
	}

}
