
using Sandbox;

namespace Dungeons;

internal partial class NavigationEntity : Entity
{

	public static NavigationEntity Current;

	public int CellSize => 16;

	private Vector2 GridSize;
	private int[] Grid;

	public NavigationEntity()
	{
		Current = this;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[Event( "dungeon.postgen" )]
	public void Generate( DungeonEntity dungeon )
	{
		var bounds = dungeon.WorldRect;
		var gridx = (int)(bounds.width / CellSize);
		var gridy = (int)(bounds.height / CellSize);

		GridSize = new Vector2( gridx, gridy );
		Grid = new int[gridx * gridy];

		for(int x = 0; x < gridx; x++)
		for(int y = 0; y < gridy; y++)
			{
				var walkable = dungeon.IsPointWalkable( new Vector2( x, y ) * CellSize );
				Grid[GetIndex(x, y)] = walkable ? 1 : 0;
			}
	}

	private int GetIndex( int x, int y )
	{
		return x * (int)GridSize.y + y;
	}

	private Vector2 GetPosition( int index )
	{
		int x = (int)(index / GridSize.y);
		int y = (int)(index % GridSize.y);
		return new Vector2( x, y );
	}

}
