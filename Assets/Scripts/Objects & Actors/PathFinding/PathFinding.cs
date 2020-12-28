using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium
{
	public static class PathFinding
	{

		public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, bool AllowDiagonals = true, bool allowDiagonalAroundCorner = false)
		{

			//TODO diagonals fix
			List<Vector2Int> waypoints = null;
			bool pathSuccess = false;

			CellBase startNode = Map.Instance.FindCell(start);
			CellBase targetNode = Map.Instance.FindCell(end);
			startNode.pathParent = startNode;


			if (startNode.Walkable && targetNode.Walkable)
			{
				Heap<CellBase> openSet = new Heap<CellBase>(Map.Instance.cells.Length);
				HashSet<CellBase> closedSet = new HashSet<CellBase>();
				openSet.Add(startNode);

				while (openSet.Count > 0)
				{
					CellBase currentNode = openSet.RemoveFirst();
					closedSet.Add(currentNode);

					if (currentNode == targetNode)
					{
						pathSuccess = true;
						break;
					}

					foreach (CellBase neighbour in currentNode.GetNeighbours(AllowDiagonals, allowDiagonalAroundCorner))
					{
						if (!neighbour.Walkable || closedSet.Contains(neighbour))
							continue;

						int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour, AllowDiagonals) + neighbour.MovementPenalty;
						if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
						{
							neighbour.gCost = newMovementCostToNeighbour;
							neighbour.hCost = GetDistance(neighbour, targetNode, AllowDiagonals);
							neighbour.pathParent = currentNode;

							if (!openSet.Contains(neighbour))
								openSet.Add(neighbour);
							else
								openSet.UpdateItem(neighbour);
						}
					}
				}
			}
			if (pathSuccess)
			{
				waypoints = RetracePath(startNode, targetNode);
				pathSuccess = waypoints.Count > 0;
			}
			return pathSuccess?waypoints:null;
		}


		static List<Vector2Int> RetracePath(CellBase startNode, CellBase endNode)
		{
			List<CellBase> path = new List<CellBase>();
			CellBase currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.pathParent;
			}
			List<Vector2Int> waypoints = SimplifyPath(path);
			waypoints.Reverse();
			return waypoints;

		}

		static List<Vector2Int> SimplifyPath(List<CellBase> path)
		{
			List<Vector2Int> waypoints = new List<Vector2Int>();
			Vector2 directionOld = Vector2.zero;

			for (int i = 1; i < path.Count; i++)
			{
				//Vector2 directionNew = new Vector2(path[i - 1].Position.x - path[i].Position.x, path[i - 1].Position.y - path[i].Position.y);
				//if (directionNew != directionOld)
					waypoints.Add(path[i].Position);
				//directionOld = directionNew;
			}
			return waypoints;
		}

		static int GetDistance(CellBase nodeA, CellBase nodeB, bool AllowDiagonals = true)
		{
			int dstX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
			int dstY = Mathf.Abs(nodeA.Position.y - nodeB.Position.y);
			if (AllowDiagonals)
			{
				if (dstX > dstY)
					return 14 * dstY + 10 * (dstX - dstY);
				return 14 * dstX + 10 * (dstY - dstX);
			}
			else return dstX*10 + dstY*10;
		}
	}
}
