
using Sandbox;

namespace Dungeons;

internal class NavigationAgent : EntityComponent, ISingletonComponent
{

	public int MoveSpeed { get; set; } = 135;

	Vector3[] PathArray = new Vector3[24];
	int TotalWaypoints;
	int CurrentWaypoint;

	public void SetDestination( Vector3 position )
	{
		CurrentWaypoint = 1;
		TotalWaypoints = NavigationEntity.Current.CalculatePath( Entity.Position, position, PathArray );
	}

	public void Simulate()
	{
		if ( CurrentWaypoint >= TotalWaypoints )
		{
			Entity.Velocity = 0;
			return;
		}

		var movedir = (PathArray[CurrentWaypoint] - Entity.Position).WithZ( 0 ).Normal;
		var movevec = movedir * MoveSpeed;
		var distance = Vector3.DistanceBetween( Entity.Position, PathArray[CurrentWaypoint] );
		if( distance <= NavigationEntity.Current.CellSize / 2 )
		{
			CurrentWaypoint++;
			return;
		}

		Entity.Velocity = movevec;
		Entity.Position += movevec * Time.Delta;
		Entity.Rotation = Rotation.Slerp( Entity.Rotation, Rotation.LookAt( movedir ), 8f * Time.Delta );
	}

}
