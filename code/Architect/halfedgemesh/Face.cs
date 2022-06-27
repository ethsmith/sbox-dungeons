using System.Collections.Generic;
using System.IO;

namespace HalfEdgeMesh
{
	public class Face
	{
		public HalfEdge HalfEdge;
		public bool IsOuterFace;
		public float SignedArea;
		public Vector2 Centroid;

		public int FloorType;
		public int FloorHeight;

		public bool IsInnerFace => !IsOuterFace;
		public bool IsFloor => !IsOuterFace && FloorType != 0;

		public void Initialize( HalfEdge halfEdge )
		{
			HalfEdge = halfEdge;

			foreach ( var faceHalfEdge in HalfEdges )
			{
				faceHalfEdge.Face = this;
			}

			ComputeCentroid();
			ComputeOuterFace();
		}

		public void InitializeProperties()
		{
			FloorType = 1;
			FloorHeight = 0;
		}

		public void CopyProperties( Face face )
		{
			FloorType = face.FloorType;
			FloorHeight = face.FloorHeight;
		}

		public IEnumerable<HalfEdge> HalfEdges => HalfEdge.HalfEdges;

		public IEnumerable<HalfEdge> NonParallelHalfEdges
		{
			get
			{
				HalfEdge prevHalfEdge = null;

				foreach ( var halfEdge in HalfEdges )
				{
					if ( halfEdge.IsParallel( prevHalfEdge ) )
						continue;

					yield return halfEdge;

					prevHalfEdge = halfEdge;
				}
			}
		}

		public IEnumerable<Vertex> Vertices
		{
			get
			{
				foreach ( var halfEdge in HalfEdges )
				{
					yield return halfEdge.Vertex1;
				}
			}
		}

		public IEnumerable<Vertex> NonParallelVertices
		{
			get
			{
				HalfEdge prevHalfEdge = null;

				foreach ( var halfEdge in HalfEdges )
				{
					if ( halfEdge.IsParallel( prevHalfEdge ) )
						continue;

					yield return halfEdge.Vertex1;

					prevHalfEdge = halfEdge;
				}
			}
		}

		private void ComputeCentroid()
		{
			Centroid = Vector2.Zero;
			SignedArea = 0.0f;

			foreach ( var halfEdge in HalfEdges )
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

		public bool Write( BinaryWriter bw )
		{
			bw.Write( FloorType );
			bw.Write( FloorHeight );

			return true;
		}

		public bool Read( BinaryReader br )
		{
			FloorType = br.ReadInt32();
			FloorHeight = br.ReadInt32();

			return true;
		}
	}
}
