using ClipperLib;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Architect
{
	using Path = List<IntPoint>;
	using Paths = List<List<IntPoint>>;

	public class WallGeometry
	{
		public HalfEdgeMesh.Mesh hemesh = new();
		Model model;
		Mesh mesh;
		Mesh mesh2;
		Mesh floorMesh;
		SceneObject so;
		PhysicsBody body;
		PhysicsShape shape1;
		PhysicsShape shape2;

		public readonly Vector3 GridSize;

		static Vector2 WallPlanar( Vector3 pos, Vector3 right, Vector3 up, float scale = 32.0f )
		{
			return new Vector2( Vector3.Dot( right, pos ) * (1.0f / 32.0f), Vector3.Dot( up, pos ) * (1.0f / 128.0f) );
		}

		static Vector2 Planar( Vector3 pos, Vector3 right, Vector3 up, float scale = 32.0f )
		{
			return new Vector2( Vector3.Dot( right, pos ), Vector3.Dot( up, pos ) ) * (1.0f / scale);
		}

		public WallGeometry( Vector2 gridSize, Vector3 mapBounds )
		{
			//hemesh.CreateGrid( 32, 32 );

			GridSize = gridSize;

			mesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01b.vmat" ) );
			mesh.SetBounds( Vector3.Zero, mapBounds );
			mesh.CreateVertexBuffer<SimpleVertex>( 200000, SimpleVertex.Layout );

			mesh2 = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01b.vmat" ) );
			mesh2.SetBounds( Vector3.Zero, mapBounds );
			mesh2.CreateVertexBuffer<SimpleVertex>( 200000, SimpleVertex.Layout );

			floorMesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01.vmat" ) );
			floorMesh.SetBounds( Vector3.Zero, mapBounds );
			floorMesh.CreateVertexBuffer<SimpleVertex>( 200000, SimpleVertex.Layout );

			body = new PhysicsBody( Map.Physics );

			//RebuildMesh();

			model = Model.Builder
				.AddMesh( mesh )
				.AddMesh( mesh2 )
				.AddMesh( floorMesh )
				.Create();

			so = new SceneObject( Map.Scene, model, new Transform( Vector3.Zero ) );
			so.Flags.CastShadows = true;
		}

		public void Destroy()
		{
			body?.Remove();
			body = null;

			so?.Delete();
			so = null;
		}

		public void RebuildMesh()
		{
			var vertices = new List<SimpleVertex>();
			var vertices2 = new List<SimpleVertex>();

			for ( int i = 0; i < hemesh.HalfEdges.Count; ++i )
			{
				var h = hemesh.HalfEdges[i];
				h.Thickness = 2;
			}

			if ( shape1.IsValid() )
			{
				shape1.Remove();
			}

			if ( shape2.IsValid() )
			{
				shape2.Remove();
			}

			const float height = 128;

			for ( int i = 0; i < hemesh.HalfEdges.Count; ++i )
			{
				var h = hemesh.HalfEdges[i];

				if ( h.Vertex1 == null )
					continue;

				if ( h.Next == null )
					continue;

				var h_prev = h.Prev;
				var h_next = h.Next;
				var h_next2 = h.Next.Next;

				var p1_2 = new Vector3( h_prev.Vertex1.X, h_prev.Vertex1.Y, 0.0f ) * 32;
				var p2_2 = new Vector3( h.Vertex1.X, h.Vertex1.Y, 0.0f ) * 32;
				var p3_2 = new Vector3( h_next.Vertex1.X, h_next.Vertex1.Y, 0.0f ) * 32;
				var p4_2 = new Vector3( h_next2.Vertex1.X, h_next2.Vertex1.Y, 0.0f ) * 32;

				var forward = (p2_2 - p3_2).Normal;
				var right = forward.Cross( Vector3.Up );

				Vector3 leftPoint;
				Vector3 rightPoint;

				var leftParallel = h_prev.IsParallel( h );
				var rightParallel = h.IsParallel( h_next );

				if ( h_prev.Vertex1 == h.Vertex2 || leftParallel )
				{
					leftPoint = p2_2 + right * h.Thickness;

					vertices.Add( new SimpleVertex( leftPoint + Vector3.Up * height, forward, right, WallPlanar( leftPoint + Vector3.Up * height, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( leftPoint, forward, right, WallPlanar( leftPoint, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p2_2, forward, right, WallPlanar( p2_2, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p2_2 + Vector3.Up * height, forward, right, WallPlanar( p2_2 + Vector3.Up * height, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( leftPoint + Vector3.Up * height, forward, right, WallPlanar( leftPoint + Vector3.Up * height, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p2_2, forward, right, WallPlanar( p2_2, -right, Vector3.Down ) ) );
				}
				else
				{
					leftPoint = new Vector3( GetInnerPoint( p1_2, p2_2, p3_2, h_prev.Thickness, h.Thickness ) );
				}

				if ( h.Vertex1 == h_next.Vertex2 || rightParallel )
				{
					rightPoint = p3_2 + right * h.Thickness;

					vertices.Add( new SimpleVertex( p3_2, -forward, right, WallPlanar( p3_2, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( rightPoint, -forward, right, WallPlanar( rightPoint, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, -forward, right, WallPlanar( rightPoint + Vector3.Up * height, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p3_2, -forward, right, WallPlanar( p3_2, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, -forward, right, WallPlanar( rightPoint + Vector3.Up * height, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p3_2 + Vector3.Up * height, -forward, right, WallPlanar( p3_2 + Vector3.Up * height, right, Vector3.Down ) ) );
				}
				else
				{
					rightPoint = new Vector3( GetInnerPoint( p2_2, p3_2, p4_2, h.Thickness, h_next.Thickness ) );
				}

				vertices2.Add( new SimpleVertex( p2_2 + Vector3.Up * height, Vector3.Up, Vector3.Forward, WallPlanar( p2_2, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( rightPoint + Vector3.Up * height, Vector3.Up, Vector3.Forward, WallPlanar( rightPoint, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( leftPoint + Vector3.Up * height, Vector3.Up, Vector3.Forward, WallPlanar( leftPoint, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( rightPoint + Vector3.Up * height, Vector3.Up, Vector3.Forward, WallPlanar( rightPoint, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( p2_2 + Vector3.Up * height, Vector3.Up, Vector3.Forward, WallPlanar( p2_2, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( p3_2 + Vector3.Up * height, Vector3.Up, Vector3.Forward, WallPlanar( p3_2, Vector3.Left, Vector3.Forward ) ) );

				vertices.Add( new SimpleVertex( leftPoint + Vector3.Up * height, right, forward, WallPlanar( leftPoint + Vector3.Up * height, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, right, forward, WallPlanar( rightPoint + Vector3.Up * height, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( leftPoint, right, forward, WallPlanar( leftPoint, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, right, forward, WallPlanar( rightPoint + Vector3.Up * height, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( rightPoint, right, forward, WallPlanar( rightPoint, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( leftPoint, right, forward, WallPlanar( leftPoint, forward, Vector3.Down ) ) );
			}

			if ( vertices.Count > 0 )
			{
				shape1 = body.AddMeshShape( vertices.Select( x => x.position ).ToArray(), Enumerable.Range( 0, vertices.Count ).ToArray() );

				mesh.SetVertexBufferData<SimpleVertex>( vertices.ToArray() );
			}

			if ( vertices2.Count > 0 )
			{
				shape2 = body.AddMeshShape( vertices2.Select( x => x.position ).ToArray(), Enumerable.Range( 0, vertices2.Count ).ToArray() );

				mesh2.SetVertexBufferData<SimpleVertex>( vertices2.ToArray() );
			}

			mesh.SetVertexRange( 0, vertices.Count );
			mesh2.SetVertexRange( 0, vertices2.Count );

			RebuildFloorMesh();
		}

		private void RebuildFloorMesh()
		{
			var vertices = new List<SimpleVertex>();

			for ( int faceIndex = 0; faceIndex < hemesh.Faces.Count; ++faceIndex )
			{
				var face = hemesh.Faces[faceIndex];
				if ( face.IsOuterFace )
					continue;

				var tess = new LibTessDotNet.Tess();
				var contour = face.NonParallelVertices.Select( x => new LibTessDotNet.ContourVertex( new LibTessDotNet.Vec3( x.X, x.Y, 0 ) ) ).ToArray();

				tess.AddContour( contour, LibTessDotNet.ContourOrientation.Clockwise );
				tess.Tessellate( LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3 );

				int numTriangles = tess.ElementCount;
				for ( int i = 0; i < numTriangles; i++ )
				{
					var v0 = tess.Vertices[tess.Elements[i * 3]].Position;
					var v1 = tess.Vertices[tess.Elements[i * 3 + 1]].Position;
					var v2 = tess.Vertices[tess.Elements[i * 3 + 2]].Position;

					var p = new Vector3( v0.X, v0.Y, 0.01f ) * 32;
					var p1 = new Vector3( v1.X, v1.Y, 0.01f ) * 32;
					var p2 = new Vector3( v2.X, v2.Y, 0.01f ) * 32;
					vertices.Add( new SimpleVertex( p, Vector3.Up, Vector3.Forward, Planar( p, Vector3.Forward, Vector3.Right ) ) );
					vertices.Add( new SimpleVertex( p1, Vector3.Up, Vector3.Forward, Planar( p1, Vector3.Forward, Vector3.Right ) ) );
					vertices.Add( new SimpleVertex( p2, Vector3.Up, Vector3.Forward, Planar( p2, Vector3.Forward, Vector3.Right ) ) );
				}
			}

			if ( vertices.Count == 0 ) return;

			floorMesh.SetVertexBufferData( vertices );
			floorMesh.SetVertexRange( 0, vertices.Count );
		}

		public int GetVertexId( int x, int y )
		{
			if ( x < 0 || x >= GridSize.x || y < 0 || y >= GridSize.y ) return -1;

			return (int)(x * GridSize.x) + y;
		}

		static bool LineIntersect( Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, out Vector2 intersectPoint )
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

		static Vector2 GetInnerPoint( Vector2 p0, Vector2 p1, Vector2 p2, float widthA, float widthB )
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

		public void AddEdge( int x, int y, int x1, int y1, int type )
		{
			var xcur = x;
			var ycur = y;
			var xdelta = x1 - x;
			var ydelta = y1 - y;
			var xdir = Math.Sign( xdelta );
			var ydir = Math.Sign( ydelta );
			var steps = Math.Max( Math.Abs( xdelta ), Math.Abs( ydelta ) );

			if ( steps == 0 )
				return;

			var isStraight = xdir == 0 || ydir == 0;

			for ( int i = 0; i < steps; ++i )
			{
				if ( isStraight || !HasEdge( GetVertexId( xcur + xdir, ycur ), GetVertexId( xcur, ycur + ydir ) ) )
				{
					hemesh.AddEdge( GetVertexId( xcur, ycur ), GetVertexId( xcur + xdir, ycur + ydir ), (byte)type );
				}

				xcur += xdir;
				ycur += ydir;
			}
		}

		public bool HasEdge( int x, int y )
		{
			return hemesh.HasEdge( x, y );
		}

		public void DebugDraw()
		{
			DebugOverlay.ScreenText( $"{hemesh.HalfEdges.Count} half edges" );
			DebugOverlay.ScreenText( $"{hemesh.Faces.Count} faces", 1 );

			for ( int i = 0; i < hemesh.Faces.Count; ++i )
			{
				var face = hemesh.Faces[i];
				if ( face.IsOuterFace )
					continue;

				DebugOverlay.Text( $"room #{i}", new Vector3( face.Centroid, 1 ) * 32 );

				foreach ( var halfEdge in face.HalfEdge.HalfEdges )
				{
					var p1 = new Vector3( halfEdge.Vertex1.X, halfEdge.Vertex1.Y, 1.0f ) * 32;
					var p2 = new Vector3( halfEdge.Next.Vertex1.X, halfEdge.Next.Vertex1.Y, 1.0f ) * 32;

					DebugOverlay.Line( p1, p2, Color.Green, 0, false );
				}
			}

			for ( int i = 0; i < hemesh.HalfEdges.Count; ++i )
			{
				var h = hemesh.HalfEdges[i];

				if ( h.Vertex1 == null )
					continue;

				if ( h.Next == null )
					continue;

				var h_prev = h.Prev;
				var h_next = h.Next;
				var h_next2 = h.Next.Next;

				var p1_2 = new Vector3( h_prev.Vertex1.X, h_prev.Vertex1.Y, 0.0f ) * 32;
				var p2_2 = new Vector3( h.Vertex1.X, h.Vertex1.Y, 0.0f ) * 32;
				var p3_2 = new Vector3( h_next.Vertex1.X, h_next.Vertex1.Y, 0.0f ) * 32;
				var p4_2 = new Vector3( h_next2.Vertex1.X, h_next2.Vertex1.Y, 0.0f ) * 32;

				var leftPoint = new Vector3( GetInnerPoint( p1_2, p2_2, p3_2, 2.0f, 2.0f ) );
				var rightPoint = new Vector3( GetInnerPoint( p2_2, p3_2, p4_2, 2.0f, 2.0f ) );

				if ( h_prev.Vertex1 == h.Vertex2 )
				{
					var forward = (p2_2 - p3_2).Normal;
					var right = forward.Cross( Vector3.Up );
					leftPoint = p2_2 + right * 2.0f;
				}

				if ( h.Vertex1 == h_next.Vertex2 )
				{
					var forward = (p2_2 - p3_2).Normal;
					var right = forward.Cross( Vector3.Up );
					rightPoint = p3_2 + right * 2.0f;
				}

				DebugOverlay.Line( leftPoint, rightPoint, (i % 2 == 0) ? Color.Cyan : Color.Orange, 0, false );
			}

			foreach ( var v in hemesh.Vertices )
			{
				//DebugOverlay.Text( $"({v.X}, {v.Y})", new Vector3( v.X, v.Y, 0 ) * 32 );
				DebugOverlay.Sphere( new Vector3( v.X, v.Y, 0 ) * 32, 5, v.Connections.Count == 0 ? Color.White.WithAlpha( 0.02f ) : Color.White.WithAlpha( 0.2f ), 0, false );
			}
		}
	}
}
