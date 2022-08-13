
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Stash;

internal partial class StashEntity : Entity
{

	[Net]
	public int SlotCount { get; set; } = 50;
	[Net]
	public IList<Stashable> Items { get; set; } 

	public StashEntity()
	{
		Transmit = TransmitType.Owner;
	}

	public bool Add( Stashable item )
	{
		Host.AssertServer();

		if ( Items.Contains( item ) ) 
			return false;

		Items.Add( item );
		item.Parent = this;
		item.LocalPosition = 0;

		return true;
	}

	public bool AddWithNextAvailableSlot( Stashable item )
	{
		if ( Add( item ) )
		{
			item.Detail.StashSlot = FirstAvailableSlot();
			return true;
		}
		return false;
	}

	public bool Remove( Stashable item )
	{
		Host.AssertServer();

		if ( !Items.Contains( item ) ) 
			return false;

		Items.Remove( item );
		item.Parent = null;

		return true;
	}

	private bool SlotsOpen( int slot )
	{
		return !Items.Any( x => x.Detail.StashSlot == slot );
	}

	private int FirstAvailableSlot()
	{
		for ( int i = 0; i < SlotCount; i++ )
		{
			if ( !SlotsOpen( i ) )
				continue;
			return i;
		}
		return -1;
	}

	[ConCmd.Server]
	public static void ServerCmd_MoveItem( int stashIdent, int itemIdent, int slotIndex )
	{
		//todo: verify ownership

		var stashable = FindByIndex( itemIdent ) as Stashable;
		var targetStash = FindByIndex( stashIdent ) as StashEntity;

		if ( !stashable.IsValid() || !targetStash.IsValid() ) 
			return;

		if( slotIndex == -1 )
			slotIndex = targetStash.FirstAvailableSlot();

		if ( !targetStash.SlotsOpen( slotIndex ) ) 
			return;

		(stashable.Parent as StashEntity)?.Remove( stashable );
		targetStash.Add( stashable );
		stashable.Detail.StashSlot = slotIndex;
	}

}
