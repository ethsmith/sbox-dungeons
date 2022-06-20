
using System.Collections.Generic;
using System.Linq;

namespace Dungeons;

internal class DungeonRoute
{

	public readonly Dungeon Dungeon;
	public readonly DungeonEdge Edge;
	public List<DungeonCell> Route;

	public DungeonRoute( Dungeon dungeon, DungeonEdge edge )
	{
		Dungeon = dungeon;
		Edge = edge;
		Route = new();
	}

	public void Calculate()
	{
		Route = new List<DungeonCell>();
		var unexplored = new List<DungeonCell>( Dungeon.Cells );

		foreach ( var c in unexplored )
		{
			c.Distance = float.PositiveInfinity;
			c.Parent = null;
		}

		Edge.A.Distance = 0;

		while ( unexplored.Count > 0 )
		{
			var current = unexplored.OrderBy( x => x.Distance ).First();
			unexplored.Remove( current );

			if ( current == Edge.B )
			{
				while ( current != null )
				{
					Route.Add( current );
					current = current.Parent;
				}
				break;
			}

			var neighbors = Dungeon.NeighborsOf( current );
			foreach ( var n in neighbors )
			{
				var dist = current.Distance + n.Rect.Position.Distance( current.Rect.Position );

				if ( !unexplored.Contains( n ) ) continue;
				if ( dist >= n.Distance ) continue;

				n.Distance = dist;
				n.Parent = current;
			}
		}

		Route.Reverse();
	}

}
