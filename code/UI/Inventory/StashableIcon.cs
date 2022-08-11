
using Dungeons.Stash;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Dungeons.UI;

internal class StashableIcon : Panel
{

	public Stashable Stashable { get; private set; }

	public StashableIcon( Stashable stashable )
	{
		Stashable = stashable;

		Add.Label( $"#{stashable.NetworkIdent}, #{stashable.Detail.StashSlot}" );
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		if ( HasClass( "dragging" ) ) return;

		Tippy.Create( this, Tippy.Pivots.TopRight )
			.WithMessage( @$"Item #{Stashable.NetworkIdent}
Durability: {Stashable.Detail.Durability}
Quantity: {Stashable.Detail.Quantity}" );
	}

}
