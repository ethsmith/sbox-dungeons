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

		public int VertexCount => vertices.Count;
		public int EdgeCount => edges.Count;
		public int HalfEdgeCount => halfEdges.Count;
		public int FaceCount => faces.Count;

		private List<Vertex> vertices = new();
		private List<Edge> edges = new();
		private List<HalfEdge> halfEdges = new();
		private List<Face> faces = new();

		public void CreateGrid( int width, int height )
		{
			Clear();

			for ( int x = 0; x < width; ++x )
			{
				for ( int y = 0; y < height; ++y )
				{
					AddVertex( x, y );
				}
			}
		}

		public void Clear()
		{
			vertices.Clear();
			halfEdges.Clear();
			edges.Clear();
			faces.Clear();
		}

		public bool HasEdge( int fromVertexId, int toVertexId )
		{
			return GetEdge( fromVertexId, toVertexId ) != null;
		}

		public Edge GetEdge( int fromVertexId, int toVertexId )
		{
			if ( fromVertexId < 0 || toVertexId < 0 )
				return null;

			if ( fromVertexId == toVertexId )
				return null;

			int numVertices = vertices.Count;
			if ( fromVertexId >= numVertices || toVertexId >= numVertices )
				return null;

			var fromVertex = vertices[fromVertexId];
			var toVertex = vertices[toVertexId];

			return fromVertex.GetConnectionEdge( toVertex );
		}

		public void AddEdge( int edgeIndex, int type )
		{
			if ( edgeIndex < 0 || edgeIndex >= EdgeCount )
				return;

			AddEdge( Edges[edgeIndex], type );
		}

		public void AddEdge( Edge edge, int type )
		{
			AddEdge( GetVertexIndex( edge.Vertex1 ), GetVertexIndex( edge.Vertex2 ), type );
		}

		public void AddEdge( int fromVertexId, int toVertexId, int type = 1 )
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
				edge.WallType = type;

				return;
			}

			edge = AddEdge();
			edge.Vertex1 = fromVertex;
			edge.Vertex2 = toVertex;
			edge.HalfEdge1 = AddHalfEdge();
			edge.HalfEdge2 = AddHalfEdge();
			edge.WallType = type;
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
						if ( halfEdge.Face.IsOuterFace )
						{
							halfEdge.Face.InitializeProperties();
						}

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
					foreach ( var halfEdge in face2.HalfEdges )
					{
						halfEdge.Face = null;
					}

					faces.Remove( face2 );
				}

				if ( face1 != null )
				{
					if ( face1.IsOuterFace )
					{
						face1.InitializeProperties();
					}

					face1.Initialize( edge.HalfEdge1.Next );
				}

				if ( edge.HalfEdge1.Face == null )
				{
					face2 = AddFace();
					face2.Initialize( edge.HalfEdge1 );

					if ( face1 != null && face1.IsInnerFace && face2.IsInnerFace )
					{
						face2.CopyProperties( face1 );
					}
				}

				if ( edge.HalfEdge2.Face == null )
				{
					face2 = AddFace();
					face2.Initialize( edge.HalfEdge2 );

					if ( face1 != null && face1.IsInnerFace && face2.IsInnerFace )
					{
						face2.CopyProperties( face1 );
					}
				}
			}
		}

		public void RemoveEdge( int edgeIndex )
		{
			if ( edgeIndex < 0 || edgeIndex >= EdgeCount )
				return;

			RemoveEdge( Edges[edgeIndex] );
		}

		public void RemoveEdge( Edge edge )
		{
			if ( edge == null )
				return;

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
					var face = FindFace( halfEdge );

					if ( face1 != null && face1.IsInnerFace )
					{
						face.CopyProperties( face1 );
					}
					else if ( face2 != null && face2.IsInnerFace )
					{
						face.CopyProperties( face2 );
					}
				}
			}

			halfEdges.Remove( edge.HalfEdge1 );
			halfEdges.Remove( edge.HalfEdge2 );
			edges.Remove( edge );
		}

		public void RemoveEdge( int fromVertexId, int toVertexId )
		{
			RemoveEdge( GetEdge( fromVertexId, toVertexId ) );
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
			var halfedge = new HalfEdge();
			halfEdges.Add( halfedge );

			return halfedge;
		}

		private Face AddFace()
		{
			var face = new Face();
			face.InitializeProperties();
			faces.Add( face );

			return face;
		}

		private int GetVertexIndex( Vertex vertex )
		{
			if ( vertex == null ) throw new Exception( "null vertex" );

			return vertices.IndexOf( vertex );
		}

		private int GetEdgeIndex( Edge edge )
		{
			if ( edge == null ) throw new Exception( "null edge" );

			return edges.IndexOf( edge );
		}

		private int GetHalfEdgeIndex( HalfEdge halfEdge )
		{
			if ( halfEdge == null ) throw new Exception( "null half edge" );

			return halfEdges.IndexOf( halfEdge );
		}

		private int GetFaceIndex( Face face )
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
				bw.Write( GetVertexIndex( edge.Vertex1 ) );
				bw.Write( GetVertexIndex( edge.Vertex2 ) );
				edge.Write( bw );
				edge.HalfEdge1.Write( bw );
				edge.HalfEdge2.Write( bw );
			}

			foreach ( var face in Faces )
			{
				bw.Write( GetHalfEdgeIndex( face.HalfEdge ) );
				face.Write( bw );
			}

			return true;
		}

		public bool Read( BinaryReader br )
		{
			CreateGrid( br.ReadInt32(), br.ReadInt32() );

			var edgeCount = br.ReadInt32();
			var faceCount = br.ReadInt32();

			for ( int i = 0; i < edgeCount; i++ )
			{
				var edge = AddEdge();
				edge.Vertex1 = vertices[br.ReadInt32()];
				edge.Vertex2 = vertices[br.ReadInt32()];
				edge.Read( br );
				edge.HalfEdge1 = AddHalfEdge();
				edge.HalfEdge2 = AddHalfEdge();
				edge.HalfEdge1.Read( br );
				edge.HalfEdge2.Read( br );
				edge.Initialize();
			}

			for ( int i = 0; i < faceCount; i++ )
			{
				var face = AddFace();
				face.Initialize( halfEdges[br.ReadInt32()] );
				face.Read( br );
			}

			return true;
		}
	}
}
