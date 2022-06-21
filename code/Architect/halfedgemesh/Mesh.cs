using System;
using System.Collections.Generic;
using System.IO;

namespace HalfEdgeMesh
{
	public class Mesh
	{
		public IReadOnlyList<Vertex> Vertices => vertices;
		public IReadOnlyList<Edge> Edges => edges;
		public IReadOnlyList<HalfEdge> HalfEdges => halfEdges;
		public IReadOnlyList<Face> Faces => faces;

		private List<Vertex> vertices = new();
		private List<Edge> edges = new();
		private List<HalfEdge> halfEdges = new();
		private List<Face> faces = new();

		public void CreateGrid( int width, int height )
		{
			vertices.Clear();
			halfEdges.Clear();
			edges.Clear();

			for ( int x = 0; x < width; ++x )
			{
				for ( int y = 0; y < height; ++y )
				{
					AddVertex( x, y );
				}
			}
		}

		public bool HasEdge( int fromVertexId, int toVertexId )
		{
			if ( fromVertexId < 0 || toVertexId < 0 )
				return false;

			if ( fromVertexId == toVertexId )
				return false;

			int numVertices = vertices.Count;
			if ( fromVertexId >= numVertices || toVertexId >= numVertices )
				return false;

			var fromVertex = vertices[fromVertexId];
			var toVertex = vertices[toVertexId];

			return fromVertex.GetConnectionEdge( toVertex ) != null;
		}

		public void AddEdge( int fromVertexId, int toVertexId, byte type = 1 )
		{
			if ( fromVertexId < 0 || toVertexId < 0 )
				return;

			if ( fromVertexId == toVertexId )
				return;

			int numVertices = vertices.Count;
			if ( fromVertexId >= numVertices || toVertexId >= numVertices )
				return;

			var fromVertex = vertices[fromVertexId];
			var toVertex = vertices[toVertexId];

			var edge = fromVertex.GetConnectionEdge( toVertex );
			if ( edge != null )
			{
				if ( type == 0 )
				{
					var halfEdgeSet = new HashSet<HalfEdge>
					{
						edge.HalfEdge1.Prev,
						edge.HalfEdge1.Next,
						edge.HalfEdge2.Prev,
						edge.HalfEdge2.Next
					};

					var face1 = edge.HalfEdge1.Face;
					var face2 = edge.HalfEdge2.Face;

					if ( face1 != null )
					{
						foreach ( var halfEdge in edge.HalfEdge1.HalfEdges )
						{
							halfEdge.Face = null;
						}

						faces.Remove( face1 );
					}

					if ( face2 != null )
					{
						foreach ( var halfEdge in edge.HalfEdge2.HalfEdges )
						{
							halfEdge.Face = null;
						}

						faces.Remove( face2 );
					}

					edge.Vertex1.Disconnect( edge.HalfEdge1 );
					edge.Vertex2.Disconnect( edge.HalfEdge2 );

					foreach ( var halfEdge in halfEdgeSet )
					{
						if ( halfEdge != edge.HalfEdge1 && halfEdge != edge.HalfEdge2 && halfEdge.Face == null )
						{
							FindFace( halfEdge );
						}
					}

					halfEdges.Remove( edge.HalfEdge1 );
					halfEdges.Remove( edge.HalfEdge2 );
					edges.Remove( edge );
				}

				return;
			}
			else if ( type == 0 )
			{
				return;
			}

			edge = AddEdge();
			edge.Vertex1 = fromVertex;
			edge.Vertex2 = toVertex;
			edge.HalfEdge1 = AddHalfEdge();
			edge.HalfEdge2 = AddHalfEdge();
			edge.Initialize();

			if ( fromVertex.Connections.Count == 1 && toVertex.Connections.Count == 1 )
			{
				FindFace( edge.HalfEdge1 );
			}
			else if ( fromVertex.Connections.Count == 1 || toVertex.Connections.Count == 1 )
			{
				var halfEdges = new HashSet<HalfEdge>
				{
					edge.HalfEdge1.Prev,
					edge.HalfEdge1.Next,
					edge.HalfEdge2.Prev,
					edge.HalfEdge2.Next
				};

				foreach ( var halfEdge in halfEdges )
				{
					if ( halfEdge.ParentEdge == edge )
					{
						continue;
					}

					if ( halfEdge.Face != null )
					{
						halfEdge.Face.Initialize( halfEdge );

						break;
					}
				}
			}
			else
			{
				var face1 = edge.HalfEdge1.Next.Face;
				var face2 = edge.HalfEdge1.Prev.Face;

				if ( face1 != face2 )
				{
					foreach ( var halfEdge in face2.HalfEdge.HalfEdges )
					{
						halfEdge.Face = null;
					}

					faces.Remove( face2 );
				}

				if ( face1 != null )
				{
					face1.Initialize( edge.HalfEdge1.Next );
				}

				if ( edge.HalfEdge1.Face == null )
				{
					face2 = AddFace();
					face2.Initialize( edge.HalfEdge1 );
				}

				if ( edge.HalfEdge2.Face == null )
				{
					face2 = AddFace();
					face2.Initialize( edge.HalfEdge2 );
				}
			}
		}

