using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Stash;

internal partial class StashEntity : Entity
{

	[Net]
	public int SlotCount { get; set; } = 50;
	[Net]
	private List<Stashable> items { get; set; } = new();
	public IEnumerable<Stashable> Items => items.AsEnumerable();

	public StashEntity()
	{
		Transmit = TransmitType.Always;
	}

	public bool Add( Stashable item )
	{
		if ( IsClient )
		{
			AddItemToStash( NetworkIdent, item.NetworkIdent );

			return true;
		}

		if ( !item.IsValid() ) return false;
		if ( items.Contains( item ) ) return false;
		items.Add( item );
		item.Parent = this;
		item.LocalPosition = 0;
		item.Detail.StashSlot = 0;

		// todo: set in first open slot

		return true;
	}

	public override int GetHashCode()
	{
		var result = SlotCount;
		foreach ( var item in Items )
		{
			result = HashCode.Combine( result, item.NetworkIdent );
		}
		return result;
	}

	public bool Remove( Stashable item )
	{
		Host.AssertServer();

		if ( !item.IsValid() ) return false;
		if ( !items.Contains( item ) ) return false;
		items.Remove( item );
		return true;
	}

	public void Clear()
	{
		Host.AssertServer();

		foreach( var item in items ) item.Delete();

		items.Clear();
	}

	[ConCmd.Server]
	public static void AddItemToStash( int stashIdent, int itemIdent )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller == null ) return;
		//todo: verify ownership

		var stashable = Entity.FindByIndex( itemIdent ) as Stashable;
		if ( !stashable.IsValid() ) return;

		var targetStash = Entity.FindByIndex( stashIdent ) as StashEntity;
		if ( !targetStash.IsValid() ) return;

		var previousStash = stashable.Parent as StashEntity;
		if ( targetStash == previousStash ) return;

		if( targetStash.Add( stashable ) )
		{
			if ( previousStash.IsValid() )
			{
				previousStash.Remove( stashable );
			}
		}
	}

}
