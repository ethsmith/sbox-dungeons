
using Dungeons.Data;
using Dungeons.Items;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Dungeons.UI;

[UseTemplate]
internal class ItemTooltip : Panel
{

	public ItemData Item { get; }
	public ItemResource Resource { get; }
	public Label ItemName { get; protected set; }
	public Panel AffixCanvas { get; protected set; }
	public Panel ImplicitCanvas { get; protected set; }

	public ItemTooltip( ItemData item )
	{
		Item = item;
		Resource = ResourceLibrary.GetAll<ItemResource>().FirstOrDefault( x => x.ResourceName == Item.Identity );

		AddClass( item.Rarity.ToString().ToLower() );

		ItemName.Text = Resource.DisplayName + $" (i{item.Level})";

		BuildAffixes();
	}

	private void BuildAffixes()
	{
		AffixCanvas.DeleteChildren( true );
		ImplicitCanvas.DeleteChildren( true );

		foreach ( var impl in Resource.Implicits )
		{
			var desc = impl.ToValue( Item.Seed ).UserDescription();
			ImplicitCanvas.Add.Label( desc );
		}

		foreach ( var affix in Item.Affixes )
		{
			var desc = affix.ToValue().UserDescription();
			AffixCanvas.Add.Label( desc + $" (i{affix.Tier})" );
		}
	}

}
