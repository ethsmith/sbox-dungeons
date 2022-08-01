
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Dungeons.UI;

internal class Tippy : DungeonsPanel
{

	private Panel target;

	public Panel Canvas { get; set; }

	public override void Tick()
	{
		base.Tick();

		if( target == null
			|| target.Parent == null
			|| !target.HasHovered )
		{
			Delete();
		}
	}

	public Tippy WithMessage( string message ) => WithContent( Canvas.Add.Label( message ) );
	public Tippy WithContent( Panel content )
	{
		Canvas.AddChild( content );
		return this;
	}

	public static Tippy Create( Panel target, Pivot pivot )
	{
		if ( Local.Hud == null ) throw new System.Exception( "Hud null" );

		var result = new Tippy();
		result.Parent = Local.Hud;
		result.target = target;

		var scale = Local.Hud.ScaleFromScreen;
		var hudsize = Local.Hud.Box.Rect.Size;
		var r = target.Box.ClipRect;
		switch ( pivot )
		{
			case Pivot.TopRight:
				result.Style.Left = r.right * scale;
				result.Style.Top = r.top * scale;
				break;
			case Pivot.TopLeft:
				result.Style.Right = (hudsize.x - r.left) * scale;
				result.Style.Top = r.top * scale;
				break;
			case Pivot.BottomRight:
				result.Style.Left = r.right * scale;
				result.Style.Bottom = (hudsize.y - r.bottom) * scale;
				break;
			case Pivot.BottomLeft:
				result.Style.Right = (hudsize.x - r.left) * scale;
				result.Style.Bottom = (hudsize.y - r.bottom) * scale;
				break;
		}

		return result;
	}

	public enum Pivot
	{
		TopLeft,
		TopRight,
		BottomRight,
		BottomLeft
	}

}
