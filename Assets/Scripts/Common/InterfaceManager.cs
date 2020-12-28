using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium {
	public class InterfaceManager : Singleton<InterfaceManager>
	{
		public Camera mainCamera;
		public LayerMask layerMask;
		public float wallHeight = 1;
		public float floorHeight = 0;
		// Update is called once per frame
		void Update()
		{
			Vector2 clickPoint;
			if (Ex.OnClick(out clickPoint, mainCamera))
			{
				CellBase cell;
				if ((cell = FindCellByClickPoint(clickPoint)) != null)
					print(cell.Position);
			}
		}

		public CellBase FindCellByClickPoint(Vector2 clickPoint)
		{
			CellBase cell;
			//get point at height of walls
			Ray ray = mainCamera.ScreenPointToRay(clickPoint);
			//ray equation
			float t = (wallHeight - ray.origin.y) / ray.direction.y;
			cell = Map.Instance.FindCell((ray.origin + ray.direction * t).ToVector2());
			if(cell is CellWall)
				return cell;
			//otherwise search for normal cell
			t = (floorHeight - ray.origin.y) / ray.direction.y;
			cell = Map.Instance.FindCell((ray.origin + ray.direction * t).ToVector2());
			return cell;
		}
	}
}