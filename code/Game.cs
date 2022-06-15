
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

			new ModelEntity( "models/sbox_props/watermelon/watermelon.vmdl" )
			{
				Position = Vector3.Zero
			};

			Map.Scene.ClearColor = new Color( .08f );
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Pawn();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		DebugOverlay.Sphere( Vector3.Zero, 20f, Color.Green );
	}

}

