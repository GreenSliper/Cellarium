using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium
{
	public abstract class Cellular : MonoBehaviour
	{
		//in CE coordinates
		[SerializeField]
		Vector2Int pos;
		//moves object actually on set
		public virtual Vector2Int Position {
			get { return pos; }
			set { pos = value; }
		}

		public virtual bool RefreshPosition()
		{
			Vector2Int p = Map.Instance.ToCECoordinates(transform.position.ToVector2());
			bool dirty = p != pos;
			pos = p;
			return dirty;
		}

		public virtual void Awake()
		{
			RefreshPosition();
		}
	}
}
