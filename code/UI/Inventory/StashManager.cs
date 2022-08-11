
using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Dungeons.UI;

internal class StashManager : Panel
{

	public static StashManager Current;

	private Stashable PickedItem;

	public StashManager()
	{
		Current = this;
		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );
		Style.Left = 0;
		Style.Top = 0;
		Style.ZIndex = 999;
		Style.Position = PositionMode.Absolute;
		Style.PointerEvents = "none";
	}

	public void Register( StashPanel stashPanel )
	{
		stashPanel.AddEventListener( "onmousedown", HandleCellClicked );
	}

	private void HandleCellClicked( PanelEvent e )
	{
		var stashPanel = e.Target.Ancestors.OfType<StashPanel>().FirstOrDefault();
		var cell = e.Target.AncestorsAndSelf.OfType<StashCell>().FirstOrDefault();

		if ( stashPanel == null || cell == null )
			return;

		var cellIndex = cell.SiblingIndex - 1;

		if ( PickedItem.IsValid() )
		{
			PickedItem.SetStashSlot( cellIndex );
			PickedItem = null;
			return;
		}

		if ( stashPanel.TryGetItem( cellIndex, out int itemId ) )
		{
			PickedItem = Entity.FindByIndex( itemId ) as Stashable;
		}
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		if( PickedItem.IsValid() )
		{
			Render.Draw2D.Color = Color.Black;
			Render.Draw2D.Box( new Rect( Mouse.Position - 24, 48 ) );
		}
	}

}
