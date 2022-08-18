
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

internal class Minimap : Panel
{

	private TextureDrawer Drawer;
	private Panel PlayerIcon;
	private Panel Map;
	private Panel MapContainer;
	private int DungeonId;

	public Minimap()
	{
		MapContainer = Add.Panel( "map-container" );
		MapContainer.Style.Overflow = OverflowMode.Hidden;
		MapContainer.Style.Width = Length.Percent( 100 );
		MapContainer.Style.Height = Length.Percent( 100 );

		Map = MapContainer.Add.Panel( "map" );
		PlayerIcon = Map.Add.Panel( "player-icon" );

		var size = 512;
		var texture = new Texture2DBuilder()
			.WithScreenFormat()
			.WithSize( size )
			.WithData( new byte[size * size * 4] )
			.Finish();

		Map.Style.SetBackgroundImage( texture );
		Map.Style.BackgroundSizeX = Length.Percent( 100 );
		Map.Style.BackgroundSizeY = Length.Percent( 100 );
		Map.Style.Width = Length.Percent( 100 );
		Map.Style.Height = Length.Percent( 100 );

		Drawer = new( texture );
	}

	public override void Tick()
	{
		base.Tick();

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
		var dungeon = DungeonEntity.Current;

		foreach ( var room in dungeon.Rooms )
		{
			var rect = WorldToDrawerRect( room.WorldRect );
			Drawer.DrawFilledRectangle( rect, Color.Gray.WithAlpha( .05f ) );
			Drawer.DrawRectangle( rect, Color.Gray );
			Drawer.DrawRectangle( rect.Shrink( 1 ), Color.Gray );
			Drawer.DrawRectangle( rect.Shrink( 2 ), Color.Gray );
		}

		foreach( var route in dungeon.Routes )
		{
			foreach( var door in route.Doors )
			{
				var rect = WorldToDrawerRect( door.WorldRect );
				Drawer.DrawFilledRectangle( rect, Color.Gray.WithAlpha( .05f ) );
			}
		}

		Drawer.Apply();
	}

	private Rect WorldToDrawerRect( Rect rect )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		var sizeFraction = rect.Size / worldrect.Size;
		var posFraction = rect.Position / worldrect.Size;
		var pos = Drawer.Size * posFraction;
		var size = Drawer.Size * sizeFraction;

		pos = new Vector2( Drawer.Size.x - pos.y - size.y, Drawer.Size.y - pos.x - size.x );
		size = new Vector2( size.y, size.x );

		return new Rect( pos, size );
	}

	private Vector3 WorldToDrawerPosition( Vector3 position )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		var posFraction = position / worldrect.Size;
		var pos = Drawer.Size * posFraction;
		return new Vector2( Drawer.Size.x - pos.y, Drawer.Size.y - pos.x );
	}

	private Vector2 WorldToDrawerFraction( Vector3 position )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		var posFraction = position / worldrect.Size;

		return new Vector2( 1f - posFraction.y, 1f - posFraction.x );
	}

}
