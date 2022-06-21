
using System.Collections.Generic;

namespace HalfEdgeMesh
{
	public class Face
	{
		public HalfEdge HalfEdge;
		public bool IsOuterFace;
		public float SignedArea;
		public Vector2 Centroid;

		public void Initialize( HalfEdge halfEdge )
		{
			HalfEdge = halfEdge;

			foreach ( var faceHalfEdge in HalfEdge.HalfEdges )
			{
				faceHalfEdge.Face = this;
			}

			ComputeCentroid();
			ComputeOuterFace();
		}

		public IEnumerable<Vertex> NonParallelVertices
		{
			get
			{
				HalfEdge prevHalfEdge = null;

				foreach ( var halfEdge in HalfEdge.HalfEdges )
				{
					if ( halfEdge.IsParallel( prevHalfEdge ) )
						continue;

					yield return halfEdge.Vertex1;

					prevHalfEdge = halfEdge;
				}
			}
		}

		public IEnumerable<Vertex> Vertices
		{
			get
			{
				foreach ( var halfEdge in HalfEdge.HalfEdges )
				{
					yield return halfEdge.Vertex1;
				}
			}
		}

		private void ComputeCentroid()
		{
			Centroid = Vector2.Zero;
			SignedArea = 0.0f;

			foreach ( var halfEdge in HalfEdge.HalfEdges )
			{
				var p1 = new Vector2( halfEdge.Vertex1.X, halfEdge.Vertex1.Y );
				var p2 = new Vector2( halfEdge.Vertex2.X, halfEdge.Vertex2.Y );

				float area = (p1.x * p2.y) - (p2.x * p1.y);

				SignedArea += area;

				Centroid.x += (p1.x + p2.x) * area;
				Centroid.y += (p2.y + p1.y) * area;
			}

			if ( SignedArea == 0.0f )
			{
				Centroid = new Vector2( HalfEdge.Vertex1.X, HalfEdge.Vertex1.Y );

				return;
			}

			SignedArea *= 0.5f;
			Centroid.x *= 1.0f / (6.0f * SignedArea);
			Centroid.y *= 1.0f / (6.0f * SignedArea);
		}

		private void ComputeOuterFace()
		{
			if ( HalfEdge == null )
			{
				return;
			}

			var hasInteriorFace = false;
			var start = HalfEdge;
			var current = start;

			do
			{
				if ( current.Opposite.Face != this )
				{
					hasInteriorFace = true;

					break;
				}

				current = current.Next;
			}
			while ( current != start );

			if ( !hasInteriorFace )
			{
				IsOuterFace = true;

				return;
			}

			IsOuterFace = -SignedArea >= 0.0f;
		}
	}
}
