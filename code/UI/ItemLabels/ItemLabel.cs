
using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.UI;

internal class ItemLabel : Panel
{

	private static List<ItemLabel> All = new();

	public Stashable Item { get; }

	public ItemLabel( Stashable item )
	{
		Item = item;

		Add.Label( $"Item #{item.NetworkIdent}" );
		Style.Position = PositionMode.Absolute;

		All.Add( this );
	}

	public override void Tick()
	{
		base.Tick();

		if( !Item.IsValid() || Item.Parent != null )
		{
			Delete( true );
			return;
		}

		var position = Item.Position.ToScreen();
		Style.Left = Length.Fraction( position.x );
		Style.Top = Length.Fraction( position.y );
	}

	public override void OnDeleted()
	{
		base.OnDeleted();

		All.Remove( this );
	}

	public static Stashable HoveredItem()
	{
		return All.FirstOrDefault( x => x.HasHovered )?.Item;
	}

}
