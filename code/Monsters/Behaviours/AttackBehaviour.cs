
using Sandbox;

namespace Dungeons;

internal class AttackBehaviour : StateBehaviour<Monster>
{

	private TimeUntil TimeUntilAttack;

	public override void OnSimulate()
	{
		base.OnSimulate();

		if ( TimeUntilAttack > 0 ) return;

		var target = Owner.Target;
		if ( !target.IsValid() )
		{
			SetState( MonsterStates.Idle );
			return;
		}

		var dist = target.Position.Distance( Owner.Position );
		if( dist > 60 )
		{
			SetState( MonsterStates.Chase );
			return;
		}

		DoAttack();

		TimeUntilAttack = 1f;
	}

	private async void DoAttack()
	{
		Log.Info( "Hurt you" );
	}

}
