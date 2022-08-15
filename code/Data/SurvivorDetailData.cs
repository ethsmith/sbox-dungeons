
using System;
using System.Collections.Generic;

namespace Dungeons.Data;

internal partial class SurvivorDetailData
{

	public int Id { get; set; }
	public string Name { get; set; }
	public int Experience { get; set; }
	public List<ItemData> Stash { get; set; }
	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }
	public DateTime LastPlayed { get; set; }


	// internal use
	public string FileSystemPath;

}
