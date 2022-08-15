
using Dungeons.Data;
using Dungeons.Items;
using Dungeons.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Dungeons.Stash;

internal partial class ItemDataNetworkable : BaseNetworkable, INetworkSerializer
{

	public ItemData Data { get; private set; } = new();

	public string Identity
	{
		get => Data.Identity;
		set
		{
			Host.AssertServer();

			Data.Identity = value;
			WriteNetworkData();
		}
	}

	public int StashSlot
	{
		get => Data.StashSlot;
		set
		{
			Host.AssertServer();

			Data.StashSlot = value;
			WriteNetworkData();
		}
	}

	public int Quantity
	{
		get => Data.Quantity;
		set
		{
			Host.AssertServer();

			Data.Quantity = value;
			WriteNetworkData();
		}
	}

	public int Durability
	{
		get => Data.Durability;
		set
		{
			Host.AssertServer();

			Data.Durability = value;
			WriteNetworkData();
		}
	}

	public int Seed
	{
		get => Data.Seed;
		set
		{
			Host.AssertServer();

			Data.Seed = value;
			WriteNetworkData();
		}
	}

	public List<AffixData> Affixes
	{
		get => Data.Affixes;
		set 
		{
			Host.AssertServer();

			Data.Affixes = value;
			WriteNetworkData();
		}
	}

	public ItemRarity Rarity
	{
		get => Data.Rarity;
		set
		{
			Host.AssertServer();

			Data.Rarity = value;
			WriteNetworkData();
		}
	}

	public void Set( ItemData data )
	{
		Host.AssertServer();

		Data = data ?? throw new System.Exception( "NULL ARGUMENT" );

		WriteNetworkData();
	}

	public void Read( ref NetRead read ) => Data = read.ReadString().JsonDeserialize<ItemData>();
	public void Write( NetWrite write ) => write.Write( Data.JsonSerialize() );
	public ItemData GetStorableObject() => Data.JsonClone();

}
