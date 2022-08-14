
using Dungeons.Attributes;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Dungeons.UI;

[UseTemplate]
internal class StatsPanel : Panel
{

	private static StatTypes[] EnumCache;
	private int ActiveHash;

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player pl )
			return;

		EnumCache ??= Enum.GetValues<StatTypes>();

		var newhash = 0;
		var system = pl.Stats;

		foreach ( var attr in EnumCache )
		{
			var total = system.Calculate( attr );
			newhash = HashCode.Combine( newhash, total.GetHashCode() );
		}

		if( newhash != ActiveHash )
		{
			ActiveHash = newhash;
			Rebuild( system );
		}
	}

	private void Rebuild( StatSystem system )
	{
		DeleteChildren( true );

		foreach ( var attr in EnumCache )
		{
			var total = system.Calculate( attr );
			Add.Label( $"{attr}: {total}" );
		}
	}

}
