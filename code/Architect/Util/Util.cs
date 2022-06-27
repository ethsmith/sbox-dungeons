using Sandbox;

namespace Architect
{
	public static class Util
	{
		public static Vector2 WallPlanar( Vector3 pos, Vector3 right, Vector3 up )
		{
			return new Vector2( Vector3.Dot( right, pos ) * (1.0f / 32.0f), Vector3.Dot( up, pos ) * (1.0f / 128.0f) );
		}

		public static Vector2 Planar( Vector3 pos, Vector3 right, Vector3 up, float scale = 32.0f )
		{
			return new Vector2( Vector3.Dot( right, pos ), Vector3.Dot( up, pos ) ) * (1.0f / scale);
		}

		public static bool LineIntersect( Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, out Vector2 intersectPoint )
		{
			intersectPoint = Vector2.Zero;

			var a = p1 - p0;
			var b = p2 - p3;
			var c = p0 - p2;

			float f = a.y * b.x - a.x * b.y;

			if ( f == 0 )
			{
				return false;
			}

			float d = b.y * c.x - b.x * c.y;

			intersectPoint = p0 + (d * a) / f;

			return true;
		}

		public static Vector2 GetInnerPoint( Vector2 p0, Vector2 p1, Vector2 p2, float widthA, float widthB )
		{
			var offsetA = new Vector2( p0.y - p1.y, p1.x - p0.x );
			var offsetB = new Vector2( p1.y - p2.y, p2.x - p1.x );

			offsetA = offsetA.Normal;
			offsetB = offsetB.Normal;

			offsetA *= widthA;
			offsetB *= widthB;

			if ( !LineIntersect( p0 + offsetA, p1 + offsetA, p2 + offsetB, p1 + offsetB, out var innerPoint ) )
			{
				return p1 + offsetA;
			}

			return innerPoint;
		}

		public static Vector2 GetDirectionSnapEnd( Vector2 start, Vector2 end, float gridSnap )
		{
			var directions = new Vector2[]
			{
				new Vector2(1, 0),
				new Vector2(-1, 0),
				new Vector2(0, 1),
				new Vector2(0, -1),
				new Vector2(1, 0) + new Vector2(0, 1),
				new Vector2(-1, 0) + new Vector2(0, -1),
				new Vector2(-1, 0) + new Vector2(0, 1),
				new Vector2(1, 0) + new Vector2(0, -1),
			};

			var closestPoint = Vector2.Zero;
			var closestDistance = 0.0f;

			for ( int i = 0; i < 8; ++i )
			{
				var direction = directions[i].Normal;
				var snapValue = directions[i].Length * gridSnap;

				var rayDistance = (float)Vector2.GetDot( end - start, direction );
				var rayPoint = start + direction * rayDistance.SnapToGrid( snapValue );

				var distance = (float)Vector2.GetDistance( end, start + direction * rayDistance );

				if ( i == 0 || distance < closestDistance )
				{
					closestDistance = distance;
					closestPoint = rayPoint;
				}
			}

			return closestPoint;
		}
	}
}
