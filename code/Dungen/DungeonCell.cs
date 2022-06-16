
using Sandbox;

namespace Dungeons;

internal class DungeonCell
{

	//public bool Void;
	public Rect Rect;
	public DungeonNode Node;

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
