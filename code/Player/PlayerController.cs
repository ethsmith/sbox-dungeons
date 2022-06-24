
using Sandbox;

namespace Dungeons;

internal class PlayerController : EntityComponent, ISingletonComponent
{

	private Rotation Rotation;
	private Rotation EyeRotation;
	private Vector3 Velocity;
	private Vector3 Position;

	public void Simulate()
	{
		FromEntity();

		var movement = new Vector3( Input.Forward, Input.Left, 0 ).Normal;
		Velocity = EyeRotation * movement * 100f;
		Position += Velocity * Time.Delta;

		var origin = Input.Cursor.Origin;
		var endpos = origin + Input.Cursor.Direction * 3000f;
		var mtr = Trace.Ray( origin, endpos )
			.WorldOnly()
			.Run();

		if ( mtr.Hit )
		{
			var targetRot = Rotation.LookAt( (mtr.HitPosition - Position).WithZ( 0 ) );
			Rotation = Rotation.Slerp( Rotation, targetRot, Time.Delta * 8f );
		}

		MoveHelper helper = new( Position, Velocity );
		helper.Trace = helper.Trace.Size( 16 );
		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			Position = helper.Position;
		}

		ToEntity();
	}

	public void FrameSimulate()
	{
	}

	private void FromEntity()
	{
		Rotation = Entity.Rotation;
		EyeRotation = Entity.EyeRotation;
		Velocity = Entity.Velocity;
		Position = Entity.Position;
	}

	private void ToEntity()
	{
		Entity.Rotation = Rotation;
		Entity.EyeRotation = EyeRotation;
		Entity.Velocity = Velocity;
		Entity.Position = Position;
	}

}
