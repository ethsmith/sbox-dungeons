
namespace Dungeons;

internal class DungeonCell
{

	public Rect Rect;
	public DungeonNode Node;

	// Pathfinding parameters (prolly separate this later)
	public DungeonCell Parent;
	public float Distance;

	public DungeonCell( Rect rect )
	{
		Rect = rect;
	}

	public T SetNode<T>( string name )
		where T : DungeonNode, new()
	{
		Node = new T();
		Node.Cell = this;
		Node.Name = name;
		return Node as T;
	}

}
