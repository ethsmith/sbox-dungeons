
using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Dungeons.UI;

[UseTemplate]
internal class StashPanel : DungeonsPanel
{

	public int CellSize = 50;
	public int Spacing = 4;
	public int Columns = 8;
	public bool DraggingEnabled = true;

	private Panel Canvas;
	private Panel DragCanvas;
	private int DragBeginSlot;
	private Panel DragPanel;
	private Vector2 DragPanelOffset;

	public StashEntity Stash { get; set; }

	private int activeStashHash = -1233;
	private int activehash = -1233;
	public override void Tick()
	{
		base.Tick();

		if ( !Stash.IsValid() )
		{
			//Error();
			return;
		}

		var stashHash = HashCode.Combine( CellSize, Columns );
		if( stashHash != activeStashHash )
		{
			activeStashHash = stashHash;
			BuildCells();
			return;
		}

		var itemHash = -1233;
		foreach( var item in Stash.Items )
		{
			itemHash = HashCode.Combine( itemHash, item.NetworkIdent, item.Detail.StashSlot );
		}

		if( itemHash != activehash )
		{
			activehash = itemHash;
			BuildItems();
		}
	}

	Panel DropTarget;
	[Event.Frame]
	private void UpdateDropTarget()
	{
		DropTarget?.RemoveClass( "drop-target" );

		var cellidx = layout.SlotIndex( MousePosition );
		if( cellidx == -1 )
		{
			DropTarget = null;
			return;
		}

		DropTarget = Canvas.GetChild( cellidx );
		DropTarget?.AddClass( "drop-target" );
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name.ToLower() ) 
		{
			case "cellsize":
				int.TryParse( value, out CellSize );
				return;
			case "columns":
				int.TryParse( value, out Columns );
				return;
			case "spacing":
				int.TryParse( value, out Spacing );
				return;
		}

		base.SetProperty( name, value );
	}

	[Event.Hotload]
	private void BuildCells()
	{
		DeleteChildren( true );

		Canvas = Add.Panel( "slots" );
		DragCanvas = Add.Panel();
		DragCanvas.Style.Position = PositionMode.Absolute;

		for ( int i = 0; i < Stash.SlotCount; i++ )
		{
			Canvas.Add.Panel( "slot" );
		}

		layouthash = -1;
	}

	private void BuildItems()
	{
		foreach( var slot in Canvas.Children )
		{
			slot.DeleteChildren( true );
		}

		foreach( var item in Stash.Items )
		{
			var slotIdx = item.Detail.StashSlot;
			var slot = Canvas.GetChild( slotIdx );
			if ( slot == null )
			{
				Log.Error( "Item is in missing slot??" );
				continue;
			}
			slot.AddChild( new StashableIcon( item ) );
		}
	}

	private int layouthash = -1;
	private GridLayout layout = default;
	public override void OnLayout( ref Rect layoutRect )
	{
		base.OnLayout( ref layoutRect );

		var hash = layoutRect.GetHashCode();
		if ( hash == layouthash ) return;

		layouthash = hash;
		layout = new( layoutRect, Columns, 0, Spacing, ScaleFromScreen );

		foreach( var child in Canvas.Children )
		{
			layout.Position( child.SiblingIndex - 1, child, CellSize );
		}

		if ( CellSize <= 0 ) return;

		var rows = MathX.CeilToInt( (float)Canvas.Children.Count() / Columns );
		var width = CellSize * Columns;
		var height = rows * CellSize;

		Style.Width = width + Spacing;
		Style.Height = height + Spacing;

		BuildItems();
	}

	protected override void OnDragBegin( MousePanelEvent e )
	{
		base.OnDragBegin( e );

		if ( !DraggingEnabled ) return;

		DragBeginSlot = layout.SlotIndex( MousePosition );

		var itemIcon = Canvas.Children.ElementAtOrDefault( DragBeginSlot )?.Children.FirstOrDefault();
		if ( itemIcon == null ) return;

		DragCanvas.Style.Width = itemIcon.Parent.Box.RectOuter.width * ScaleFromScreen;
		DragCanvas.Style.Height = itemIcon.Parent.Box.RectOuter.height * ScaleFromScreen;

		DragPanelOffset = itemIcon.MousePosition;
		DragPanel = itemIcon;
		DragPanel.SetClass( "dragging", true );
		DragPanel.Parent = DragCanvas;

		DragPanel.Style.Left = (DragCanvas.MousePosition.x - DragPanelOffset.x) * ScaleFromScreen;
		DragPanel.Style.Top = (DragCanvas.MousePosition.y - DragPanelOffset.y) * ScaleFromScreen;
	}

	protected override void OnDrag( MousePanelEvent e )
	{
		base.OnDrag( e );

		if ( DragPanel == null ) return;

		DragPanel.Style.Left = (DragCanvas.MousePosition.x - DragPanelOffset.x) * ScaleFromScreen;
		DragPanel.Style.Top = (DragCanvas.MousePosition.y - DragPanelOffset.y) * ScaleFromScreen;
	}

	protected override void OnDragEnd( MousePanelEvent e )
	{
		base.OnDragEnd( e );

		if ( !DraggingEnabled ) return;

		var targetpanel = DragPanel;
		DragPanel = null;

		if ( targetpanel != null )
		{
			targetpanel.Style.Left = 0;
			targetpanel.Style.Top = 0;
			targetpanel.SetClass( "dragging", false );

			var slot = Canvas.Children.ElementAtOrDefault( DragBeginSlot );
			if ( slot != null ) targetpanel.Parent = slot;
		}

		var dropslot = layout.SlotIndex( MousePosition );
		if ( dropslot == DragBeginSlot ) return;
		if ( dropslot == -1 ) return;
		if ( dropslot >= Stash.SlotCount ) return;

		var dragitem = Stash.Items.FirstOrDefault( x => x.Detail.StashSlot == DragBeginSlot );
		if ( dragitem == null ) return;

		Canvas.GetChild( dragitem.Detail.StashSlot )?.DeleteChildren();

		dragitem.SetStashSlot( dropslot );
	}

}
