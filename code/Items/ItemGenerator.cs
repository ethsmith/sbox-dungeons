
using Dungeons.Data;
using Sandbox;
using System.Linq;

namespace Dungeons.Items;

internal static class ItemGenerator
{

	public static StashableDetailData Random()
	{
		var rnd = new System.Random();

		var allresources = ResourceLibrary.GetAll<ItemResource>().ToList();
		var rndname = allresources[rnd.Next( allresources.Count )];
		var result = new StashableDetailData();
		result.Identity = rndname.ResourceName;
		result.Durability = rndname.Durability;
		result.Quantity = 1;
		result.Seed = rnd.Next( int.MaxValue );

		result.Affixes.Add( new() 
		{
			Identifier = "added-life",
			Seed = rnd.Next( int.MaxValue )
		} );

		return result;
	}

}
