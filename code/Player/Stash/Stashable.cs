
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

}
