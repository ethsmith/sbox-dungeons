
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Attributes;

internal partial class StatSystem : BaseNetworkable
{

	[Net]
	public IList<AttributeEntry> Entries { get; set; }

	public int Add( StatTypes attribute, StatModifiers modifier, float amount )
	{
		Host.AssertServer();

		var entry = new AttributeEntry( modifier, attribute, amount );

		Entries.Add( entry );

		return entry.Id;
	}

	public bool Remove( int id )
	{
		Host.AssertServer();

		return Entries.RemoveAll( x => x.Id == id ) == 1;
	}

	public float Calculate( StatTypes attribute )
	{
		var all = Entries.Where( x => x.Attribute == attribute );
		var flatSum = all.Where( x => x.Modifier == StatModifiers.Flat ).Sum( x => x.Amount );
		var multSum = all.Where( x => x.Modifier == StatModifiers.Multiplicative ).Sum( x => x.Amount );

		var result = flatSum;

		foreach ( var pre in all )
		{
			if ( pre.Modifier != StatModifiers.Additive )
				continue;

			result += flatSum * (pre.Amount / 100f);
		}

		result += result * (multSum / 100f);

		return result;
	}

}

internal struct AttributeEntry
{

	public readonly int Id;
	public readonly StatModifiers Modifier;
	public readonly StatTypes Attribute;
	public readonly float Amount;

	private static int IdAccumulator;

	public AttributeEntry( StatModifiers modifier, StatTypes type, float amount )
	{
		Id = ++IdAccumulator;
		Modifier = modifier;
		Attribute = type;
		Amount = amount;
	}

}
