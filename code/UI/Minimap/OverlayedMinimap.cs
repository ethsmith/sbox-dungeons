
using Sandbox;

namespace Dungeons.UI;

internal class OverlayedMinimap : DungeonsPanel
{

	protected override DisplayModes DisplayMode => DisplayModes.Toggle;
	protected override InputButton ToggleButton => InputButton.Score;
	
	public OverlayedMinimap()
	{
		AddChild<Minimap>().AddClass( "overlay" );
	}

}
