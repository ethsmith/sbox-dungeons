
using Dungeons.Attributes;
using Sandbox;
using System.Collections.Generic;

namespace Dungeons.Items;

[GameResource( "Item", "item", "An item definition" )]
internal class ItemResource : GameResource
{
	
	public string DisplayName { get; set; }
	[ResourceType( "image" )]
	public string Icon { get; set; }
	public ItemTypes ItemType { get; set; }
	public int Durability { get; set; }
	public List<ImplicitAffixData> Implicits { get; set; } = new();

}

internal struct ImplicitAffixData
{
	public StatTypes Stat { get; set; }
	public float Minimum { get; set; }
	public float Maximum { get; set; }
}
