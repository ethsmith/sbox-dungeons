
using Sandbox;

namespace Dungeons;

internal partial class Monster : ModelEntity
{

	private StateComponent State => Components.GetOrCreate<StateComponent>();

	public override void Spawn()
	{
		base.Spawn();

		State.SetBehaviour( MonsterStates.Idle, new IdleBehaviour() );
		State.SetState( MonsterStates.Idle );

		SetModel( "models/citizen/citizen.vmdl" );
	}

	[Event.Tick]
	private void OnTick()
	{
		State?.Simulate();
	}

}

public enum MonsterStates
{
	Idle,
	Chase,
	Attack
}
