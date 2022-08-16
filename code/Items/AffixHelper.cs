
using Dungeons.Attributes;
using Dungeons.Data;
using Dungeons.Stash;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Items;

internal static class AffixHelper
{

	// todo: this still doesn't feel right

	public static IEnumerable<AffixValues> GetStats( this ItemDataNetworkable detail )
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

	public static AffixValues ToValue( this ImplicitStat data, int seed )
	{
		Rand.SetSeed( seed );

		return new AffixValues()
		{
			Stat = data.Stat,
			Modifier = StatModifiers.Flat,
			Amount = Rand.Float( data.Minimum, data.Maximum )
		};
	}

	public static AffixValues ToValue( this AffixData data )
	{
		var affix = ResourceLibrary.GetAll<AffixResource>()
			.Where( x => x.ResourceName == data.Identifier )
			.FirstOrDefault();

		if ( affix == null ) return default;
		if ( affix.Tiers.Count == 0 ) return default;

		var tier = Rand.Int( affix.Tiers.Count - 1 );
		var minroll = (float)affix.Tiers[data.Level].MinimumRoll;
		var maxroll = (float)affix.Tiers[data.Level].MaximumRoll;
		var amount = minroll.LerpTo( maxroll, data.Roll );

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

		public override string ToString()
		{
			return $"({Stat}, {Modifier}, {Amount})";
		}

		public string UserDescription()
		{
			var amount = (int)Math.Round( Amount );
			switch ( Modifier )
			{
				case StatModifiers.Flat:
					return $"+{amount} to {Stat}";
				case StatModifiers.Additive:
					return $"{amount}% increased {Stat}";
				case StatModifiers.Multiplicative:
					return $"{amount}% more {Stat}";
			}

			return this.ToString();
		}

	}

}
