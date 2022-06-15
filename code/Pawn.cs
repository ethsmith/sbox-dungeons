
using Sandbox;

namespace Dungeons;

partial class Pawn : Entity
{

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Rotation = Rotation.From( Input.Rotation.Angles().WithPitch( 0 ) );
		EyeRotation = Input.Rotation;

		var movement = new Vector3( Input.Forward, Input.Left, 0 ).Normal;
		Velocity = EyeRotation * movement;
		Velocity *= Input.Down( InputButton.Run ) ? 1000 : 200;

		MoveHelper helper = new( Position, Velocity );
		helper.Trace = helper.Trace.Size( 16 );
		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			Position = helper.Position;
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		EyeRotation = Input.Rotation;
	}

}
