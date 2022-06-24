
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

		SetModel( "models/citizen/citizen.vmdl" );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller.Simulate();
		Animator.Simulate();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller.FrameSimulate();
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		Camera.Build( ref camSetup );
	}

}
