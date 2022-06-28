
using Sandbox;

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

		Owner.Rotation = Owner.Rotation.RotateAroundAxis( Vector3.Up, 400f * Time.Delta );
	}

}
