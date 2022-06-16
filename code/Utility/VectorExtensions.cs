
using System;

namespace Dungeons.Utility;

internal static class VectorExtensions
{

	public static int DistanceOrtho( this Vector2 a, Vector2 b )
	{
		return (int)(Math.Abs( a.x - b.x ) + Math.Abs( a.y - b.y ));
	}

}
