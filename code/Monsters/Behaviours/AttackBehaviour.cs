
using Sandbox;

namespace Dungeons;

internal class AttackBehaviour : StateBehaviour<Monster>
{

	private TimeUntil TimeUntilAttack;

	public override void OnSimulate()
	{
		base.OnSimulate();

		var target = Owner.Target;
		if ( !target.IsValid() )
		{
			SetState( MonsterStates.Idle );
			return;
		}

		var lookdir = Owner.Target.Position - Owner.Position;
		Owner.Rotation = Rotation.Slerp( Owner.Rotation, Rotation.LookAt( lookdir ), 8f * Time.Delta );

		if ( TimeUntilAttack > 0 ) return;

		var dist = target.Position.Distance( Owner.Position );
		if( dist > 48 )
		{
			SetState( MonsterStates.Chase );
			return;
		}

		DoAttack();

		TimeUntilAttack = 1f;
	}

	private void DoAttack()
	{
		Owner.Animator.Attack();
	}

}
