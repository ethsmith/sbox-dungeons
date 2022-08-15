
using Dungeons.Stash;
using Dungeons.Items;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Dungeons.UI;

internal class ItemIcon : Panel
{

	public Stashable Stashable { get; private set; }

	private ItemResource Item;

	public ItemIcon( Stashable stashable )
	{
		Stashable = stashable;

		Item = ResourceLibrary.GetAll<ItemResource>().FirstOrDefault( x => x.ResourceName == stashable.ItemData.Identity );
		if ( Item == null )
		{
			Add.Label( $"#{stashable.NetworkIdent}, #{stashable.ItemData.StashSlot}" );
			return;
		}

		Style.SetBackgroundImage( Item.Icon );
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		Tippy.Create( this, Tippy.Pivots.TopRight ).WithContent( new ItemTooltip( Stashable.ItemData.Data ) );
	}

}
