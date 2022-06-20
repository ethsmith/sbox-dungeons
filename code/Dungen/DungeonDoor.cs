
using System;

namespace Dungeons;

internal class DungeonDoor
{

	public readonly DungeonRoute Route;
	public readonly DungeonCell A;
	public readonly DungeonCell B;

	public DungeonDoor( DungeonRoute route, DungeonCell a, DungeonCell b )
	{
		Route = route;
		A = a;
		B = b;
	}

	public Rect CalculateRect()
	{
		var r1 = A.Rect;
		var r2 = B.Rect;

		float x1 = Math.Min( r1.right, r2.right );
		float x2 = Math.Max( r1.left, r2.left );
		float y1 = Math.Min( r1.bottom, r2.bottom );
		float y2 = Math.Max( r1.top, r2.top );

		var intersection = new Rect()
		{
			left = Math.Min( x1, x2 ),
			top = Math.Min( y1, y2 ),
			width = Math.Max( 0.0f, x1 - x2 ),
			height = Math.Max( 0.0f, y1 - y2 )
		};

		var sz = .15f;
		var mins = intersection.Center - sz;
		var size = Vector2.One * sz * 2;
		var result = new Rect( mins, size );

		return result;
	}

}
