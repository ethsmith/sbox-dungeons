
using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.UI;

internal class ItemLabel : Panel
{

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

	// todo: much cleaner in OnMouseDown if I sort out other panels taking pointer priority
	private static List<ItemLabel> All = new();
	private static bool ClearPrimaryAttack;
	[Event.BuildInput]
	private static void OnBuildInput( InputBuilder b )
	{
		var hovered = All.FirstOrDefault( x => x.HasHovered );
		if( hovered != null && b.Pressed( InputButton.PrimaryAttack ) )
		{
			(Local.Pawn as Player).Stash.Add( hovered.Item );
			ClearPrimaryAttack = true;
		}

		if ( b.Released( InputButton.PrimaryAttack ) )
		{
			ClearPrimaryAttack = false;
		}

		if( ClearPrimaryAttack )
		{
			b.ClearButton( InputButton.PrimaryAttack );
		}
	}

}
