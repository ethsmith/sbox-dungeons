
using Sandbox;
using System;

namespace Dungeons;

public class StateBehaviour
{

	public Entity Owner { get; set; }

	public virtual void OnEnter() { }
	public virtual void OnExit() { }
	public virtual void OnSimulate() { }

	protected void SetState( int newstate )
	{
		if ( !Owner.IsValid() )
			return;

		Owner.Components.GetOrCreate<StateComponent>().SetState( newstate );
	}

	protected void SetState<TEnum>( TEnum newState )
		where TEnum : struct, Enum
	{
		SetState( Convert.ToInt32( newState ) );
	}

}

public class StateBehaviour<TEntity> : StateBehaviour 
	where TEntity : Entity
{
	public new TEntity Owner => base.Owner as TEntity;
}
