﻿
using Dungeons.Data;
using Sandbox;
using System.Linq;

namespace Dungeons.Items;

internal static class ItemGenerator
{

	public static StashableDetailData Random()
	{
		var allresources = ResourceLibrary.GetAll<ItemResource>().ToList();
		var rndname = allresources[Rand.Int( allresources.Count - 1 )];
		var result = new StashableDetailData();
		result.Identity = rndname.ResourceName;
		result.Durability = rndname.Durability;
		result.Quantity = 1;
		result.Seed = Rand.Int( int.MaxValue - 1 );

		result.Affixes.Add( new() 
		{
			Identifier = "added-life",
			Seed = 1
		} );

		return result;
	}

}