		Face FindFace( HalfEdge halfEdge )
		{
			if ( halfEdge.Face != null )
			{
				return halfEdge.Face;
			}

			var face = AddFace();
			face.Initialize( halfEdge );

			return face;
		}

		private Vertex AddVertex( int x, int y )
		{
			var vertex = new Vertex { X = x, Y = y };
			vertices.Add( vertex );

			return vertex;
		}

		private Edge AddEdge()
		{
			var edge = new Edge();
			edges.Add( edge );

			return edge;
		}

		private HalfEdge AddHalfEdge()
		{
			var halfedge = new HalfEdge { Thickness = 2 };
			halfEdges.Add( halfedge );

			return halfedge;
		}

		private Face AddFace()
		{
			var face = new Face();
			faces.Add( face );

			return face;
		}

		private int VertexIndex( Vertex vertex )
		{
			if ( vertex == null ) throw new Exception( "null vertex" );

			return vertices.IndexOf( vertex );
		}

		private int EdgeIndex( Edge edge )
		{
			if ( edge == null ) throw new Exception( "null edge" );

			return edges.IndexOf( edge );
		}

		private int HalfEdgeIndex( HalfEdge halfEdge )
		{
			if ( halfEdge == null ) throw new Exception( "null half edge" );

			return halfEdges.IndexOf( halfEdge );
		}

		private int FaceIndex( Face face )
		{
			if ( face == null ) throw new Exception( "null face" );

			return faces.IndexOf( face );
		}

		public bool Write( BinaryWriter bw )
		{
			bw.Write( 32 );
			bw.Write( 32 );

			bw.Write( Edges.Count );
			bw.Write( Faces.Count );

			foreach ( var edge in Edges )
			{
				bw.Write( VertexIndex( edge.Vertex1 ) );
				bw.Write( VertexIndex( edge.Vertex2 ) );
			}

			foreach ( var face in Faces )
			{
				bw.Write( HalfEdgeIndex( face.HalfEdge ) );
			}

			return true;
		}

		public bool Read( BinaryReader br )
		{
			CreateGrid( br.ReadInt32(), br.ReadInt32() );

			var edgeCount = br.ReadInt32();
			var faceCount = br.ReadInt32();

			Log.Info( $"edgeCount: {edgeCount}, faceCount: {faceCount}" );

			for ( int i = 0; i < edgeCount; i++ )
			{
				var edge = AddEdge();
				edge.Vertex1 = vertices[br.ReadInt32()];
				edge.Vertex2 = vertices[br.ReadInt32()];
				edge.HalfEdge1 = AddHalfEdge();
				edge.HalfEdge2 = AddHalfEdge();
				edge.Initialize();
			}

			for ( int i = 0; i < faceCount; i++ )
			{
				var face = AddFace();
				face.Initialize( halfEdges[br.ReadInt32()] );
			}

			return true;
		}
	}
}
