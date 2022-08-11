
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Dungeons.UI;

internal class StashPanel2 : Panel
{

	public int CellCount = 84;
	public int Columns = 12;
	public int Margin = 1;

	private Panel Cells;
	private int CellsHash;
	private bool CellsNeedLayout;
	private int CurrentLayout;

	public override void Tick()
	{
		base.Tick();

		var layout = HashCode.Combine( Margin, CellCount, Columns, Cells?.Box?.Rect );
		if( layout != CurrentLayout )
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
			case "cellcount":
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

	private void BuildCells()
	{
		Cells?.Delete( true );
		Cells = Add.Panel( "cells" );
		Cells.Style.FlexWrap = Wrap.Wrap;
		Cells.Style.JustifyContent = Justify.Center;
		Cells.Style.Width = Length.Percent( 100 );

		for ( int i = 0; i < CellCount; i++ )
		{
			Cells.Add.Panel( "cell" ).Add.Label( (i + 1).ToString() );
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

}
