
using Sandbox;
using System;

namespace Dungeons;

internal class MonsterAnimator : EntityComponent<Monster>
{

	public void Simulate()
	{
		var dir = Entity.Velocity;
		var forward = Entity.Rotation.Forward.Dot( dir );
		var sideward = Entity.Rotation.Right.Dot( dir );
		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		SetAnimParameter( "holdtype", 4 );
		SetAnimParameter( "b_grounded", true );
		SetAnimParameter( "move_direction", angle );
		SetAnimParameter( "move_speed", Entity.Velocity.Length );
		SetAnimParameter( "move_groundspeed", Entity.Velocity.WithZ( 0 ).Length );
		SetAnimParameter( "move_y", sideward );
		SetAnimParameter( "move_x", forward );
		SetAnimParameter( "move_z", Entity.Velocity.z );
	}

	public void Attack()
	{
		Trigger( "b_attack" );
	}

	private void SetAnimParameter( string name, int value ) => (Entity as AnimatedEntity)?.SetAnimParameter( name, value );
	private void SetAnimParameter( string name, float value ) => (Entity as AnimatedEntity)?.SetAnimParameter( name, value );
	private void SetAnimParameter( string name, bool value ) => (Entity as AnimatedEntity)?.SetAnimParameter( name, value );
	private void Trigger( string name ) => (Entity as AnimatedEntity)?.SetAnimParameter( name, true );

}
