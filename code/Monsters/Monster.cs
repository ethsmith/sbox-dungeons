
using Sandbox;

namespace Dungeons;

internal partial class Monster : AnimatedEntity
{

	[Net]
	public Player Target { get; set; }

	public MonsterAnimator Animator
	{
		get => Components.Get<MonsterAnimator>();
		set => Components.Add( value );
	}

	public NavigationAgent Agent
	{
		get => Components.Get<NavigationAgent>();
		set => Components.Add( value );
	}

	private StateComponent State => Components.GetOrCreate<StateComponent>();

	public override void Spawn()
	{
		base.Spawn();

		Animator = new();
		Agent = new();
		Agent.MoveSpeed = 85;

		State.SetBehaviour( MonsterStates.Idle, new IdleBehaviour() );
		State.SetBehaviour( MonsterStates.Chase, new ChaseBehaviour() );
		State.SetBehaviour( MonsterStates.Attack, new AttackBehaviour() );
		State.SetState( MonsterStates.Idle );

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( Vector3.Up * 16f, Vector3.Up * 64, 16f ) );

		LocalScale = .92f;
		RenderColor = RenderColor.WithGreen( .8f );
		Predictable = false;
	}

	[Event.Tick]
	private void OnTick()
	{
		State?.Simulate();
		Animator?.Simulate();
		Agent?.Simulate();
	}

}

public enum MonsterStates
{
	Idle,
	Chase,
	Attack
}
