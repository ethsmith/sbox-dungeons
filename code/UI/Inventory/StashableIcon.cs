
using Dungeons.Data;
using Dungeons.Stash;
using Dungeons.Items;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Dungeons.UI;

internal class StashableIcon : Panel
{

	public Stashable Stashable { get; private set; }

	private ItemResource Item;

	public StashableIcon( Stashable stashable )
	{
		Stashable = stashable;

		Item = ResourceLibrary.GetAll<ItemResource>().FirstOrDefault( x => x.ResourceName == stashable.Detail.Identity );
		if ( Item == null )
		{
			Add.Label( $"#{stashable.NetworkIdent}, #{stashable.Detail.StashSlot}" );
			return;
		}

		Style.SetBackgroundImage( Item.Icon );
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		var name = Item?.DisplayName ?? "Unknown";
		var durability = Stashable.Detail.Durability;
		var maxDurability = Item?.Durability ?? 0;
		var quantity = Stashable.Detail.Quantity;

		Tippy.Create( this, Tippy.Pivots.TopRight )
			.WithMessage( @$"{name}
Durability: {durability}/{maxDurability}
Quantity: {quantity}" );
	}

}
