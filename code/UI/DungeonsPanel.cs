
using Sandbox;
using Sandbox.UI;

namespace Dungeons.UI;

internal class DungeonsPanel : Panel
{

	protected virtual void OnDrag( MousePanelEvent e ) { }
	protected virtual void OnDragBegin( MousePanelEvent e ) { }
	protected virtual void OnDragEnd( MousePanelEvent e ) { }

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

}
