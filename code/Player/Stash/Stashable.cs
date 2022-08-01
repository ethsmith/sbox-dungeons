
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

}
