
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

	private List<StashConstraint> Constraints = new();

	public StashEntity()
	{
		Transmit = TransmitType.Always;
	}

	public bool Add( Stashable item )
	{
		Host.AssertServer();

		if ( Items.Contains( item ) ) 
			return false;

		Items.Add( item );
		item.Parent = this;
		item.LocalPosition = 0;

		GetPlayer()?.OnItemAdded( this, item );

		return true;
	}

	public bool AcceptsItem( Stashable item, int cell )
	{
		if ( Constraints.Any( x => !x.AcceptsItem( item, cell ) ) )
			return false;
		return true;
	}

	public bool AddWithNextAvailableSlot( Stashable item )
	{
		var slot = FirstAvailableSlot();
		if ( Add( item ) )
		{
			item.ItemData.StashSlot = slot;
			return true;
		}
		return false;
	}

	public bool Remove( Stashable item )
	{
		Host.AssertServer();

		if ( !Items.Contains( item ) ) 
			return false;

		GetPlayer()?.OnItemRemoved( this, item );

		Items.Remove( item );
		item.Parent = null;

		return true;
	}

	public void AddConstraint( StashConstraint constraint )
	{
		constraint.Stash = this;
		Constraints.Add( constraint );
	}

	private Player GetPlayer()
	{
		var parent = Parent;
		while( parent != null )
		{
			if ( parent is Player pl ) 
				return pl;
			parent = parent.Parent;
		}
		return null;
	}

	private bool SlotsOpen( int slot )
	{
		return !Items.Any( x => x.ItemData.StashSlot == slot );
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

	// note: if this shit fails I want it to fail in one spot, at the rpc

	[ConCmd.Server]
	public static void ServerCmd_DropItem( int networkIdent )
	{
		//todo: verify ownership

		var stashable = FindByIndex( networkIdent ) as Stashable;
		if ( !stashable.IsValid() ) 
			return;

		if ( stashable.Parent is not StashEntity stash ) 
			return;

		stash.Remove( stashable );
		stashable.ItemData.StashSlot = -1;
	}

	[ConCmd.Server]
	public static void ServerCmd_MoveItem( int stashIdent, int itemIdent, int slotIndex )
	{
		//todo: verify ownership

		var item = FindByIndex( itemIdent ) as Stashable;
		var toStash = FindByIndex( stashIdent ) as StashEntity;

		if ( !item.IsValid() || !toStash.IsValid() ) 
			return;

		if( slotIndex == -1 )
			slotIndex = toStash.FirstAvailableSlot();

		if ( !toStash.AcceptsItem( item, slotIndex ) )
			return;

		if ( !toStash.SlotsOpen( slotIndex ) ) 
			return;

		if ( toStash.Items.Contains( item ) )
		{
			item.ItemData.StashSlot = slotIndex;
			return;
		}

		if( item.Parent is StashEntity oldStash )
		{
			oldStash.Remove( item );
		}

		if( toStash.Add( item ) )
		{
			item.ItemData.StashSlot = slotIndex;
		}
		else
		{
			Log.Error( "Something got fucked" );
		}
	}

}
