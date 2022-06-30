
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
			return;
		}

		var lookdir = (Owner.Target.Position - Owner.Position).WithZ( 0 ).Normal;
		var lookrot = Rotation.LookAt( lookdir );
		Owner.Rotation = Rotation.Slerp( Owner.Rotation, lookrot, 8f * Time.Delta );
		Owner.Velocity = lookdir * 80f;
		Owner.Position += Owner.Velocity * Time.Delta;
	}

}
