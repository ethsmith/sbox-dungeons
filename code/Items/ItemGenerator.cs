
using Dungeons.Data;
using Sandbox;
using System;
using System.Linq;

namespace Dungeons.Items;

internal static class ItemGenerator
{

	public static ItemData Random()
	{
		var rnd = new System.Random();

		var allresources = ResourceLibrary.GetAll<ItemResource>().ToList();
		var rndname = allresources[rnd.Next( allresources.Count )];
		var result = new ItemData();
		result.Identity = rndname.ResourceName;
		result.Durability = rndname.Durability;
		result.Quantity = 1;
		result.Seed = rnd.Next( int.MaxValue );
		result.Rarity = RandomRarity( rnd );

		result.Affixes.Add( new() 
		{
			Identifier = "added-life",
			Seed = rnd.Next( int.MaxValue )
		} );

		return result;
	}

	private static ItemRarity[] RarityValues;
	private static ItemRarity RandomRarity( Random random )
	{
		RarityValues ??= Enum.GetValues<ItemRarity>();
		return RarityValues[random.Next( RarityValues.Length )];
	}

}
