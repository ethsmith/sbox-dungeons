
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
	public int Columns = 8;
	public bool DraggingEnabled = true;

	private Panel Canvas;
	private Panel DragCanvas;
	private int DragBeginSlot;
	private Panel DragPanel;
	private Vector2 DragPanelOffset;

	public StashEntity Stash { get; set; }

	private int activehash = -1233;
	public override void Tick()
	{
		base.Tick();

		if ( !Stash.IsValid() )
		{
			//Error();
			return;
		}

		var hash = HashCode.Combine( CellSize, Columns );
		foreach( var item in Stash.Items )
		{
			hash = HashCode.Combine( hash, item.NetworkIdent, item.Detail.StashSlot );
		}

		if ( activehash == hash ) return;
		activehash = hash;

		Build();
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
		}

		base.SetProperty( name, value );
	}

	[Event.Hotload]
	private void Build()
	{
		DeleteChildren( true );

		Canvas = Add.Panel( "slots" );
		DragCanvas = Add.Panel();
		DragCanvas.Style.Position = PositionMode.Absolute;

		for( int i = 0; i < Stash.SlotCount; i++ )
		{
			var slot = Canvas.Add.Panel( "slot" );
			var item = Stash.Items.FirstOrDefault( x => x.Detail.StashSlot == i );
			if ( item == null ) continue;
			slot.AddChild( new StashableIcon( item ) );
		}

		layouthash = -1;
	}

	private int layouthash = -1;
	private GridLayout layout = default;
	public override void OnLayout( ref Rect layoutRect )
	{
		base.OnLayout( ref layoutRect );

		var hash = layoutRect.GetHashCode();
		if ( hash == layouthash ) return;

		layouthash = hash;
		layout = new( layoutRect, Columns, 0, 4, ScaleFromScreen );

		foreach( var child in Canvas.Children )
		{
			layout.Position( child.SiblingIndex - 1, child, CellSize );
		}

		if ( CellSize <= 0 ) return;

		var rows = MathX.CeilToInt( (float)Canvas.Children.Count() / Columns );
		var width = CellSize * Columns;
		var height = rows * CellSize;

		Style.Width = width + 4;
		Style.Height = height + 4;
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

		dragitem.SetStashSlot( dropslot );
	}

}
