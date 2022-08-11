
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

	public Inventory()
	{
		StashManager.Current?.Register( Stash );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player pl || !pl.Stash.IsValid() )
			return;

		foreach ( var item in pl.Stash.Items )
		{
			if ( Stash.Contains( item.NetworkIdent ) ) 
				continue;

			var icon = new StashableIcon( item );
			Stash.InsertItem( item.NetworkIdent, () => item.Detail.StashSlot, icon );
		}
	}

}
