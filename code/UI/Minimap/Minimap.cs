
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Dungeons.UI;

internal class Minimap : Panel
{

	private Panel PlayerIcon;
	private Panel Map;
	private Panel MapContainer;
	private int DungeonId;

	private static TextureDrawer ExploredMinimap;
	private static TextureDrawer FullMinimap;
	private static HashSet<int> ExploredCells = new();

	public Minimap()
	{
		MapContainer = Add.Panel( "map-container" );
		MapContainer.Style.Overflow = OverflowMode.Hidden;
		MapContainer.Style.Width = Length.Percent( 100 );
		MapContainer.Style.Height = Length.Percent( 100 );

		Map = MapContainer.Add.Panel( "map" );
		PlayerIcon = Map.Add.Panel( "player-icon" );

		var size = 512;

		ExploredMinimap ??= new( new Texture2DBuilder()
			.WithScreenFormat()
			.WithSize( size )
			.WithData( new byte[size * size * 4] )
			.Finish() );

		FullMinimap ??= new( new Texture2DBuilder()
			.WithScreenFormat()
			.WithSize( size )
			.WithData( new byte[size * size * 4] )
			.Finish() );

		Map.Style.SetBackgroundImage( ExploredMinimap.Texture );
		Map.Style.BackgroundSizeX = Length.Percent( 100 );
		Map.Style.BackgroundSizeY = Length.Percent( 100 );
		Map.Style.Width = Length.Percent( 100 );
		Map.Style.Height = Length.Percent( 100 );
	}

	public override void Tick()
	{
		base.Tick();

		UpdateExploredCells();

		if ( !Local.Pawn.IsValid() )
			return;

		if ( !DungeonEntity.Current.IsValid() )
			return;

		if ( DungeonId != DungeonEntity.Current.NetworkIdent )
		{
			DungeonId = DungeonEntity.Current.NetworkIdent;
			DrawDungeon();
		}

		var pos = WorldToDrawerFraction( Local.Pawn.Position );
		PlayerIcon.Style.Position = PositionMode.Absolute;
		PlayerIcon.Style.Left = Length.Fraction( pos.x );
		PlayerIcon.Style.Top = Length.Fraction( pos.y );

		var sx = pos.x * Map.Box.RectInner.Width * ScaleFromScreen;
		var sy = pos.y * Map.Box.RectInner.Height * ScaleFromScreen;

		Map.Style.Left = -sx + ( Box.RectInner.Width * .5f * ScaleFromScreen );
		Map.Style.Top = -sy + ( Box.RectInner.Height * .5f * ScaleFromScreen );
	}

	private void DrawDungeon()
	{
		ExploredCells.Clear();
		ExploredMinimap.Empty();
		ExploredMinimap.Apply();

		var dungeon = DungeonEntity.Current;

		foreach ( var room in dungeon.Rooms )
		{
			var rect = WorldToDrawerRect( room.WorldRect );
			FullMinimap.DrawFilledRectangle( rect, Color.Gray.WithAlpha( .05f ) );
			FullMinimap.DrawRectangle( rect, Color.Gray );
			FullMinimap.DrawRectangle( rect.Shrink( 1 ), Color.Gray );
			FullMinimap.DrawRectangle( rect.Shrink( 2 ), Color.Gray );
		}

		foreach( var route in dungeon.Routes )
		{
			foreach( var door in route.Doors )
			{
				var rect = WorldToDrawerRect( door.WorldRect );
				FullMinimap.DrawFilledRectangle( rect, Color.Gray.WithAlpha( .05f ) );
			}
		}

		FullMinimap.Apply();
	}

	private Rect WorldToDrawerRect( Rect rect )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		var sizeFraction = rect.Size / worldrect.Size;
		var posFraction = rect.Position / worldrect.Size;
		var pos = ExploredMinimap.Size * posFraction;
		var size = ExploredMinimap.Size * sizeFraction;

		pos = new Vector2( ExploredMinimap.Size.x - pos.y - size.y, ExploredMinimap.Size.y - pos.x - size.x );
		size = new Vector2( size.y, size.x );

		return new Rect( pos, size );
	}

	private Vector3 WorldToDrawerPosition( Vector3 position )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		var posFraction = position / worldrect.Size;
		var pos = ExploredMinimap.Size * posFraction;
		return new Vector2( ExploredMinimap.Size.x - pos.y, ExploredMinimap.Size.y - pos.x );
	}

	private Vector2 WorldToDrawerFraction( Vector3 position )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		var posFraction = position / worldrect.Size;

		return new Vector2( 1f - posFraction.y, 1f - posFraction.x );
	}

	private void UpdateExploredCells()
	{
		if ( !Local.Pawn.IsValid() )
			return;

		if ( !DungeonEntity.Current.IsValid() )
			return;

		const int chunksize = 48;
		const int seedistance = 300;
		const int iter = seedistance / chunksize;

		var needsupdate = false;
		var playerpos = Local.Pawn.Position.SnapToGrid( chunksize );

		for( int x = -iter; x < iter; x++ )
		{
			for( int y = -iter; y < iter; y++ )
			{
				var samplepoint = playerpos + new Vector3( x * chunksize, y * chunksize, 0 );
				var idx = (int)samplepoint.x * 10000 + (int)samplepoint.y;

				if ( ExploredCells.Contains( idx ) ) 
					continue;

				var dist = playerpos.Distance( samplepoint );
				if ( dist > seedistance )
					continue;

				var localrect = WorldToDrawerRect( new Rect( samplepoint.x, samplepoint.y, chunksize, chunksize ) );
				var copyx = (int)Math.Round( localrect.Position.x );
				var copyy = (int)Math.Round( localrect.Position.y );
				var copyw = (int)Math.Round( localrect.Size.x );
				var copyh = (int)Math.Round( localrect.Size.y );

				needsupdate = true;
				ExploredCells.Add( idx );
				ExploredMinimap.CopyPixels( FullMinimap, copyx, copyy, copyw, copyh );
			}
		}

		if( needsupdate ) ExploredMinimap.Apply();
	}

}
