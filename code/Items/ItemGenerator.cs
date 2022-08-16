
using Dungeons.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Items;

internal static class ItemGenerator
{

	public static ItemData Random()
	{
		var result = new ItemData();
		var rnd = new System.Random();
		var itemtype = RandomItem( rnd );

		result.Identity = itemtype.ResourceName;
		result.Durability = itemtype.Durability;
		result.Seed = rnd.Next( int.MaxValue );
		result.Quantity = 1;
		result.Level = rnd.Next( 1, 84 );
		result.Rarity = RandomRarity( rnd );
		result.Affixes = RandomAffixes( itemtype, result.Level, result.Rarity, rnd );

		return result;
	}

	private static ItemRarity[] RarityValues;
	private static ItemRarity RandomRarity( Random random )
	{
		RarityValues ??= Enum.GetValues<ItemRarity>();
		return RarityValues[random.Next( RarityValues.Length )];
	}

	private static List<AffixData> RandomAffixes( ItemResource item, int itemLevel, ItemRarity rarity, Random random )
	{
		var result = new List<AffixData>();

		if ( rarity == ItemRarity.Normal || rarity == ItemRarity.Unique )
			return result;

		var pool = ResourceLibrary.GetAll<AffixResource>().ToList();
		var prefixcount = 0;
		var suffixcount = 0;

		// at least 1 affix
		if( rarity == ItemRarity.Magic )
		{
			prefixcount = random.Next( 0, 2 );
			suffixcount = random.Next( 0, 2 );

			if( prefixcount == 0 && suffixcount == 0 )
			{
				if( random.Next(0, 2) == 0 )
				{
					prefixcount = 1;
				}
				else
				{
					suffixcount = 1;
				}
			}
		}

		// 1-3 prefixes, 1-3 suffixes, no less than 3 total
		if ( rarity == ItemRarity.Rare )
		{
			prefixcount = random.Next( 1, 4 );
			suffixcount = random.Next( 1, 4 );

			if( prefixcount + suffixcount < 3 )
			{
				if ( random.Next( 0, 2 ) == 0 )
				{
					prefixcount++;
				}
				else
				{
					suffixcount++;
				}
			}
		}

		var prefixes = pool.Where( x => x.Type == AffixTypes.Prefix ).OrderBy( x => random.Next( 999 ) ).Take( prefixcount );
		var suffixes = pool.Where( x => x.Type == AffixTypes.Suffix ).OrderBy( x => random.Next( 999 ) ).Take( prefixcount );

		foreach( var affix in prefixes.Concat( suffixes ) )
		{
			var level = affix.Tiers.Where( x => x.ItemLevel <= itemLevel )
				.OrderBy( x => random.Next(999) )
				.FirstOrDefault();
			var roll = random.NextSingle();

			result.Add( new AffixData()
			{
				Identifier = affix.ResourceName,
				Level = level.ItemLevel,
				Roll = roll
			} );
		}

		return result;
	}

	private static ItemResource RandomItem( Random random )
	{
		var allresources = ResourceLibrary.GetAll<ItemResource>().ToList();
		return allresources[random.Next( allresources.Count )];
	}

}
