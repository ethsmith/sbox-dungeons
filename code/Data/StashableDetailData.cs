
using System.Collections.Generic;

namespace Dungeons.Data;

internal class StashableDetailData
{

	public string Identity { get; set; }
	public int StashSlot { get; set; }
	public int Quantity { get; set; }
	public int Durability { get; set; }
	public int Seed { get; set; }
	public List<AffixData> Affixes { get; set; } = new();

}

internal struct AffixData
{
	public string Identifier { get; set; }
	public int Seed { get; set; }
}
