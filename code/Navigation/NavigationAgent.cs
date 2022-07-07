
using Sandbox;

namespace Dungeons;

internal class NavigationAgent : EntityComponent, ISingletonComponent
{

	public int MoveSpeed { get; set; } = 135;
	public Vector3[] Waypoints { get; } = new Vector3[24];
	public int TotalWaypoints { get; private set; }
	public int CurrentWaypoint { get; private set; }
	public Vector3 Destination { get; private set; }
	public bool Persistent => true;

	private int AgentId;

	public void Stop()
	{
		CurrentWaypoint = 0;
		TotalWaypoints = 0;
	}

	public void SetDestination( Vector3 position )
	{
		CurrentWaypoint = 1;
		TotalWaypoints = NavigationEntity.Current.CalculatePath( Entity.Position, position, Waypoints, AgentId );
		Destination = position;
	}

	public void Simulate()
	{
		if ( !Entity.IsValid() ) 
			return;

		if ( Entity.IsClient && !Entity.Predictable )
			return;

		NavigationEntity.Current.UpdateAgent( AgentId, Entity.Position );

		if ( CurrentWaypoint >= TotalWaypoints )
		{
			Entity.Velocity = 0;
			return;
		}

		if ( Persistent )
		{
			SetDestination( Destination );
		}

		var movedir = (Waypoints[CurrentWaypoint] - Entity.Position).WithZ( 0 ).Normal;
		var movevec = movedir * MoveSpeed;
		var distance = Vector3.DistanceBetween( Entity.Position, Waypoints[CurrentWaypoint] );
		if( distance <= 2 )
		{
			CurrentWaypoint++;
			return;
		}

		Entity.Velocity = movevec;
		Entity.Position += movevec * Time.Delta;
		Entity.Rotation = Rotation.Slerp( Entity.Rotation, Rotation.LookAt( movedir ), 8f * Time.Delta );
	}

	protected override void OnActivate()
	{
		base.OnActivate();

		AgentId = Entity.NetworkIdent;
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();

		NavigationEntity.Current.RemoveAgent( AgentId );
	}

}
