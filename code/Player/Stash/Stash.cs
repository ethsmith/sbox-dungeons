using Sandbox;
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
		Host.AssertServer();

		if ( !item.IsValid() ) return false;
		if ( items.Contains( item ) ) return false;
		items.Add( item );
		item.Parent = this;

		// todo: set in first open slot

		return true;
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

}
