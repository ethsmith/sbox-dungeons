
using Sandbox;

namespace Dungeons;

partial class DungeonsGame : Sandbox.Game
{

	[Net]
	DungeonEntity Dungeon { get; set; }

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Pawn();

		var startRoom = Dungeon.FindRoom( "start" );
		if ( startRoom == null ) return;

		cl.Pawn.Position = startRoom.WorldRect.Center;
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		Map.Scene.ClearColor = new Color32( 100, 149, 237 ).ToColor();

		Dungeon = new DungeonEntity()
		{
			Seed = 32
		};

		Dungeon.Generate();

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

