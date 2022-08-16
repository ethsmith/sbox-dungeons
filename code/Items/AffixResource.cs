
using Dungeons.Attributes;
using Sandbox;
using System.Collections.Generic;

namespace Dungeons.Items;

[GameResource( "Affix", "affix", "An affix definition" )]
internal class AffixResource : GameResource
{

	public AffixTypes Type { get; set; }
	public StatTypes Stat { get; set; }
	public StatModifiers Modifier { get; set; }
	public List<AffixTier> Tiers { get; set; }

}

internal enum AffixTypes
{
	Prefix,
	Suffix,
	Unique
}

internal struct AffixTier
{
	public int ItemLevel { get; set; }
	public int MinimumRoll { get; set; }
	public int MaximumRoll { get; set; }
}
