
using Sandbox;
using System.Collections.Generic;
using System.Linq;

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

			Map.Scene.ClearColor = Color.White;
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Pawn();
	}

}

