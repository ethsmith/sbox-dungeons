using Sandbox;
using System.Linq;

namespace Architect
{
	public class GridObject
	{
		SceneObject so;
		PhysicsBody body;

		public GridObject( SceneWorld sceneWorld, PhysicsWorld physicsWorld, int width, int length )
		{
			var mesh = new Mesh( Material.Load( "materials/dev/gray_grid_4.vmat" ) );
			mesh.SetBounds( Vector3.Zero, new Vector3( width * 32, length * 32, 100.0f ) );

			var vertices = new SimpleVertex[]
			{
				new SimpleVertex( new Vector3( 0, 0, 0 ), Vector3.Up, Vector3.Forward, new Vector2( 0, 0 ) ),
				new SimpleVertex( new Vector3( width * 32, 0, 0 ), Vector3.Up, Vector3.Forward, new Vector2( width / 4.0f, 0 ) ),
				new SimpleVertex( new Vector3( width * 32, length * 32, 0 ), Vector3.Up, Vector3.Forward, new Vector2( width / 4.0f, length / 4.0f ) ),
				new SimpleVertex( new Vector3( 0, length * 32, 0 ), Vector3.Up, Vector3.Forward, new Vector2( 0, length / 4.0f ) )
			};

			var indices = new int[]
			{
				0, 1, 2, 2, 3, 0
			};

			mesh.CreateVertexBuffer<SimpleVertex>( 4, SimpleVertex.Layout, vertices );
			mesh.CreateIndexBuffer( 6, indices );

			var model = Model.Builder
				.AddMesh( mesh )
				.Create();

			so = new SceneObject( sceneWorld, model, new Transform( Vector3.Zero ) );

			body = new PhysicsBody( physicsWorld );
			body.AddMeshShape( vertices.Select( x => x.position ).ToArray(), indices );
		}

		public Vector3 Position { get => so.Position; set => so.Position = value; }

		public void Destroy()
		{
			body?.Remove();
			body = null;

			so?.Delete();
			so = null;
		}
	}
}
