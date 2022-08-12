
using Dungeons.UI;
using Sandbox;

namespace Dungeons;

internal partial class Player
{

	[Net, Predicted]
	public Entity HoveredEntity { get; set; }

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		inputBuilder.Position = 0;
		inputBuilder.Position.x = GetHoveredEntity()?.NetworkIdent ?? -1;
	}

	private void SimulateInput()
	{
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

	private Entity GetHoveredEntity()
	{
		var hoveredItem = ItemLabel.HoveredItem();
		if( hoveredItem.IsValid() )
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

		foreach( var tr in trs ) 
		{
			if ( tr.Entity.IsValid() )
				return tr.Entity;
		}

		return null;
	}

}
