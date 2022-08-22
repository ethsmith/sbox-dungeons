
using Dungeons.Stash;
using Dungeons.UI;
using Dungeons.Attributes;
using Sandbox;
using System;
using System.Threading;

namespace Dungeons;

internal partial class Player
{

	[Net, Predicted]
	public Entity HoveredEntity { get; set; }

	private CancellationTokenSource ItemPickup = new();
	private bool SkipInputUntilMouseRelease;

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		inputBuilder.Position = 0;
		inputBuilder.Position.x = GetHoveredEntity()?.NetworkIdent ?? -1;
	}

	private void SimulateInput()
	{
		if ( !Input.Down( InputButton.PrimaryAttack ) )
			SkipInputUntilMouseRelease = false;

		if ( SkipInputUntilMouseRelease )
			return;

		ItemPickup?.Cancel();
		ItemPickup = null;

		if ( HoveredEntity is Stashable item && Input.Pressed( InputButton.PrimaryAttack ) )
		{
			SkipInputUntilMouseRelease = true;
			Agent.SetDestination( item.Position );
			ItemPickup = new( TimeSpan.FromSeconds( 10 ) );

			if ( IsServer )
			{
				PickupItemAsync( item, ItemPickup.Token );
			}
			return;
		}

		var start = Input.Cursor.Origin;
		var end = Input.Cursor.Origin + Input.Cursor.Direction * 5000f;
		var tr = Trace.Ray( start, end )
			.WorldOnly()
			.Run();

		if ( tr.Hit && Input.Down( InputButton.PrimaryAttack ) )
		{
			Agent.SetDestination( tr.HitPosition );
		}
	}

	private async void PickupItemAsync( Stashable item, CancellationToken token )
	{
		while ( true )
		{
			if ( token.IsCancellationRequested ) break;
			if ( !item.IsValid() || item.Parent.IsValid() ) break;

			if ( item.Position.Distance( Position ) > 16 )
			{
				await Task.Delay( 10 );
				continue;
			}

			Stash.AddWithNextAvailableSlot( item );
			break;
		}
	}

	private Entity GetHoveredEntity()
	{
		var hoveredItem = ItemLabel.HoveredItem();
		if ( hoveredItem.IsValid() )
		{
			return hoveredItem;
		}

		var start = Input.Cursor.Origin;
		var end = Input.Cursor.Origin + Input.Cursor.Direction * 5000f;
		var trs = Trace.Ray( start, end )
			.EntitiesOnly()
			.RunAll();

		if ( trs == null )
			return null;

		foreach ( var tr in trs )
		{
			if ( tr.Entity.IsValid() )
				return tr.Entity;
		}

		return null;
	}

}
