
using Dungeons.Data;
using Dungeons.Stash;
using Dungeons.UI;
using Sandbox;
using System.Linq;

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

	public NavigationAgent Agent
	{
		get => Components.Get<NavigationAgent>();
		set => Components.Add( value );
	}

	[Net]
	public StashEntity Stash { get; set; }

	[Net]
	public StashEntity Stash2 { get; set; }

	[Net]
	public StashEntity StashEquipment { get; set; }

	[Net]
	public SpotLightEntity LightRadius { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Controller = new PlayerController();
		Camera = new PlayerCamera();
		Animator = new PlayerAnimator();
		Agent = new NavigationAgent();
		PhysicsEnabled = true;
		EnableAllCollisions = true;

		LightRadius = new();
		LightRadius.SetParent( this );
		LightRadius.LocalPosition = Vector3.Up * 100f;
		LightRadius.Rotation = Rotation.FromPitch( 85 );
		LightRadius.DynamicShadows = true;
		LightRadius.Range = 250f;
		LightRadius.Color = Color.White.Darken( .9f );

		Stash = new();
		Stash.Parent = this;
		Stash.LocalPosition = 0;
		Stash.SlotCount = 40;
		Stash.Add( new Stashable( ItemGenerator.Random() ) );
		Stash.Add( new Stashable( ItemGenerator.Random() ) );
		Stash.Add( new Stashable( ItemGenerator.Random() ) );

		StashEquipment = new();
		StashEquipment.Parent = this;
		StashEquipment.LocalPosition = 0;
		StashEquipment.SlotCount = 8;

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 64 ) );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//Controller.Simulate();
		Animator.Simulate();
		Agent.Simulate();

		HoveredEntity?.SetGlow( false );
		HoveredEntity = FindByIndex( (int)Input.Position.x );
		HoveredEntity?.SetGlow( true, Color.Red );

		SimulateInput();
	}

}
