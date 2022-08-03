
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

[UseTemplate]
internal class Hud : RootPanel
{

	public Hud()
	{
		Local.Hud = this;
	}

}
