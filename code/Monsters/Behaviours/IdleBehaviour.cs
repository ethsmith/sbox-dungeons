
using Sandbox;
using System.Linq;

namespace Dungeons;

internal class IdleBehaviour : StateBehaviour<Monster>
{

	public override void OnEnter()
	{
		base.OnEnter();

		Log.Info( "Entered idle" );
	}

	public override void OnExit()
	{
		base.OnExit();

		Log.Info( "Exicted idle" );
	}

	public override void OnSimulate()
	{
		base.OnSimulate();

		if ( Owner.Target.IsValid() )
		{
			SetState( MonsterStates.Chase );
			return;
		}

		Owner.Target = Entity.All.FirstOrDefault( x => x is Player pl && pl.Position.Distance( Owner.Position ) < 300 ) as Player;
	}

}
