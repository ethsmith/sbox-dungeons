
using Dungeons.Data;
using Sandbox;
using System.Linq;

namespace Dungeons;

internal static class ItemGenerator
{

	public static StashableDetailData Random()
	{
		var allresources = ResourceLibrary.GetAll<ItemResource>().ToList();
		var rndname = Rand.FromList( allresources );
		var result = new StashableDetailData();
		result.Identity = rndname.ResourceName;
		result.Durability = rndname.Durability;
		result.Quantity = 1;

		return result;
	}

}
