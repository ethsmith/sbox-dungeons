
using Sandbox;

namespace Dungeons;

internal class ChaseBehaviour : StateBehaviour<Monster>
{

	public override void OnExit()
	{
		base.OnExit();

		Owner.Velocity = 0;
	}

	public override void OnSimulate()
	{
		base.OnSimulate();

		if ( !Owner.Target.IsValid() )
		{
			SetState( MonsterStates.Idle );
			return;
		}

		var dist = Owner.Target.Position.Distance( Owner.Position );
		if( dist > 300f )
		{
			Owner.Target = null;
			return;
		}

		if( dist < 40f )
		{
			SetState( MonsterStates.Attack );
			Owner.Agent.Stop();
			return;
		}

		Owner.Agent.SetDestination( Owner.Target.Position );
	}

}
