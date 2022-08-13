
namespace Dungeons.Stash;

abstract class StashConstraint
{
	public StashEntity Stash;
	public abstract bool AcceptsItem( Stashable item, int cell );
}
