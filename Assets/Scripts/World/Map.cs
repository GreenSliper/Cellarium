using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium
{
	public class Map : Singleton<Map>
	{
		public static class Zones
		{
			public const float SeeThroughWall = -1, Wall = 0, Floor = 1, Water = 2;
			public const float MinVisLight = .2f, Seen = 0.1f;
		}
		//size of a cell in world coordinates
		public int CellSize = 2;
		public bool CanSeeBehindCorners = false;

		//maps: [horizontal, vertical]
		//what cellPrefab stands on each cell (for generation)
		public int[,] prefabMap;
		//use Zones for this
		public float[,] walkMap = { { }, { } };
		//What is lit, seen or unseen
		public float[,] visionMap;
		//Color blending for multiple lights
		public Color[,] lightColorMap;
		//all cells
		public CellBase[] cells;
		//cells aligned with their position
		public CellBase[,] cellMatrix;
		public Color ambientLevelColor = Color.black;
		public Color seenColor;
		public Transform[] cellPrefabs;
		public TextAsset curMap;

		public void Start()
		{
			CreateMapFromFile(curMap, 10, 10);
		}

		public void CreateMapFromFile(TextAsset level, int width, int height)
		{
			InitMap(width, height);
			cells = new CellBase[width * height];
			string[] lines = level.text.Split('\n');
			for (int y = 0; y < height; y++)
			{
				string[] curLine = lines[y].Split(' '); int prefabID;
				for (int x = 0; x < width; x++)
				{
					prefabMap[x, y] = prefabID = int.Parse(curLine[x]);
					walkMap[x, y] = prefabID!=1?Zones.Floor:Zones.Wall;
					cellMatrix[x,y] = cells[x + y * width] = Instantiate(cellPrefabs[prefabID], 
						cellPrefabs[prefabID].localPosition + new Vector3(x, 0, y) * CellSize,
						cellPrefabs[prefabID].rotation, transform).GetComponentInChildren<CellBase>();
				}
			}
			RefreshVisionMap();
		}

		public void CreatePlaneMap(int width, int height, int prefabID = 0)
		{
			InitMap(width, height);
			cells = new CellBase[width * height];
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
				{
					prefabMap[x, y] = prefabID;
					walkMap[x, y] = Zones.Floor;
					cells[x + y * width] = Instantiate(cellPrefabs[prefabID], new Vector3(x, 0, y) * CellSize, 
						cellPrefabs[prefabID].rotation, transform).GetComponentInChildren<CellBase>();
				}
			RefreshVisionMap();
		}

		public void InitMap(int width, int height)
		{
			prefabMap = new int[width, height];
			walkMap = new float[width, height];
			visionMap = new float[width, height];
			lightColorMap = new Color[width, height];
			cellMatrix = new CellBase[width, height];
		}

		#region Coordinates Transformation & Cell searching
		public Vector2Int ToCECoordinates(Vector2 point) => (point / CellSize).Round();
		//get cell center
		public Vector3 FromCECoordinates(Vector2Int point) => new Vector3(point.x * CellSize, 0, point.y  * CellSize);

		//just pass its world postion
		public CellBase FindCell(Vector3 worldPos) => FindCell(new Vector2(worldPos.x, worldPos.z));

		//x & y have to be in CE coordinates
		public CellBase FindCell(int x, int y) => FindCell(new Vector2Int(x,y));

		//position = position in world space
		public CellBase FindCell(Vector2 position)
		{
			Vector2Int pos = ToCECoordinates(position);
			return FindCell(pos);
		}

		//find cell in the list by its coordinates (CE coordinates)
		public CellBase FindCell(Vector2Int position)
		{
			if (position.x > -1 && position.y > -1 && position.x < cellMatrix.GetLength(0) && position.y < cellMatrix.GetLength(1))
				return cellMatrix[position.x, position.y];
			return null;
		}

		public bool isCellWalkable(Vector2Int pos)
		{
			CellBase c = FindCell(pos);
			return c == null ? false : c.Walkable;
		}

		#endregion
		#region vision
		public List<LightSource> lights = new List<LightSource>();

		public void RefreshVisionMap()
		{
			RefreshVisionMap(lights, CanSeeBehindCorners);
		}

		public bool CheckVisibility(float x0, float y0, float x1, float y1, bool isWall, bool canSeeBehindCorners = false, bool SeeWallsBetter = true)
		{
			float difX = x1 - x0;
			float difY = y1 - y0;
			float dist = Mathf.Abs(difX) + Mathf.Abs(difY);
			float dx = difX / dist;
			float dy = difY / dist;
			//for corners correction
			float microX = 0.001f * Mathf.Sign(difX) * (canSeeBehindCorners ? 1 : -1);
			float microY = 0.001f * Mathf.Sign(difY) * (canSeeBehindCorners ? -1 : 1);
			for (int i = 0, x, y; i <= /*Mathf.Ceil*/(dist); i++)
			{
				x = (int)Mathf.Floor(x0 + dx * i - microX);
				y = (int)Mathf.Floor(y0 + dy * i - microY);
				if (x >= 0 && y >= 0 && x <= walkMap.GetLength(0) && y <= walkMap.GetLength(1) && walkMap[x, y] == Zones.Wall)
					if (isWall) //if we are checking the wall, we ignore one final collision
						isWall = false;
					else
						return false;
			}
			return true;
		}

		public void RefreshVisionMap(List<LightSource> ls, bool canSeeBehindCorners = false, bool seeWallsBetter = true)
		{
			//remove old sources light & add ambient to seen
			for (int i = 0; i < cells.Length; i++)
				if (visionMap[cells[i].Position.x, cells[i].Position.y] >= Zones.MinVisLight)
				{
					visionMap[cells[i].Position.x, cells[i].Position.y] = Zones.Seen;
					lightColorMap[cells[i].Position.x, cells[i].Position.y] = seenColor;
				}

			for (int i = 0; i < ls.Count; i++)
			{
				//add new light
				for (int x = Mathf.Max(ls[i].Position.x - ls[i].Radius, 0); x < Mathf.Min(walkMap.GetLength(0), ls[i].Position.x + ls[i].Radius + 1); x++)
					for (int y = Mathf.Max(ls[i].Position.y - ls[i].Radius, 0); y < Mathf.Min(walkMap.GetLength(1), ls[i].Position.y + ls[i].Radius + 1); y++)
					{
						float dist = Mathf.Sqrt(Mathf.Pow(ls[i].Position.x - x, 2) + Mathf.Pow(ls[i].Position.y - y, 2));
						if (dist <= ls[i].Radius)
						{
							bool isWall = walkMap[x, y] == Zones.Wall;
							Vector2 wallFix = Vector2.zero;
							bool visible = CheckVisibility(ls[i].Position.x + 0.5f, ls[i].Position.y + 0.5f,
								x + 0.5f + wallFix.x, y + 0.5f+ wallFix.y, isWall, canSeeBehindCorners);
							//additional check
							if (x == 5 && y == 6)
								;
							if (!visible && isWall && seeWallsBetter)
							{
								//if (Mathf.Abs(ls[i].Position.x - x) == 1)
									wallFix.x = Mathf.Sign(ls[i].Position.x - x) / 1.98f;
								//if (Mathf.Abs(ls[i].Position.y - y) == 1)
									wallFix.y = Mathf.Sign(ls[i].Position.y - y) / 1.98f;
								//if(wallFix.sqrMagnitude!=0)
									visible = CheckVisibility(ls[i].Position.x + 0.5f, ls[i].Position.y + 0.5f,
										x + 0.5f + wallFix.x, y + 0.5f + wallFix.y, false/*we ignore the fact it's a wall*/, canSeeBehindCorners);
							}

							if (visible)
							{
								//(1-x^n)*intensity, where x = dist/(maxdist+1), n = 1, 2, 3.. if non-root, n = 1/2, 1/3, 1/4... if root type
								float value = 0;
								if ((int)ls[i].lightFunction>0)
									value = (1 - Mathf.Pow(dist / (ls[i].Radius + 1), (int)ls[i].lightFunction));
								else
									value = 1 - Mathf.Pow(dist / (ls[i].Radius + 1), -1f/(int)ls[i].lightFunction);
								if (visionMap[x, y] <= Zones.Seen)
									lightColorMap[x, y] = ambientLevelColor;
								visionMap[x, y] = Mathf.Max(Zones.MinVisLight, visionMap[x,y]) + ls[i].Intensity * value;
								//color blend
								lightColorMap[x, y] += ls[i].color * value * ls[i].Intensity;
							}
						}
					}
			}
			//update <Cell> cells
			for (int i = 0; i < cells.Length; i++)
				cells[i].ApplyLightning();
		}
		#endregion
	}
}
