
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.UI;

internal class StashPanel : Panel
{

	public int CellCount = 84;
	public int Columns = 12;
	public int Margin = 1;

	private Panel Cells;
	private int CellsHash;
	private bool CellsNeedLayout;
	private int CurrentLayout;
	private List<(int, Func<int>, Panel)> Items = new();

	public override void Tick()
	{
		base.Tick();

		if ( Cells != null )
		{
			PlaceItems();
		}

		var layout = HashCode.Combine( Margin, CellCount, Columns, Cells?.Box?.Rect );
		if ( layout != CurrentLayout )
		{
			CurrentLayout = layout;
			CellsNeedLayout = true;
		}

		var cellhash = HashCode.Combine( Columns, Margin );
		if ( cellhash == CellsHash ) return;
		CellsHash = cellhash;

		BuildCells();
	}

	public override void FinalLayout()
	{
		base.FinalLayout();

		if ( !CellsNeedLayout ) return;
		CellsNeedLayout = false;

		LayoutCells();
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name.ToLower() )
		{
			case "cells":
				int.TryParse( value, out CellCount );
				return;
			case "columns":
				int.TryParse( value, out Columns );
				return;
			case "margin":
				int.TryParse( value, out Margin );
				return;
		}

		base.SetProperty( name, value );
	}

	public bool Contains( int itemId )
	{
		return Items.Any( x => x.Item1 == itemId );
	}

	public void InsertItem( int itemId, Func<int> itemSlot, Panel panel )
	{
		if ( Contains( itemId ) )
		{
			Log.Error( "Inserting an already inserted item" );
			return;
		}

		Items.Add( (itemId, itemSlot, panel) );
	}

	public void RemoveItem( int itemId )
	{
		if ( !Contains( itemId ) )
		{
			Log.Error( "Removing an item that hasn't been inserted" );
			return;
		}

		var item = Items.First( x => x.Item1 == itemId );
		item.Item3.Delete( true );
		Items.Remove( item );
	}

	public bool TryGetItem( int cellIndex, out int itemId )
	{
		itemId = -1;

		foreach( var item in Items )
		{
			if( item.Item2() == cellIndex )
			{
				itemId = item.Item1;
				return true;
			}
		}

		return false;
	}

	public IEnumerable<int> AllItems()
	{
		foreach( var item in Items )
		{
			yield return item.Item1;
		}
	}

	private void BuildCells()
	{
		Cells?.Delete( true );
		Cells = Add.Panel( "cells" );
		Cells.Style.FlexWrap = Wrap.Wrap;
		Cells.Style.JustifyContent = Justify.Center;
		Cells.Style.Width = Length.Percent( 100 );

		for ( int i = 0; i < CellCount; i++ )
		{
			Cells.AddChild<StashCell>();
		}

		CellsNeedLayout = true;
	}

	private void LayoutCells()
	{
		var width = MathX.FloorToInt( Cells.Box.RectInner.width * ScaleFromScreen );
		var marginSpace = MathX.CeilToInt( Margin * (Columns + 1) * 2 * ScaleFromScreen );
		width -= marginSpace;

		var cellsize = width / Columns;

		foreach ( var child in Cells.Children )
		{
			child.Style.Width = cellsize;
			child.Style.Height = cellsize;
			child.Style.Margin = Margin;
		}
	}

	private void PlaceItems()
	{
		foreach ( var item in Items )
		{
			var cell = Cells.GetChild( item.Item2() );
			if ( cell == null ) continue;
			item.Item3.Parent = cell;
		}
	}

}
