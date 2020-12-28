using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Cellarium
{
	public class CellWall : CellBase
	{
		//wall is never walkable (some walkable walls are added by overriding this again)
		public override bool Walkable { get => false; set { } }
		//wall is never walkable (some walkable walls are added by overriding this again)
		public override int MovementPenalty { get => 0; set { } }
		//destructibility TODO
	}
}