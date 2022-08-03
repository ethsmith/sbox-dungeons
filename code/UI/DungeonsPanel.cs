
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.UI;

internal class DungeonsPanel : Panel
{

	private static List<DungeonsPanel> All = new();
	public static bool InputBlocked()
	{
		return All.Any( x => x.BlockingInput() );
	}

	protected virtual CursorModes CursorMode { get; }
	protected virtual DisplayModes DisplayMode { get; }
	protected virtual InputButton ToggleButton { get; }
	protected virtual void OnDrag( MousePanelEvent e ) { }
	protected virtual void OnDragBegin( MousePanelEvent e ) { }
	protected virtual void OnDragEnd( MousePanelEvent e ) { }

	public DungeonsPanel()
	{
		All.Add( this );

		AddClass( "dungeons-panel" );
	}

	public override void Delete( bool immediate = false )
	{
		base.Delete( immediate );

		All.Remove( this );
	}

	private bool wantsdrag;
	private bool dragging;
	private Vector2 dragstart;
	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		wantsdrag = true;
		dragstart = e.LocalPosition;
	}

	protected override void OnMouseMove( MousePanelEvent e )
	{
		base.OnMouseMove( e );

		if ( dragging )
		{
			OnDrag( e );
			return;
		}

		if ( !wantsdrag ) return;
		if ( e.LocalPosition.Distance( dragstart ) > 10 ) return;

		OnDragBegin( e );

		dragging = true;
		wantsdrag = false;
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		if ( dragging )
		{
			OnDragEnd( e );
		}

		dragging = false;
		wantsdrag = false;
	}

	private bool BlockingInput()
	{
		if( CursorMode == CursorModes.Always )
			return true;

		if ( CursorMode == CursorModes.Hover && HasHovered )
			return true;

		return false;
	}

	[Event.Frame]
	public void OnFrame()
	{
		SetClass( "display-toggle", DisplayMode == DisplayModes.Toggle );
		SetClass( "display-always", DisplayMode == DisplayModes.Always );

		if ( DisplayMode != DisplayModes.Toggle )
			return;

		if( !Input.Pressed( ToggleButton ) )
			return;

		SetClass( "open", !HasClass( "open" ) );
	}

}
