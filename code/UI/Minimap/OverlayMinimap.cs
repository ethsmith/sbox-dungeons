
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

internal class OverlayMinimap : DungeonsPanel
{

	protected override InputButton ToggleButton => InputButton.Score;
	protected override DisplayModes DisplayMode => DisplayModes.Toggle;

	public Minimap Minimap { get; set; }

	public OverlayMinimap()
	{
		Style.Position = PositionMode.Absolute;
		Style.Left = 0;
		Style.Top = 0;
		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );

		var inner = AddChild<Panel>( "inner" );
		Minimap = inner.AddChild<Minimap>();
		Minimap.DrawOpacity = .35f;
	}

}
