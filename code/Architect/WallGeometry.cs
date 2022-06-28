using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Architect
{
	public class WallObject
	{
		public HalfEdgeMesh.Mesh HEMesh = new();

		Model model;
		Mesh mesh;
		Mesh mesh2;
		Mesh floorMesh;
		Mesh floorGridMesh;
		SceneObject so;

		Dictionary<PhysicsBody, int> wallBodies = new();
		Dictionary<PhysicsBody, int> floorBodies = new();

		PhysicsWorld physicsWorld;

		public readonly Vector2 GridSize;

		public WallObject( SceneWorld sceneWorld, PhysicsWorld physicsWorld, Vector2 gridSize, Vector3 mapBounds )
		{
			GridSize = gridSize;

			this.physicsWorld = physicsWorld;

			mesh = new Mesh( Material.Load( "materials/walls/plaster/basic_01a/plaster_wall_01a.vmat" ) );
			mesh.SetBounds( Vector3.Zero, mapBounds );
			mesh.CreateVertexBuffer<SimpleVertex>( SimpleVertex.Layout );

			mesh2 = new Mesh( Material.Load( "materials/dev/reflectivity_50b.vmat" ) );
			mesh2.SetBounds( Vector3.Zero, mapBounds );
			mesh2.CreateVertexBuffer<SimpleVertex>( SimpleVertex.Layout );

			floorMesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01b.vmat" ) );
			floorMesh.SetBounds( Vector3.Zero, mapBounds );
			floorMesh.CreateVertexBuffer<SimpleVertex>( SimpleVertex.Layout );

			floorGridMesh = new Mesh( Material.Load( "materials/dev/grid/archi_grid.vmat" ) );
			floorGridMesh.SetBounds( Vector3.Zero, mapBounds );
			floorGridMesh.CreateVertexBuffer<SimpleVertex>( SimpleVertex.Layout );

			model = Model.Builder
				.AddMesh( mesh )
				.AddMesh( mesh2 )
				.AddMesh( floorMesh )
				.AddMesh( floorGridMesh )
				.Create();

			so = new SceneObject( sceneWorld, model, new Transform( Vector3.Zero ) );
			so.Flags.CastShadows = true;
		}

		public void Destroy()
		{
			foreach ( var wallBody in wallBodies.Keys )
			{
				wallBody?.Remove();
			}

			foreach ( var floorBody in floorBodies.Keys )
			{
				floorBody?.Remove();
			}

			wallBodies.Clear();
			floorBodies.Clear();

			so?.Delete();
			so = null;
		}

		public void Select( PhysicsBody body )
		{
			if ( wallBodies.TryGetValue( body, out var halfEdgeIndex ) )
			{
				var halfEdge = HEMesh.HalfEdges[halfEdgeIndex];
				var face = halfEdge.Face;

				if ( face.IsOuterFace )
				{
					face = halfEdge.Opposite.Face;
				}

				if ( face.IsFloor )
				{
					RemoveRoom( face );
				}
				else
				{
					HEMesh.RemoveEdge( halfEdge.ParentEdge );
				}

				RebuildMesh();
			}
			else if ( floorBodies.TryGetValue( body, out var faceIndex ) )
			{
				RemoveRoom( HEMesh.Faces[faceIndex] );
				RebuildMesh();
			}
		}

		public void Remove( PhysicsBody body )
		{
			if ( wallBodies.TryGetValue( body, out var halfEdgeIndex ) )
			{
				var halfEdge = HEMesh.HalfEdges[halfEdgeIndex];
				var edge = halfEdge.ParentEdge;

				if ( edge.HalfEdge1.Face.IsFloor || edge.HalfEdge2.Face.IsFloor )
				{
					HEMesh.AddEdge( edge, 0 );
				}
				else
				{
					HEMesh.RemoveEdge( edge );
				}

				RebuildMesh();
			}
			else if ( floorBodies.TryGetValue( body, out var faceIndex ) )
			{
				var face = HEMesh.Faces[faceIndex];
				var edges = new HashSet<HalfEdgeMesh.Edge>();

				face.FloorType = 0;

				foreach ( var halfEdge in face.HalfEdges )
				{
					var edge = halfEdge.ParentEdge;
					if ( edges.Contains( edge ) )
						continue;

					if ( edge.WallType != 0 )
						continue;

					if ( !face.IsFloor && !halfEdge.Opposite.Face.IsFloor )
					{
						edges.Add( edge );
					}
					else if ( face.IsOuterFace )
					{
						edges.Add( edge );
					}
				}

				foreach ( var edge in edges )
				{
					HEMesh.RemoveEdge( edge );
				}

				RebuildMesh();
			}
		}

		public void RemoveRoom( HalfEdgeMesh.Face face )
		{
			if ( face.IsOuterFace )
				return;

			var edges = new HashSet<HalfEdgeMesh.Edge>();

			foreach ( var halfEdge in face.HalfEdges )
			{
				var edge = halfEdge.ParentEdge;
				if ( edges.Contains( edge ) )
					continue;

				if ( face.IsFloor && !halfEdge.Opposite.Face.IsFloor )
				{
					edges.Add( edge );
				}
				else if ( face == halfEdge.Opposite.Face )
				{
					edges.Add( edge );
				}
			}

			face.FloorType = 0;

			foreach ( var edge in edges )
			{
				HEMesh.RemoveEdge( edge );
			}

			RebuildMesh();
		}

		private static HalfEdgeMesh.HalfEdge GetWallPrevHalfEdge( HalfEdgeMesh.HalfEdge halfEdge )
		{
			var prevHalfEdge = halfEdge.Prev;

			// Check at most 7 times, there's only 7 possible edges around us
			for ( int i = 0; i < 7; ++i )
			{
				if ( prevHalfEdge.ParentEdge.WallType != halfEdge.ParentEdge.WallType )
				{
					prevHalfEdge = prevHalfEdge.Opposite.Prev;
				}
				else
				{
					break;
				}
			}

			return prevHalfEdge;
		}

		private static HalfEdgeMesh.HalfEdge GetWallNextHalfEdge( HalfEdgeMesh.HalfEdge halfEdge )
		{
			var nextHalfEdge = halfEdge.Next;

			// Check at most 7 times, there's only 7 possible edges around us
			for ( int i = 0; i < 7; ++i )
			{
				if ( nextHalfEdge.ParentEdge.WallType != halfEdge.ParentEdge.WallType )
				{
					nextHalfEdge = nextHalfEdge.Opposite.Next;
				}
				else
				{
					break;
				}
			}

			return nextHalfEdge;
		}

		public void RebuildMesh()
		{
			var vertices = new List<SimpleVertex>();
			var vertices2 = new List<SimpleVertex>();
			var collisionVertices = new List<Vector3>();

			foreach ( var wallBody in wallBodies.Keys )
			{
				wallBody?.Remove();
			}

			wallBodies.Clear();

			for ( int i = 0; i < HEMesh.HalfEdgeCount; ++i )
			{
				var h = HEMesh.HalfEdges[i];

				if ( h.Vertex1 == null )
					continue;

				if ( h.Next == null )
					continue;

				if ( h.ParentEdge.WallType == 0 )
					continue;

				float height = 128;

				var h_prev = GetWallPrevHalfEdge( h );
				var h_next = GetWallNextHalfEdge( h );
				var h_next2 = h_next.Next;

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

				int vertexOffset0 = vertices.Count;
				int vertexOffset1 = vertices2.Count;

				if ( h_prev.Vertex1 == h.Vertex2 || leftParallel )
				{
					leftPoint = p2_2 + right * h.WallThickness;

					vertices.Add( new SimpleVertex( leftPoint + Vector3.Up * height, forward, right, Util.WallPlanar( leftPoint + Vector3.Up * height, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( leftPoint, forward, right, Util.WallPlanar( leftPoint, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p2_2, forward, right, Util.WallPlanar( p2_2, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p2_2 + Vector3.Up * height, forward, right, Util.WallPlanar( p2_2 + Vector3.Up * height, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( leftPoint + Vector3.Up * height, forward, right, Util.WallPlanar( leftPoint + Vector3.Up * height, -right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p2_2, forward, right, Util.WallPlanar( p2_2, -right, Vector3.Down ) ) );
				}
				else
				{
					leftPoint = new Vector3( Util.GetInnerPoint( p1_2, p2_2, p3_2, h_prev.WallThickness, h.WallThickness ) );
				}

				if ( h.Vertex1 == h_next.Vertex2 || rightParallel )
				{
					rightPoint = p3_2 + right * h.WallThickness;

					vertices.Add( new SimpleVertex( p3_2, -forward, right, Util.WallPlanar( p3_2, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( rightPoint, -forward, right, Util.WallPlanar( rightPoint, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, -forward, right, Util.WallPlanar( rightPoint + Vector3.Up * height, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p3_2, -forward, right, Util.WallPlanar( p3_2, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, -forward, right, Util.WallPlanar( rightPoint + Vector3.Up * height, right, Vector3.Down ) ) );
					vertices.Add( new SimpleVertex( p3_2 + Vector3.Up * height, -forward, right, Util.WallPlanar( p3_2 + Vector3.Up * height, right, Vector3.Down ) ) );
				}
				else
				{
					rightPoint = new Vector3( Util.GetInnerPoint( p2_2, p3_2, p4_2, h.WallThickness, h_next.WallThickness ) );
				}

				vertices2.Add( new SimpleVertex( p2_2 + Vector3.Up * height, Vector3.Up, Vector3.Forward, Util.WallPlanar( p2_2, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( rightPoint + Vector3.Up * height, Vector3.Up, Vector3.Forward, Util.WallPlanar( rightPoint, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( leftPoint + Vector3.Up * height, Vector3.Up, Vector3.Forward, Util.WallPlanar( leftPoint, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( rightPoint + Vector3.Up * height, Vector3.Up, Vector3.Forward, Util.WallPlanar( rightPoint, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( p2_2 + Vector3.Up * height, Vector3.Up, Vector3.Forward, Util.WallPlanar( p2_2, Vector3.Left, Vector3.Forward ) ) );
				vertices2.Add( new SimpleVertex( p3_2 + Vector3.Up * height, Vector3.Up, Vector3.Forward, Util.WallPlanar( p3_2, Vector3.Left, Vector3.Forward ) ) );

				vertices.Add( new SimpleVertex( leftPoint + Vector3.Up * height, right, forward, Util.WallPlanar( leftPoint + Vector3.Up * height, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, right, forward, Util.WallPlanar( rightPoint + Vector3.Up * height, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( leftPoint, right, forward, Util.WallPlanar( leftPoint, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( rightPoint + Vector3.Up * height, right, forward, Util.WallPlanar( rightPoint + Vector3.Up * height, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( rightPoint, right, forward, Util.WallPlanar( rightPoint, forward, Vector3.Down ) ) );
				vertices.Add( new SimpleVertex( leftPoint, right, forward, Util.WallPlanar( leftPoint, forward, Vector3.Down ) ) );

				var collisionOffset = collisionVertices.Count;

				collisionVertices.AddRange( vertices.GetRange( vertexOffset0, vertices.Count - vertexOffset0 ).Select( x => x.position ) );
				collisionVertices.AddRange( vertices2.GetRange( vertexOffset1, vertices2.Count - vertexOffset1 ).Select( x => x.position ) );

				var body = new PhysicsBody( physicsWorld );
				body.AddMeshShape( collisionVertices.GetRange( collisionOffset, collisionVertices.Count - collisionOffset ).ToArray(),
					Enumerable.Range( 0, collisionVertices.Count - collisionOffset ).ToArray() );
				wallBodies.Add( body, i );
			}

			if ( vertices.Count > 0 )
			{
				mesh.SetVertexBufferSize( vertices.Count );
				mesh.SetVertexBufferData( vertices );
			}

			if ( vertices2.Count > 0 )
			{
				mesh2.SetVertexBufferSize( vertices2.Count );
				mesh2.SetVertexBufferData( vertices2 );
			}

			mesh.SetVertexRange( 0, vertices.Count );
			mesh2.SetVertexRange( 0, vertices2.Count );

			RebuildFloorMesh();
		}

		private void RebuildFloorMesh()
		{
			var vertices = new List<SimpleVertex>();
			var gridVertices = new List<SimpleVertex>();

			foreach ( var floorBody in floorBodies.Keys )
			{
				floorBody?.Remove();
			}

			floorBodies.Clear();

			for ( int faceIndex = 0; faceIndex < HEMesh.Faces.Count; ++faceIndex )
			{
				var face = HEMesh.Faces[faceIndex];
				if ( !face.IsFloor )
				{
					continue;
				}

				var collisionVertices = new List<Vector3>();

				var tess = new LibTessDotNet.Tess();
				var contour = face.NonParallelVertices.Select( x => new LibTessDotNet.ContourVertex( new LibTessDotNet.Vec3( x.X, x.Y, 0 ) ) ).ToArray();

				tess.AddContour( contour, LibTessDotNet.ContourOrientation.Clockwise );
				tess.Tessellate( LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3 );

				float texScale = 32;

				int numTriangles = tess.ElementCount;
				for ( int i = 0; i < numTriangles; i++ )
				{
					var v0 = tess.Vertices[tess.Elements[i * 3]].Position;
					var v1 = tess.Vertices[tess.Elements[i * 3 + 1]].Position;
					var v2 = tess.Vertices[tess.Elements[i * 3 + 2]].Position;

					var p = new Vector3( v0.X, v0.Y, 0.1f * face.FloorHeight ) * 32;
					var p1 = new Vector3( v1.X, v1.Y, 0.1f * face.FloorHeight ) * 32;
					var p2 = new Vector3( v2.X, v2.Y, 0.1f * face.FloorHeight ) * 32;

					vertices.Add( new SimpleVertex( p, Vector3.Up, Vector3.Forward, Util.Planar( p, Vector3.Forward, Vector3.Right, texScale ) ) );
					vertices.Add( new SimpleVertex( p1, Vector3.Up, Vector3.Forward, Util.Planar( p1, Vector3.Forward, Vector3.Right, texScale ) ) );
					vertices.Add( new SimpleVertex( p2, Vector3.Up, Vector3.Forward, Util.Planar( p2, Vector3.Forward, Vector3.Right, texScale ) ) );

					gridVertices.Add( new SimpleVertex( p.WithZ( p.z + 0.1f ), Vector3.Up, Vector3.Forward, Util.Planar( p, Vector3.Forward, Vector3.Right, 32 ) ) );
					gridVertices.Add( new SimpleVertex( p1.WithZ( p1.z + 0.1f ), Vector3.Up, Vector3.Forward, Util.Planar( p1, Vector3.Forward, Vector3.Right, 32 ) ) );
					gridVertices.Add( new SimpleVertex( p2.WithZ( p2.z + 0.1f ), Vector3.Up, Vector3.Forward, Util.Planar( p2, Vector3.Forward, Vector3.Right, 32 ) ) );

					collisionVertices.Add( p );
					collisionVertices.Add( p1 );
					collisionVertices.Add( p2 );
				}

				foreach ( var h in face.HalfEdges )
				{
					if ( h.ParentEdge.WallType != 0 )
						continue;

					var v0 = h.Vertex1;
					var v1 = h.Vertex2;

					var p = new Vector3( v0.X, v0.Y, 0.1f * face.FloorHeight ) * 32;
					var p1 = new Vector3( v1.X, v1.Y, 0.1f * face.FloorHeight ) * 32;
					var p2 = new Vector3( v0.X, v0.Y, 0 ) * 32;
					var p3 = new Vector3( v1.X, v1.Y, 0 ) * 32;

					var forward = (p1 - p).Normal;
					var right = forward.Cross( Vector3.Up );

					vertices.Add( new SimpleVertex( p2, right, forward, Util.Planar( p2, forward, Vector3.Up, texScale ) ) );
					vertices.Add( new SimpleVertex( p1, right, forward, Util.Planar( p1, forward, Vector3.Up, texScale ) ) );
					vertices.Add( new SimpleVertex( p, right, forward, Util.Planar( p, forward, Vector3.Up, texScale ) ) );

					vertices.Add( new SimpleVertex( p2, right, forward, Util.Planar( p2, forward, Vector3.Up, texScale ) ) );
					vertices.Add( new SimpleVertex( p3, right, forward, Util.Planar( p3, forward, Vector3.Up, texScale ) ) );
					vertices.Add( new SimpleVertex( p1, right, forward, Util.Planar( p1, forward, Vector3.Up, texScale ) ) );

					collisionVertices.Add( p2 );
					collisionVertices.Add( p1 );
					collisionVertices.Add( p );

					collisionVertices.Add( p2 );
					collisionVertices.Add( p3 );
					collisionVertices.Add( p1 );
				}

				var floorBody = new PhysicsBody( physicsWorld );
				floorBody.AddMeshShape( collisionVertices.ToArray(), Enumerable.Range( 0, collisionVertices.Count ).ToArray() );
				floorBodies.Add( floorBody, faceIndex );
			}

			if ( vertices.Count > 0 )
			{
				floorMesh.SetVertexBufferSize( vertices.Count );
				floorMesh.SetVertexBufferData( vertices );
			}

			if ( gridVertices.Count > 0 )
			{
				floorGridMesh.SetVertexBufferSize( gridVertices.Count );
				floorGridMesh.SetVertexBufferData( gridVertices );
			}

			floorMesh.SetVertexRange( 0, vertices.Count );
			floorGridMesh.SetVertexRange( 0, gridVertices.Count );
		}

		public int GetVertexId( int x, int y )
		{
			if ( x < 0 || x >= GridSize.x || y < 0 || y >= GridSize.y ) return -1;

			return (int)(x * GridSize.x + y);
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
					HEMesh.AddEdge( GetVertexId( xcur, ycur ), GetVertexId( xcur + xdir, ycur + ydir ), (byte)type );
				}

				xcur += xdir;
				ycur += ydir;
			}
		}

		public void RemoveEdge( int x, int y, int x1, int y1 )
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
					var edge = HEMesh.GetEdge( GetVertexId( xcur, ycur ), GetVertexId( xcur + xdir, ycur + ydir ) );
					if ( edge != null )
					{
						if ( edge.HalfEdge1.Face.IsFloor || edge.HalfEdge2.Face.IsFloor )
						{
							HEMesh.AddEdge( edge, 0 );
						}
						else
						{
							HEMesh.RemoveEdge( edge );
						}
					}
				}

				xcur += xdir;
				ycur += ydir;
			}

			RebuildMesh();
		}

		public bool HasEdge( int x, int y )
		{
			return HEMesh.HasEdge( x, y );
		}

		public void DebugDraw()
		{
			DebugOverlay.ScreenText( $"{HEMesh.HalfEdges.Count} half edges" );
			DebugOverlay.ScreenText( $"{HEMesh.Faces.Count} faces, {HEMesh.Faces.Count( x => x.IsFloor )} rooms", 1 );

			for ( int i = 0; i < HEMesh.Faces.Count; ++i )
			{
				var face = HEMesh.Faces[i];

				if ( !face.IsFloor )
				{
					continue;
				}

				DebugOverlay.Text( $"room #{i} (height: {face.FloorHeight})", new Vector3( face.Centroid, 1 ) * 32 );

				foreach ( var halfEdge in face.HalfEdges )
				{
					var p1 = new Vector3( halfEdge.Vertex1.X, halfEdge.Vertex1.Y, 0.1f * face.FloorHeight ) * 32;
					var p2 = new Vector3( halfEdge.Next.Vertex1.X, halfEdge.Next.Vertex1.Y, 0.1f * face.FloorHeight ) * 32;

					DebugOverlay.Line( p1, p2, Color.Green, 0, false );
				}
			}

			for ( int i = 0; i < HEMesh.HalfEdges.Count; ++i )
			{
				var h = HEMesh.HalfEdges[i];

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

				var leftPoint = new Vector3( Util.GetInnerPoint( p1_2, p2_2, p3_2, 2.0f, 2.0f ) );
				var rightPoint = new Vector3( Util.GetInnerPoint( p2_2, p3_2, p4_2, 2.0f, 2.0f ) );

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

				DebugOverlay.Line( leftPoint, rightPoint, h.Face.IsOuterFace ? Color.Orange : Color.Cyan, 0, false );
			}

			foreach ( var v in HEMesh.Vertices )
			{
				//DebugOverlay.Text( $"({v.X}, {v.Y})", new Vector3( v.X, v.Y, 0 ) * 32 );
				DebugOverlay.Sphere( new Vector3( v.X, v.Y, 0 ) * 32, 5, v.Connections.Count == 0 ? Color.White.WithAlpha( 0.02f ) : Color.White.WithAlpha( 0.2f ), 0, false );
			}
		}
	}
}
