
using Sandbox;

namespace Dungeons;

public partial class DungeonsGame : Sandbox.Game
{

	public DungeonsGame()
	{
		if ( IsServer )
		{
			var light = new EnvironmentLightEntity();
			light.Rotation = Rotation.From( -45, 15, 0 );
			light.Color = Color.Red;
			var watermelon = new ModelEntity( "models/sbox_props/watermelon/watermelon.vmdl" );
			watermelon.Position = Vector3.Zero;
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Pawn();
	}

	[Event.Frame]
	public void OnFrame()
	{
		DebugOverlay.Sphere( Vector3.Zero, 100f, Color.Green );
	}

}

