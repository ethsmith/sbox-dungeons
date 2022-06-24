
using Sandbox;

namespace Dungeons;

internal class PlayerCamera : CameraMode
{

	public override void Update()
	{
		var offset = Vector3.Up * 275f + Vector3.Backward * 88f;
		var target = Entity.Position + offset;
		Position = Vector3.Lerp( Position, target, Time.Delta * 3f, true );
		Rotation = Rotation.LookAt( Vector3.Down + Vector3.Forward * .35f );
	}

}
