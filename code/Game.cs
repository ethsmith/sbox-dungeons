
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

		Light.Color = Color.White.Darken( .85f );
		Light.SkyColor = Color.White.Darken( .95f );
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

		Dungeon = new DungeonEntity()
		{
			Seed = 32
		};

		Dungeon.Generate();

		_ = new PostProcessingEntity
		{
			PostProcessingFile = "postprocess/standard.vpost"
		};

		Light = new EnvironmentLightEntity
		{
			Rotation = Rotation.From( 125, 45, 0 ),
			DynamicShadows = true,
			Brightness = .5f,
			SkyColor = Color.Black,
			Color = Color.White
		};
	}

}

