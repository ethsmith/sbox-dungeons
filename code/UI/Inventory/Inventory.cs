
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

[UseTemplate]
internal class Inventory : DungeonsPanel
{

	public StashPanel Stash { get; set; }

	protected override CursorModes CursorMode => CursorModes.Hover;
	protected override DisplayModes DisplayMode => DisplayModes.Toggle;
	protected override InputButton ToggleButton => InputButton.Score;

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player pl || !pl.Stash.IsValid() ) 
			return;

		if ( Stash?.Stash == pl.Stash ) 
			return;

		Stash.Stash = pl.Stash;
	}

}
