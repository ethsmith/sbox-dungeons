
using Dungeons.Data;
using Sandbox;

namespace Dungeons.Stash;

internal partial class Stashable : Entity
{

	[Net]
	public ItemDataNetworkable ItemData { get; set; }

	public Stashable()
	{
		ItemData = new();
		Transmit = TransmitType.Always;
	}

	public Stashable( ItemData data ) : this()
	{
		ItemData.Set( data );
	}

}
