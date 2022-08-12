
using Dungeons.Data;
using Sandbox;

namespace Dungeons.Stash;

internal partial class Stashable : Entity
{

	[Net]
	public StashableDetail Detail { get; set; }

	public Stashable()
	{
		Detail = new();
		Transmit = TransmitType.Always;
	}

	public Stashable( StashableDetailData data ) : this()
	{
		Detail.Set( data );
	}

	public void SetStashSlot( int slot )
	{
		if ( IsClient )
		{
			ServerCmd_SetStashSlot( NetworkIdent, slot );
			return;
		}

		Detail.StashSlot = slot;
	}

	public void Drop()
	{
		if ( IsClient )
		{
			ServerCmd_Drop( NetworkIdent );
			return;
		}

		(Parent as StashEntity).Remove( this );
		Detail.StashSlot = -1;
		Parent = null;
	}

	[ConCmd.Server]
	public static void ServerCmd_SetStashSlot( int networkIdent, int slot )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;
		//todo: verify ownership

		var stashable = Entity.FindByIndex( networkIdent ) as Stashable;
		if ( !stashable.IsValid() ) return;

		stashable.SetStashSlot( slot );
	}

	[ConCmd.Server]
	public static void ServerCmd_Drop( int networkIdent )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;
		//todo: verify ownership

		var stashable = Entity.FindByIndex( networkIdent ) as Stashable;
		if ( !stashable.IsValid() ) return;

		stashable.Drop();
	}

}
