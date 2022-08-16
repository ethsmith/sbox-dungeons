
using Sandbox;

namespace Dungeons;

internal class PlayerCamera : CameraMode
{

	public override void Update()
	{
		var offset = Vector3.Up * 275f + Vector3.Backward * 62f;
		var target = Entity.Position + offset;
		Position = Vector3.Lerp( Position, target, Time.Delta * 93f, true );
		Rotation = Rotation.FromPitch( 75f );
	}

}
