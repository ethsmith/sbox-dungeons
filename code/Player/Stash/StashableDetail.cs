﻿
using Dungeons.Data;
using Dungeons.Items;
using Dungeons.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Dungeons.Stash;

internal partial class StashableDetail : BaseNetworkable, INetworkSerializer
{

	private ItemData data = new();

	public string Identity
	{
		get => data.Identity;
		set
		{
			Host.AssertServer();

			data.Identity = value;
			WriteNetworkData();
		}
	}

	public int StashSlot
	{
		get => data.StashSlot;
		set
		{
			Host.AssertServer();

			data.StashSlot = value;
			WriteNetworkData();
		}
	}

	public int Quantity
	{
		get => data.Quantity;
		set
		{
			Host.AssertServer();

			data.Quantity = value;
			WriteNetworkData();
		}
	}

	public int Durability
	{
		get => data.Durability;
		set
		{
			Host.AssertServer();

			data.Durability = value;
			WriteNetworkData();
		}
	}

	public int Seed
	{
		get => data.Seed;
		set
		{
			Host.AssertServer();

			data.Seed = value;
			WriteNetworkData();
		}
	}

	public List<AffixData> Affixes
	{
		get => data.Affixes;
		set 
		{
			Host.AssertServer();

			data.Affixes = value;
			WriteNetworkData();
		}
	}

	public ItemRarity Rarity
	{
		get => data.Rarity;
		set
		{
			Host.AssertServer();

			data.Rarity = value;
			WriteNetworkData();
		}
	}

	public void Set( ItemData data )
	{
		Host.AssertServer();

		this.data = data ?? throw new System.Exception( "NULL ARGUMENT" );

		WriteNetworkData();
	}

	public void Read( ref NetRead read ) => data = read.ReadString().JsonDeserialize<ItemData>();
	public void Write( NetWrite write ) => write.Write( data.JsonSerialize() );
	public ItemData GetStorableObject() => data.JsonClone();

}
