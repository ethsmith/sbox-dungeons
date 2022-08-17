
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Dungeons.UI;

internal class Minimap : Panel
{

	public float DrawOpacity = 1f;

	public Color WallColor => Color.Gray.WithAlpha( DrawOpacity );
	public Color DoorColor => Color.Gray.WithAlpha( DrawOpacity );
	public Color MonsterColor => Color.Red.WithAlpha( DrawOpacity );
	public Color PlayerColor => Color.Yellow.WithAlpha( DrawOpacity );
	public Color RoomColor => Color.Gray.WithAlpha( .08f * DrawOpacity );

	public override void SetProperty( string name, string value )
	{
		if( name == "opacity" )
		{
			float.TryParse( value, out DrawOpacity );
			return;
		}

		base.SetProperty( name, value );
	}

	public override void Tick()
	{
		base.Tick();

		var posfraction = Local.Pawn.Position / DungeonEntity.Current.WorldRect.Size;
		var mod = Box.RectInner.Size / Parent.Box.RectInner.Size;

		posfraction = 1f - posfraction;
		posfraction.x = (float)Math.Round( -posfraction.x * mod.x + .5f, 3 );
		posfraction.y = (float)Math.Round( -posfraction.y * mod.y + .5f, 3 );

		Style.Left = Length.Fraction( posfraction.y );
		Style.Top = Length.Fraction( posfraction.x );
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		var draw = Render.Draw2D;
		var dungeon = DungeonEntity.Current;

		foreach ( var room in dungeon.Rooms )
		{
			draw.Color = RoomColor;
			var rect = WorldRectToPanel( this, room.WorldRect );
			draw.BoxWithBorder( rect.Contract( 3f ), 1f, WallColor );
		}

		foreach ( var r in dungeon.Routes )
		{
			foreach ( var door in r.Doors )
			{
				draw.Color = DoorColor;
				var rect = WorldRectToPanel( this, door.WorldRect );
				draw.Box( rect.Contract( 3f ) );
			}
		}

		DrawEntity( Local.Pawn, PlayerColor );

		foreach ( var ent in Entity.All )
		{
			if ( ent is not Monster ) 
				continue;

			DrawEntity( ent, MonsterColor );
		}

		const int chunksize = 80;
		var fullrect = dungeon.WorldRect;
		var splits = fullrect.Size / chunksize;

		draw.Color = Color.Black;

		for ( int x = 0; x < splits.x; x++ )
		{
			for( int y = 0; y < splits.y; y++ )
			{
				var idx = x * 10000 + y;
				var pos = new Vector3( x * chunksize, y * chunksize );

				if ( !Visited.Contains( idx ) )
				{
					var dist = pos.Distance( Local.Pawn.Position );
					if ( dist < 300 )
					{
						Visited.Add( idx );
						continue;
					}
				}
				else
				{
					continue;
				}

				var rect = WorldRectToPanel( this, new Rect( pos.x, pos.y, chunksize, chunksize ) );
				draw.Box( rect.Expand( 1 ) );
			}
		}
	}

	private static HashSet<int> Visited = new();

	private Rect WorldRectToPanel( Panel panel, Rect rect )
	{
		var dungeon = DungeonEntity.Current;
		var worldrect = dungeon.WorldRect;

		// can this be better??

		var sizeFraction = rect.Size / worldrect.Size;
		var posFraction = rect.Position / worldrect.Size;
		var pos = panel.Box.Rect.Size * posFraction;
		var size = panel.Box.Rect.Size * sizeFraction;
		pos = new Vector2( panel.Box.Rect.width - pos.y - size.y, panel.Box.Rect.height - pos.x - size.x );
		size = new Vector2( size.y, size.x );
		pos += panel.Box.Rect.Position;

		return new Rect( pos, size );
	}

	private void DrawEntity( Entity entity, Color color )
	{
		if ( !entity.IsValid() )
			return;

		var myrect = new Rect( entity.Position - 16f, 32 );
		myrect = WorldRectToPanel( this, myrect );

		Render.Draw2D.Color = color;
		Render.Draw2D.Box( myrect );
	}

}
