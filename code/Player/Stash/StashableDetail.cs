
using Dungeons.Data;
using Dungeons.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Dungeons.Stash;

internal partial class StashableDetail : BaseNetworkable, INetworkSerializer
{

	private StashableDetailData data = new();

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

	public void Set( StashableDetailData data )
	{
		Host.AssertServer();

		this.data = data ?? throw new System.Exception( "NULL ARGUMENT" );

		WriteNetworkData();
	}

	public void Read( ref NetRead read ) => data = read.ReadString().JsonDeserialize<StashableDetailData>();
	public void Write( NetWrite write ) => write.Write( data.JsonSerialize() );
	public StashableDetailData GetStorableObject() => data.JsonClone();

}
