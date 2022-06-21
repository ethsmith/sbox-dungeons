using System;
using System.Collections.Generic;

namespace HalfEdgeMesh
{
	public class HalfEdge
	{
		public HalfEdge Prev;
		public HalfEdge Next;
		public HalfEdge Opposite;
		public Vertex Vertex1;
		public Vertex Vertex2;
		public Edge ParentEdge;
		public Face Face;
		public int Thickness;

		public void Initialize( Edge parentEdge, Vertex vertex1, Vertex vertex2 )
		{
			ParentEdge = parentEdge;
			Vertex1 = vertex1;
			Vertex2 = vertex2;

			Vertex1.Connect( this );
		}

		public float Angle => MathF.Atan2( -(Vertex2.Y - Vertex1.Y), Vertex2.X - Vertex1.X );

		public IEnumerable<HalfEdge> HalfEdges
		{
			get
			{
				var start = this;
				var current = start;

				do
				{
					if ( current == null ) throw new Exception( "null half edge" );

					yield return current;
					current = current.Next;
				}
				while ( current != start );
			}
		}

		public bool IsParallel( HalfEdge other )
		{
			if ( other == null )
				return false;

			var a1 = Vertex2.Y - Vertex1.Y;
			var b1 = Vertex2.X - Vertex1.X;

			var a2 = other.Vertex2.Y - other.Vertex1.Y;
			var b2 = other.Vertex2.X - other.Vertex1.X;

			var d = a1 * b2 - a2 * b1;

			return d == 0;
		}
	}
}
