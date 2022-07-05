
using Sandbox;

namespace Dungeons;

partial class DungeonsGame : Sandbox.Game
{

	[Net]
	DungeonEntity Dungeon { get; set; }
	[Net]
	EnvironmentLightEntity Light { get; set; }

	public DungeonsGame()
	{
		if ( IsClient )
		{
			new Hud();
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Player();

		var startRoom = Dungeon.FindRoom( "start" );
		if ( startRoom == null ) return;

		cl.Pawn.Position = startRoom.WorldRect.Center;
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		Map.Scene.ClearColor = Color.Black;

		_ = new NavigationEntity();
		_ = new PostProcessingEntity
		{
			PostProcessingFile = "postprocess/standard.vpost"
		};

		Light = new EnvironmentLightEntity
		{
			Rotation = Rotation.FromPitch( 90 ),
			DynamicShadows = true,
			Brightness = .5f,
			SkyColor = Color.White.Darken( .98f ),
			Color = Color.White.Darken( .85f )
		};

		Dungeon = new DungeonEntity()
		{
			Seed = 32
		};

		Dungeon.Generate();

		Map.Scene.GradientFog.Enabled = true;
		Map.Scene.GradientFog.Color = Color.Black;
		Map.Scene.GradientFog.MaximumOpacity = .8f;
		Map.Scene.GradientFog.StartHeight = 0;
		Map.Scene.GradientFog.EndHeight = 800;
		Map.Scene.GradientFog.DistanceFalloffExponent = 1;
		Map.Scene.GradientFog.VerticalFalloffExponent = 2;
		Map.Scene.GradientFog.StartDistance = 175;
		Map.Scene.GradientFog.EndDistance = 300;
	}

}

