
using Sandbox.UI;

namespace Dungeons.UI;

internal class TooltipButton : Button
{

	private string Tooltip;

	public override void SetProperty( string name, string value )
	{
		if(name != "tooltip")
		{
			base.SetProperty( name, value );
			return;
		}

		Tooltip = value;
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		if ( string.IsNullOrEmpty( Tooltip ) ) 
			return;

		Tippy.Create( this, Tippy.Pivots.TopLeft )
			.WithMessage( Tooltip );
	}

}
