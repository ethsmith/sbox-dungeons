
using Dungeons.Data;

namespace Dungeons;

internal static class ItemGenerator
{

	public static StashableDetailData Random()
	{
		var result = new StashableDetailData();
		result.Identity = "wood-shield";
		result.Durability = 64;
		result.Quantity = 1;

		return result;
	}

}
