
using Dungeons.Attributes;
using Sandbox;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dungeons.Items;

[GameResource( "Item", "item", "An item definition" )]
internal class ItemResource : GameResource
{

	public string DisplayName { get; set; }
	[ResourceType( "image" )]
	public string Icon { get; set; }
	[Description("The monster level required for this item to drop, also the level required for players to equip it.")]
	public int ItemLevel { get; set; }
	public ItemTypes ItemType { get; set; }
	public int Durability { get; set; }
	public List<ImplicitStat> Implicits { get; set; } = new();

}

internal struct ImplicitStat
{
	public StatTypes Stat { get; set; }
	public float Minimum { get; set; }
	public float Maximum { get; set; }
}
