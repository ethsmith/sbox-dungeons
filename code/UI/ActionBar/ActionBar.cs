
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Dungeons.UI;

[UseTemplate]
internal class ActionBar : DungeonsPanel
{

	protected override DisplayModes DisplayMode => DisplayModes.Always;
	protected override CursorModes CursorMode => CursorModes.None;

	public void ToggleInventory()
	{
		Local.Hud.ChildrenOfType<Inventory>().FirstOrDefault()?.Toggle();
	}

}
