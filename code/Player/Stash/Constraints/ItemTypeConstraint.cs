
using Dungeons.Items;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Stash;

internal class ItemTypeConstraint : StashConstraint
{

	private static Dictionary<ItemTypes, List<int>> SlotMap = new()
	{
		{ ItemTypes.Helmet, new() { 0 } },
		{ ItemTypes.BodyArmour, new() { 1 } },
		{ ItemTypes.Gloves, new() { 2 } },
		{ ItemTypes.Boots, new() { 3 } },
		{ ItemTypes.Belt, new() { 4 } },
		{ ItemTypes.Ring, new() { 5, 6 } },
		{ ItemTypes.Amulet, new() { 7 } },
		{ ItemTypes.MainHand, new() { 8 } },
		{ ItemTypes.OffHand, new() { 9 } },
	};

	public override bool AcceptsItem( Stashable item, int cell )
	{
		var resource = ResourceLibrary.GetAll<ItemResource>()
			.Where( x => x.ResourceName == item.ItemData.Identity )
			.FirstOrDefault();

		if ( resource == null ) 
			return false;

		if ( !SlotMap.ContainsKey( resource.ItemType ) )
			return false;

		if ( !SlotMap[resource.ItemType].Contains( cell ) )
			return false;

		return true;
	}

}
