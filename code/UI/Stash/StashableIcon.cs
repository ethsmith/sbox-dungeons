using Dungeons.Stash;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Dungeons.UI;

internal class StashableIcon : Panel
{

	public Stashable Stashable { get; private set; }

	private TimeSince timeSinceMouseDown;
	private bool wantsLongPress;

	public StashableIcon( Stashable stashable )
	{
		Stashable = stashable;

		Add.Label( $"#{stashable.NetworkIdent}, #{stashable.Detail.StashSlot}" );
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		timeSinceMouseDown = 0;
		wantsLongPress = true;
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		wantsLongPress = false;
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		if ( HasClass( "dragging" ) ) return;

		Tippy.Create( this, Tippy.Pivot.TopRight )
			.WithMessage( @$"Item #{Stashable.NetworkIdent}
Durability: {Stashable.Detail.Durability}
Quantity: {Stashable.Detail.Quantity}" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Stashable.IsValid() )
		{
			Delete();
			return;
		}

		if ( !HasHovered )
		{
			wantsLongPress = false;
			return;
		}

		if( wantsLongPress && timeSinceMouseDown > 0.05f )
		{
			CreateEvent( "stashable.onpress", Stashable, -RealTime.Now + timeSinceMouseDown );
		}
	}

}
