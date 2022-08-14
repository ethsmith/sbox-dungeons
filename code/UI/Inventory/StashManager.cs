
using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.UI;

internal class StashManager : Panel
{

	public static StashManager Current;

	private Stashable PickedItem;
	private List<(StashPanel, StashEntity)> Stashes = new();

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

	public void Register( StashPanel stashPanel, StashEntity stashEntity )
	{
		if ( Stashes.Any( x => x.Item1 == stashPanel && x.Item2 == stashEntity ) )
			return;

		Stashes.RemoveAll( x => x.Item1 == stashPanel || x.Item2 == stashEntity );
		Stashes.Add( (stashPanel, stashEntity) );

		stashPanel.AddEventListener( "onmousedown", HandleCellClicked );
		stashPanel.AddEventListener( "onmouseover", HandleCellHovered );
		stashPanel.AddEventListener( "onmouseout", HandleCellExit );
	}

	private void HandleCellHovered( PanelEvent e )
	{
		if ( PickedItem == null ) return;

		var targetStash = e.Target.Ancestors.OfType<StashPanel>().FirstOrDefault();
		var cell = e.Target.AncestorsAndSelf.OfType<StashCell>().FirstOrDefault();

		if ( targetStash == null || cell == null )
			return;

		var cellIndex = cell.SiblingIndex - 1;
		var hoveredStash = Stashes.Where( x => x.Item1 == targetStash ).First().Item2;
		var slotIsOkay = hoveredStash.AcceptsItem( PickedItem, cellIndex );

		if ( slotIsOkay )
		{
			cell.AddClass( "constraint-passes" );
		}
		else
		{
			cell.AddClass( "constraint-fails" );
		}
	}

	private void HandleCellExit(PanelEvent e )
	{
		if ( PickedItem == null ) return;

		var targetStash = e.Target.Ancestors.OfType<StashPanel>().FirstOrDefault();
		var cell = e.Target.AncestorsAndSelf.OfType<StashCell>().FirstOrDefault();

		if ( targetStash == null || cell == null )
			return;

		cell.RemoveClass( "constraint-passes" );
		cell.RemoveClass( "constraint-fails" );
	}

	private void HandleCellClicked( PanelEvent e )
	{
		var targetStash = e.Target.Ancestors.OfType<StashPanel>().FirstOrDefault();
		var cell = e.Target.AncestorsAndSelf.OfType<StashCell>().FirstOrDefault();

		if ( targetStash == null || cell == null )
			return;

		var cellIndex = cell.SiblingIndex - 1;

		if ( PickedItem.IsValid() )
		{
			var toStash = Stashes.Where( x => x.Item1 == targetStash ).First().Item2;
			if ( !toStash.AcceptsItem( PickedItem, cellIndex ) )
			{
				return;
			}

			cell.RemoveClass( "constraint-passes" );
			cell.RemoveClass( "constraint-fails" );

			StashEntity.ServerCmd_MoveItem( toStash.NetworkIdent, PickedItem.NetworkIdent, cellIndex );

			PickedItem = null;

			return;
		}

		if ( targetStash.TryGetItem( cellIndex, out int itemId ) )
		{
			PickedItem = Entity.FindByIndex( itemId ) as Stashable;
		}
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		if ( PickedItem.IsValid() )
		{
			Render.Draw2D.Color = Color.White.WithAlpha( .25f );
			Render.Draw2D.Box( new Rect( Mouse.Position - 24, 48 ) );
		}
	}

	public override void Tick()
	{
		base.Tick();

		foreach ( var item in Stashes )
		{
			EnsureItems( item.Item1, item.Item2 );
		}
	}

	private void EnsureItems( StashPanel stashPanel, StashEntity stashEntity )
	{
		foreach ( var item in stashPanel.AllItems().ToList() )
		{
			if ( !stashEntity.Items.Any( x => x.NetworkIdent == item ) )
			{
				stashPanel.RemoveItem( item );
			}
		}

		foreach ( var item in stashEntity.Items )
		{
			if ( stashPanel.Contains( item.NetworkIdent ) )
				continue;

			var icon = new StashableIcon( item );
			stashPanel.InsertItem( item.NetworkIdent, () => item.Detail.StashSlot, icon );
		}
	}

	private bool ClearPrimaryAttack;
	[Event.BuildInput]
	private void OnBuildInput( InputBuilder b )
	{
		if ( b.Released( InputButton.PrimaryAttack ) )
		{
			ClearPrimaryAttack = false;
		}

		if ( ClearPrimaryAttack )
		{
			b.ClearButton( InputButton.PrimaryAttack );
		}

		if ( !PickedItem.IsValid() )
			return;

		if ( !b.Down( InputButton.PrimaryAttack ) )
			return;

		StashEntity.ServerCmd_DropItem( PickedItem.NetworkIdent );

		PickedItem = null;
		ClearPrimaryAttack = true;
		b.ClearButton( InputButton.PrimaryAttack );
	}

}
