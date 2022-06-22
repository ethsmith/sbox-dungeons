
using Sandbox;

namespace Dungeons;

public partial class DungeonsGame : Sandbox.Game
{

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Pawn();
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		Map.Scene.ClearColor = new Color32( 100, 149, 237 ).ToColor();

		_ = new Dungeon()
		{
			Seed = 32
		};

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

