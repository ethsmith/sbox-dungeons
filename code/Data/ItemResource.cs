
using Sandbox;

namespace Dungeons.Data;

[GameResource( "Item", "item", "An item definition" )]
internal class ItemResource : GameResource
{
	
	public string DisplayName { get; set; }
	[ResourceType( "image" )]
	public string Icon { get; set; }
	public int Durability { get; set; }

}
