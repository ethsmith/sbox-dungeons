
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

[UseTemplate]
internal class Inventory : DungeonsPanel
{

	public StashPanel Stash { get; set; }
	public StashPanel StashEquipment { get; set; }

	protected override CursorModes CursorMode => CursorModes.Hover;
	protected override DisplayModes DisplayMode => DisplayModes.Toggle;
	protected override InputButton ToggleButton => InputButton.Score;

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player pl )
			return;

		if ( !pl.Stash.IsValid() )
			return;

		if ( !pl.StashEquipment.IsValid() )
			return;

		StashManager.Current?.Register( Stash, pl.Stash );
		StashManager.Current?.Register( StashEquipment, pl.StashEquipment );
	}

}
