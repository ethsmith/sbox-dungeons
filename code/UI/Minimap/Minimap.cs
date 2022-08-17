
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

internal class Minimap : DungeonsPanel
{

	protected override CursorModes CursorMode => CursorModes.None;
	protected override InputButton ToggleButton => InputButton.Score;
	protected override DisplayModes DisplayMode => DisplayModes.Toggle;

	public Minimap()
	{
		SetClass( "open", true );
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		var draw = Render.Draw2D;
		var dungeon = DungeonEntity.Current;

		foreach ( var room in dungeon.Rooms )
		{
			draw.Color = Color.Black.WithAlpha( .5f );
			var rect = WorldRectToPanel( room.WorldRect );
			draw.BoxWithBorder( rect.Contract( 3f ), 1f, Color.White.WithAlpha( .25f ) );

			draw.Color = Color.White;
			draw.FontSize = 10;
			draw.DrawText( rect, room.Cell?.Node?.Name ?? "_", TextFlag.Center );
		}

		foreach( var r in dungeon.Routes )
		{
			foreach( var door in r.Doors )
			{
				draw.Color = Color.White.WithAlpha( .25f );
				var rect = WorldRectToPanel( door.WorldRect );
				draw.Box( rect.Contract( 2f ) );
			}
		}

		DrawEntity( Local.Pawn, Color.Yellow );

		foreach( var ent in Entity.All )
		{
			if ( ent is not Monster ) continue;
			DrawEntity( ent, Color.Red );
		}
	}

	private Rect WorldRectToPanel( Rect rect )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		// can this be better??

		var sizeFraction = rect.Size / worldrect.Size;
		var posFraction = rect.Position / worldrect.Size;

		var pos = Box.Rect.Size * posFraction;
		var size = Box.Rect.Size * sizeFraction;
		pos = new Vector2( Box.Rect.width - pos.y - size.y, Box.Rect.height - pos.x - size.x );
		size = new Vector2( size.y, size.x );

		pos += Box.Rect.Position;

		return new Rect( pos, size );
	}

	private void DrawEntity( Entity entity, Color color )
	{
		if ( !entity.IsValid() ) 
			return;

		Render.Draw2D.Color = color;

		var myrect = new Rect( entity.Position - 16f, 32 );
		myrect = WorldRectToPanel( myrect );

		Render.Draw2D.Color = color;
		Render.Draw2D.Box( myrect );
	}

}
