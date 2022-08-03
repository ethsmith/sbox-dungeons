
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

[UseTemplate]
internal class Inventory : DungeonsPanel
{

	public StashPanel Stash { get; set; }

	private bool StashAssigned;

	public override void Tick()
	{
		base.Tick();

		if ( StashAssigned || Local.Pawn is not Player pl || !pl.Stash.IsValid() ) 
			return;

		if ( Stash?.Stash == pl.Stash ) 
			return;

		Stash.Stash = pl.Stash;
	}

}
