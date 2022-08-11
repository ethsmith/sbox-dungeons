
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Dungeons.UI;

internal class StashPanel2 : Panel
{

	public int CellCount { get; set; } = 84;
	public int Columns { get; set; } = 12;
	public int Margin { get; set; } = 1;

	private Panel Cells;
	private int CellsHash;
	private bool CellsNeedLayout;

	public override void Tick()
	{
		base.Tick();

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
