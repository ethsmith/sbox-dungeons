
using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Dungeons.UI;

internal class ItemLabel : Panel
{

	public Stashable Item { get; }

	public ItemLabel( Stashable item )
	{
		Item = item;

		Add.Label( $"Item #{item.NetworkIdent}" );
		Style.Position = PositionMode.Absolute;
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

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );


	}

}
