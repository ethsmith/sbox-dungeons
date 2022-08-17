
using Sandbox.UI;

namespace Dungeons.UI;

internal class CornerMinimap : Panel
{

	public Minimap Minimap { get; set; }

	public CornerMinimap() 
	{
		var inner = AddChild<Panel>( "inner" );
		Minimap = inner.AddChild<Minimap>();
	}

}
