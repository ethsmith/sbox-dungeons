using System.Collections.Generic;

namespace HalfEdgeMesh
{
	public class Vertex
	{
		public int X;
		public int Y;
		public readonly List<HalfEdge> Connections = new();

		public Vector2 Position => new( X, Y );

		public void Connect( HalfEdge halfEdge )
		{
			Connections.Add( halfEdge );
			Connections.Sort( ( x, y ) => (x.Angle < y.Angle) ? -1 : 1 );

			var index = Connections.IndexOf( halfEdge );
			var index1 = ((index - 1) + Connections.Count) % Connections.Count;
			var index2 = ((index + 1) + Connections.Count) % Connections.Count;

			var halfEdge1 = Connections[index1];
			var halfEdge2 = Connections[index2];

			halfEdge.Prev = halfEdge1.Opposite;
			halfEdge1.Opposite.Next = halfEdge;

			halfEdge.Opposite.Next = halfEdge2;
			halfEdge2.Prev = halfEdge.Opposite;
		}

		public void Disconnect( HalfEdge halfEdge )
		{
			if ( halfEdge == null )
				return;

			if ( !Connections.Remove( halfEdge ) )
				return;

			halfEdge.Next.Prev = halfEdge.Opposite.Prev;
			halfEdge.Opposite.Prev.Next = halfEdge.Next;
		}

		public Edge GetConnectionEdge( Vertex Vertex )
		{
			foreach ( var connection in Connections )
			{
				if ( connection.Vertex1 == Vertex || connection.Vertex2 == Vertex )
				{
					return connection.ParentEdge;
				}
			}

			return null;
		}
	}
}
