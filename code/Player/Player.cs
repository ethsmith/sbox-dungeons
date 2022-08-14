
using Dungeons.Attributes;
using Dungeons.Data;
using Dungeons.Items;
using Dungeons.Stash;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons;

internal partial class Player : AnimatedEntity
{

	public PlayerController Controller
	{
		get => Components.Get<PlayerController>();
		set => Components.Add( value );
	}

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public PlayerAnimator Animator
	{
		get => Components.Get<PlayerAnimator>();
		set => Components.Add( value );
	}

	public NavigationAgent Agent
	{
		get => Components.Get<NavigationAgent>();
		set => Components.Add( value );
	}

	[Net]
	public StatSystem Stats { get; set; }

	[Net]
	public StashEntity Stash { get; set; }

	[Net]
	public StashEntity Stash2 { get; set; }

	[Net]
	public StashEntity StashEquipment { get; set; }

	[Net]
	public SpotLightEntity LightRadius { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Controller = new PlayerController();
		Camera = new PlayerCamera();
		Animator = new PlayerAnimator();
		Agent = new NavigationAgent();
		PhysicsEnabled = true;
		EnableAllCollisions = true;

		LightRadius = new();
		LightRadius.SetParent( this );
		LightRadius.LocalPosition = Vector3.Up * 100f;
		LightRadius.Rotation = Rotation.FromPitch( 85 );
		LightRadius.DynamicShadows = true;
		LightRadius.Range = 250f;
		LightRadius.Color = Color.White.Darken( .9f );

		Stash = new();
		Stash.Parent = this;
		Stash.Owner = this;
		Stash.LocalPosition = 0;
		Stash.SlotCount = 40;
		Stash.AddConstraint( new OccupiedConstraint() );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );
		Stash.AddWithNextAvailableSlot( new Stashable( ItemGenerator.Random() ) );

		StashEquipment = new();
		StashEquipment.Parent = this;
		StashEquipment.Owner = this;
		StashEquipment.LocalPosition = 0;
		StashEquipment.SlotCount = 8;
		StashEquipment.AddConstraint( new ItemTypeConstraint() );
		StashEquipment.AddConstraint( new OccupiedConstraint() );

		Stats = new();
		Stats.Add( StatTypes.Life, StatModifiers.Flat, 55 );
		Stats.Add( StatTypes.Life, StatModifiers.Additive, 20 );

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 64 ) );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Stash.AddConstraint( new OccupiedConstraint() );
		StashEquipment.AddConstraint( new ItemTypeConstraint() );
		StashEquipment.AddConstraint( new OccupiedConstraint() );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//Controller.Simulate();
		Animator.Simulate();
		Agent.Simulate();

		HoveredEntity?.SetGlow( false );
		HoveredEntity = FindByIndex( (int)Input.Position.x );
		HoveredEntity?.SetGlow( true, Color.Red );

		SimulateInput();
	}

	public void OnItemAdded( StashEntity stash, Stashable item )
	{
		Host.AssertServer();

		if ( stash == StashEquipment )
		{
			AddItemAffixes( item );
		}
	}

	public void OnItemRemoved( StashEntity stash, Stashable item )
	{
		Host.AssertServer();

		if( stash == StashEquipment )
		{
			RemoveItemAffixes( item );
		}
	}

	private Dictionary<Stashable, List<int>> EquippedAffixes = new();
	private void AddItemAffixes( Stashable item ) 
	{
		Host.AssertServer();

		foreach( var affixData in item.Detail.Affixes )
		{
			if( AddAffix( affixData, out int statId ) )
			{
				if ( !EquippedAffixes.ContainsKey( item ) )
					EquippedAffixes.Add( item, new() );
				EquippedAffixes[item].Add( statId );
			}
		}
	}

	private void RemoveItemAffixes( Stashable item )
	{
		Host.AssertServer();

		if ( !EquippedAffixes.ContainsKey( item ) )
			return;

		foreach( var affix in EquippedAffixes[item] )
		{
			Stats.Remove( affix );
		}

		EquippedAffixes.Remove( item );
	}

	private bool AddAffix( AffixData affixData, out int statId )
	{
		statId = -1;

		var affix = ResourceLibrary.GetAll<AffixResource>()
			.Where( x => x.ResourceName == affixData.Identifier )
			.FirstOrDefault();

		if ( affix == null ) 
			return false;

		if ( affix.Tiers.Count == 0 )
			return false;

		var stat = affix.Stat;
		var modifier = affix.Modifier;
		Rand.SetSeed( affixData.Seed );
		var tier = Rand.Int( affix.Tiers.Count );
		var amount = Rand.Float( affix.Tiers[tier].MinimumRoll, affix.Tiers[tier].MaximumRoll );

		statId = Stats.Add( stat, modifier, amount );
		return true;
	}

}
