
using Sandbox;

namespace Dungeons.UI;

internal class Inventory : DungeonsPanel
{

	private StashPanel Stash;

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player pl ) return;
		if ( !pl.Stash.IsValid() ) return;
		if ( Stash != null ) return;

		Stash = new( 84, 8, true, pl.Stash );
		Stash.Parent = this;
	}

}
