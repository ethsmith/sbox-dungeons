
using Dungeons.Attributes;
using Dungeons.Data;
using Dungeons.Stash;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Items;

internal static class AffixHelper
{

	// todo: this still doesn't feel right

	public static IEnumerable<AffixValues> GetStats( this StashableDetail detail )
	{
		var result = new List<AffixValues>();

		var item = ResourceLibrary.GetAll<ItemResource>()
			.Where( x => x.ResourceName == detail.Identity )
			.FirstOrDefault();

		if ( item == null ) 
			return result;

		result.AddRange( item.Implicits.Select( x => x.ToValue( detail.Seed ) ) );
		result.AddRange( detail.Affixes.Select( x => x.ToValue() ) );

		return result;
	}

	private static AffixValues ToValue( this ImplicitStat data, int seed )
	{
		Rand.SetSeed( seed );

		return new AffixValues()
		{
			Stat = data.Stat,
			Modifier = StatModifiers.Flat,
			Amount = Rand.Float( data.Minimum, data.Maximum )
		};
	}

	private static AffixValues ToValue( this AffixData data )
	{
		var affix = ResourceLibrary.GetAll<AffixResource>()
			.Where( x => x.ResourceName == data.Identifier )
			.FirstOrDefault();

		if ( affix == null ) return default;
		if ( affix.Tiers.Count == 0 ) return default;

		Rand.SetSeed( data.Seed );
		var tier = Rand.Int( affix.Tiers.Count );
		var amount = Rand.Float( affix.Tiers[tier].MinimumRoll, affix.Tiers[tier].MaximumRoll );

		return new AffixValues()
		{
			Stat = affix.Stat,
			Modifier = affix.Modifier,
			Amount = amount
		};
	}

	internal struct AffixValues
	{
		public StatTypes Stat;
		public StatModifiers Modifier;
		public float Amount;
	}

}
