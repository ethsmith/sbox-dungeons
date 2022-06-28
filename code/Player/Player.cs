
using Sandbox;

namespace Dungeons;

internal partial class Player : AnimatedEntity
{

	public PlayerController Controller
	{
		get => Components.Get<PlayerController>();
		set => Components.Add( value );
	}

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public PlayerAnimator Animator
	{
		get => Components.Get<PlayerAnimator>();
		set => Components.Add( value );
	}

	public override void Spawn()
	{
		base.Spawn();

		Controller = new PlayerController();
		Camera = new PlayerCamera();
		Animator = new PlayerAnimator();
		PhysicsEnabled = true;
		EnableAllCollisions = true;

		SetModel( "models/citizen/citizen.vmdl" );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller.Simulate();
		Animator.Simulate();

		var eyepos = Position + Vector3.Up * 64f;
		var tr = Trace.Ray( eyepos, eyepos + Rotation.Forward * 300f )
			.Run();

		DebugOverlay.TraceResult( tr );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller.FrameSimulate();
	}

}
