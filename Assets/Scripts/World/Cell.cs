using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium
{

	public class Cell : CellBase
	{
		[SerializeField]
		bool walkable = true;
		[SerializeField]
		int movementPenalty = 0;
		public override int MovementPenalty { get => movementPenalty; set { movementPenalty = value; } }
		public override bool Walkable { get => walkable; set { walkable = value; } }
		//character TODO
		//items TODO
		//effects TODO
	}
}