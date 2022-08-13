
using Sandbox;

namespace Dungeons.Items;

[GameResource( "Item", "item", "An item definition" )]
internal class ItemResource : GameResource
{
	
	public string DisplayName { get; set; }
	[ResourceType( "image" )]
	public string Icon { get; set; }
	public ItemTypes ItemType { get; set; }
	public int Durability { get; set; }

}
