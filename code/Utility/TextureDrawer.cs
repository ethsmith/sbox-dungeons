
using Sandbox;
using System;

namespace Dungeons;

// ported from https://github.com/ProtoTurtle/UnityBitmapDrawing/blob/d73a977ad918ef1c6b3fa820432291dbed388e54/src/BitmapDrawing.cs

public class TextureDrawer
{

	private readonly Texture Texture;
	private readonly byte[] Data;
	private readonly int Width;
	private readonly int Height;

	public Vector2 Size => new( Width, Height );

	public TextureDrawer( Texture texture )
	{
		Texture = texture;
		Width = Texture.Width;
		Height = Texture.Height;

		var colors = texture.GetPixels();
		var pos = 0;

		Data = new byte[Width * Height * 4];

		foreach ( var color in colors )
		{
			Data[pos++] = color.r;
			Data[pos++] = color.g;
			Data[pos++] = color.b;
			Data[pos++] = color.a;
		}
	}

	public void Apply()
	{
		Texture.Update( Data );
	}

	public void DrawCircle( int x, int y, int radius, Color color, bool filled = false )
	{
		int cx = radius;
		int cy = 0;
		int radiusError = 1 - cx;

		while ( cx >= cy )
		{
			if ( !filled )
			{
				PlotCircle( cx, x, cy, y, color );
			}
			else
			{
				ScanLineCircle( cx, x, cy, y, color );
			}

			cy++;

			if ( radiusError < 0 )
			{
				radiusError += 2 * cy + 1;
			}
			else
			{
				cx--;
				radiusError += 2 * (cy - cx + 1);
			}
		}
	}

	public void DrawPixel( int x, int y, Color32 color )
	{
		if ( x < 0 || x > Texture.Width || y < 0 || y > Texture.Height )
		{
			return;
		}

		var idx = ToIndex( x, y );
		Data[idx] = color.r;
		Data[idx + 1] = color.g;
		Data[idx + 2] = color.b;
		Data[idx + 3] = color.a;
	}

	public void DrawPixels( int x, int y, int width, int height, Color32 color )
	{
		for ( int px = x; px < x + width; px++ )
		{
			for ( int py = y; py < y + height; py++ )
			{
				var idx = ToIndex( px, py );
				Data[idx++] = color.r;
				Data[idx++] = color.g;
				Data[idx++] = color.b;
				Data[idx++] = color.a;
			}
		}
	}

	public void DrawLine( int x0, int y0, int x1, int y1, Color color )
	{
		int width = Texture.Width;
		int height = Texture.Height;

		bool isSteep = Math.Abs( y1 - y0 ) > Math.Abs( x1 - x0 );
		if ( isSteep )
		{
			Swap( ref x0, ref y0 );
			Swap( ref x1, ref y1 );
		}
		if ( x0 > x1 )
		{
			Swap( ref x0, ref x1 );
			Swap( ref y0, ref y1 );
		}

		int deltax = x1 - x0;
		int deltay = Math.Abs( y1 - y0 );

		int error = deltax / 2;
		int ystep;
		int y = y0;

		if ( y0 < y1 )
			ystep = 1;
		else
			ystep = -1;

		for ( int x = x0; x < x1; x++ )
		{
			if ( isSteep )
				DrawPixel( y, x, width, height, color );
			else
				DrawPixel( x, y, width, height, color );

			error = error - deltay;
			if ( error < 0 )
			{
				y = y + ystep;
				error = error + deltax;
			}
		}
	}

	public void DrawRectangle( Rect rectangle, Color color )
	{
		int x = (int)rectangle.Position.x;
		int y = (int)rectangle.Position.y;
		int height = (int)rectangle.Height;
		int width = (int)rectangle.Width;

		DrawLine( x, y, x, y + height, color );
		DrawLine( x, y + height, x + width, y + height, color );
		DrawLine( x + width, y + height, x + width, y, color );
		DrawLine( x + width, y, x, y, color );
	}

	public void DrawFilledRectangle( Rect rectangle, Color color )
	{
		DrawPixels( (int)rectangle.Position.x, (int)rectangle.Position.y,
			(int)rectangle.Width, (int)rectangle.Height, color );
	}

	private void DrawPixel( int x, int y, int width, int height, Color32 color )
	{
		if ( x < 0 || x > width || y < 0 || y > height )
		{
			return;
		}

		DrawPixel( x, y, color );
	}

	private void PlotCircle( int cx, int x, int cy, int y, Color color )
	{
		DrawPixel( cx + x, cy + y, color ); // Point in octant 1...
		DrawPixel( cy + x, cx + y, color );
		DrawPixel( -cx + x, cy + y, color );
		DrawPixel( -cy + x, cx + y, color );
		DrawPixel( -cx + x, -cy + y, color );
		DrawPixel( -cy + x, -cx + y, color );
		DrawPixel( cx + x, -cy + y, color );
		DrawPixel( cy + x, -cx + y, color ); // ... point in octant 8
	}

	private void ScanLineCircle( int cx, int x, int cy, int y, Color color )
	{
		DrawLine( cx + x, cy + y, -cx + x, cy + y, color );
		DrawLine( cy + x, cx + y, -cy + x, cx + y, color );
		DrawLine( -cx + x, -cy + y, cx + x, -cy + y, color );
		DrawLine( -cy + x, -cx + y, cy + x, -cx + y, color );
	}

	private int ToIndex( int x, int y )
	{
		x = Math.Clamp( x, 0, Texture.Width - 1 );
		y = Math.Clamp( y, 0, Texture.Height - 1 );

		return ((y * Texture.Width) + x) * 4;
	}

	private void Swap( ref int x, ref int y )
	{
		int temp = x;
		x = y;
		y = temp;
	}

}
