
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

internal class Minimap : Panel
{

	private TextureDrawer Drawer;
	private Panel PlayerIcon;

	public Minimap()
	{
		PlayerIcon = Add.Panel( "player-icon" );

		var size = 512;
		var texture = new Texture2DBuilder()
			.WithScreenFormat()
			.WithSize( size )
			.WithData( new byte[size * size * 4] )
			.Finish();

		Style.SetBackgroundImage( texture );
		Style.BackgroundSizeX = Length.Percent( 100 );
		Style.BackgroundSizeY = Length.Percent( 100 );

		Drawer = new( texture );

		DrawDungeon();
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Local.Pawn.IsValid() ) 
			return;

		var pos = WorldToDrawerFraction( Local.Pawn.Position );
		PlayerIcon.Style.Position = PositionMode.Absolute;
		PlayerIcon.Style.Left = Length.Fraction( pos.x );
		PlayerIcon.Style.Top = Length.Fraction( pos.y );
	}

	private void DrawDungeon()
	{
		var dungeon = DungeonEntity.Current;

		foreach ( var room in dungeon.Rooms )
		{
			var rect = WorldToDrawerRect( room.WorldRect );
			Drawer.DrawRectangle( rect.Shrink( 2 ), Color.White );
			Drawer.DrawFilledRectangle( rect.Shrink( 3 ), Color.Black.WithAlpha( .5f ) );
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
