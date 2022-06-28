
using Sandbox;
using System;
using System.Collections.Generic;

namespace Dungeons;

internal class StateComponent : EntityComponent
{

	int currentState = -int.MaxValue + 0_1_2_3_4_5_6_7_8_9;

	Dictionary<int, StateBehaviour> Behaviours = new();

	public void SetState<TEnum>( TEnum newState ) where TEnum : struct, Enum => SetState( Convert.ToInt32( newState ) );
	public void SetState( int state )
	{
		if ( currentState == state )
			return;

		GetBehaviour( currentState )?.OnExit();

		currentState = state;

		GetBehaviour( currentState )?.OnEnter();
	}

	public void SetBehaviour<TEnum>( TEnum state, StateBehaviour behaviour ) where TEnum : struct, Enum => SetBehaviour( Convert.ToInt32( state ), behaviour );
	public void SetBehaviour( int state, StateBehaviour behaviour )
	{
		if ( behaviour == null )
		{
			Behaviours.Remove( state );
			return;
		}

		Behaviours[state] = behaviour;
		behaviour.Owner = Entity;
	}

	public void Simulate()
	{
		GetBehaviour( currentState )?.OnSimulate();
	}

	StateBehaviour GetBehaviour( int state )
	{
		if ( !Behaviours.ContainsKey( state ) )
			return null;
		return Behaviours[state];
	}

}
