
using System.Linq;

namespace Dungeons.Stash;

internal class OccupiedConstraint : StashConstraint
{

	public override bool AcceptsItem( Stashable item, int cell )
	{
		if ( cell == item.Detail.StashSlot && item.Parent == Stash ) 
			return true;

		return !Stash.Items.Any( x => x.Detail.StashSlot == cell );
	}

}
