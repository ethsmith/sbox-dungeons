
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

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		Map.Scene.ClearColor = new Color32( 100, 149, 237 ).ToColor();

		new ModelEntity( "models/citizen/citizen.vmdl" );

		_ = new PostProcessingEntity
		{
			PostProcessingFile = "postprocess/standard.vpost"
		};

		_ = new EnvironmentLightEntity
		{
			Rotation = Rotation.From( 45, 45, 0 ),
			DynamicShadows = true,
			Brightness = 1.0f,
		};
	}

}

