using System.IO;

namespace HalfEdgeMesh
{
	public class Edge
	{
		public Vertex Vertex1;
		public Vertex Vertex2;
		public HalfEdge HalfEdge1;
		public HalfEdge HalfEdge2;

		public int WallType;
		public int WallHeight;

		public int FoundationType;
		public int FoundationHeight;

		public int PlatformType;

		public void Initialize()
		{
			HalfEdge1.ParentEdge = this;
			HalfEdge1.Opposite = HalfEdge2;
			HalfEdge1.Next = HalfEdge2;
			HalfEdge1.Prev = HalfEdge2;

			HalfEdge2.ParentEdge = this;
			HalfEdge2.Opposite = HalfEdge1;
			HalfEdge2.Next = HalfEdge1;
			HalfEdge2.Prev = HalfEdge1;

			HalfEdge1.Initialize( this, Vertex1, Vertex2 );
			HalfEdge2.Initialize( this, Vertex2, Vertex1 );
		}

		public bool Write( BinaryWriter bw )
		{
			bw.Write( WallType );
			bw.Write( WallHeight );

			bw.Write( FoundationType );
			bw.Write( FoundationHeight );

			bw.Write( PlatformType );

			return true;
		}

		public bool Read( BinaryReader br )
		{
			WallType = br.ReadInt32();
			WallHeight = br.ReadInt32();

			FoundationType = br.ReadInt32();
			FoundationHeight = br.ReadInt32();

			PlatformType = br.ReadInt32();

			return true;
		}
	}
}
