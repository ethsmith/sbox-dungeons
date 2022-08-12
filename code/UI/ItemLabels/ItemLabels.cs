
using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

internal class ItemLabels : Panel
{

	public ItemLabels()
	{
		Style.Position = PositionMode.Absolute;
		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );
		Style.Top = 0;
		Style.Left = 0;
	}

	public override void Tick()
	{
		base.Tick();

		foreach(var ent in Entity.All )
		{
			if ( ent is not Stashable s ) 
				continue;

			if ( s.Parent != null ) 
				continue;

			if ( Local.Pawn.Position.Distance( s.Position ) > 600f )
				continue;

			if ( !NeedsLabel( s ) )
				continue;

			AddChild( new ItemLabel( s ) );
		}
	}

	private bool NeedsLabel( Stashable item )
	{
		foreach( var child in Children )
		{
			if ( child is not ItemLabel l ) continue;
			if ( l.Item == item ) return false;
		}

		return true;
	}

}
