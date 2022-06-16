
using Sandbox;

namespace Dungeons;

public partial class DungeonsGame : Sandbox.Game
{

	public DungeonsGame()
	{
		if ( IsServer )
		{
			new EnvironmentLightEntity()
			{
				Rotation = Rotation.From( -45, 15, 0 ),
				Color = Color.White
			};

			new Dungeon() { Size = 2000f };

			Map.Scene.ClearColor = new Color( .08f );
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Pawn();
	}

}

