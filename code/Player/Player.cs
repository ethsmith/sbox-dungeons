
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
		Stash.SlotCount = 60;
		Stash.Add( new Stashable( new Data.StashableDetailData()
		{
			Durability = 24,
			Identity = 0,
			Quantity = 1,
			StashSlot = 1
		} ) );

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 64 ) );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//Controller.Simulate();
		Animator.Simulate();

		var start = Input.Cursor.Origin;
		var end = Input.Cursor.Origin + Input.Cursor.Direction * 5000f;
		var tr = Trace.Ray( start, end )
			.WorldOnly()
			.Run();

		if ( !InputBlocked )
		{
			if ( Input.Down( InputButton.PrimaryAttack ) )
			{
				Agent.SetDestination( tr.HitPosition );
			}
		}

		Agent.Simulate();
		UpdateTarget();
	}

	private Entity Target;
	private void UpdateTarget()
	{
		var start = Input.Cursor.Origin;
		var end = Input.Cursor.Origin + Input.Cursor.Direction * 5000f;
		var tr = Trace.Ray( start, end )
			.EntitiesOnly()
			.RunAll();

		var newTarget = tr?.FirstOrDefault( x => x.Entity != null && x.Entity != this ).Entity;

		if ( newTarget != Target )
		{
			Target?.SetGlow( false );
			Target = newTarget;
			Target?.SetGlow( true, Color.Red );
		}
	}

	public bool InputBlocked => Input.Down( InputButton.Zoom );

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		if ( DungeonsPanel.InputBlocked() )
		{
			inputBuilder.SetButton( InputButton.Zoom );
		}
	}

}
