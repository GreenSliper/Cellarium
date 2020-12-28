using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium
{
	//realized lightning model + A* stuff included
	public abstract class CellBase : Cellular, IHeapItem<CellBase>
	{
		[Header("How is cell affected by CE light, set default to 0")]
		public float lightIntensity = 0;
		[Header("The higher this value is, the less it will be affected by light")]
		public float maxIntensity = 15;
		public Color lightColor;
		public bool AllowHDR = false;
		public float colorChangeTime = 0.5f;
		public Color baseColor;
		public float MapVisibility { get => Map.Instance.visionMap[Position.x, Position.y]; }

		public delegate void CellAction();
		//when becomes visible first time
		public CellAction onBecomeLit;
		public CellAction onBecomeUnlit;
		public bool hideOnStart = true;

		float curChangeTime;
		Color prevColor;
		private Renderer _renderer;
		private MaterialPropertyBlock _propBlock;

		#region heap
		int heapIndex;
		public int HeapIndex
		{
			get => heapIndex;
			set
			{
				heapIndex = value;
			}
		}
		public int CompareTo(CellBase cellToCompare)
		{
			int compare = fCost.CompareTo(cellToCompare.fCost);
			if (compare == 0)
				compare = hCost.CompareTo(cellToCompare.hCost);
			return -compare;
		}
		#endregion

		#region A Star pathfinding
		[HideInInspector]
		public int gCost;
		[HideInInspector]
		public int hCost;
		public int fCost { get => gCost + hCost; }
		public List<CellBase> GetNeighbours(bool allowDiagonals = true, bool allowDiagonalAroundCorner = false)
		{
			List<CellBase> nb = new List<CellBase>();
			CellBase cb;
			for (int x = -1; x < 2; x++)
				for (int y = -1; y < 2; y++)
				{
					if (allowDiagonals)
					{
						//same point
						if (x == 0 && y == 0)
							continue;
						//if diagonal step around isn't allowed
						if (!allowDiagonalAroundCorner)
						{
							if ((x + y) % 2 == 0)
								if ((y < 0 && !Map.Instance.isCellWalkable(Position + Vector2Int.down))
									|| (y > 0 && !Map.Instance.isCellWalkable(Position + Vector2Int.up))
									|| (x < 0 && !Map.Instance.isCellWalkable(Position + Vector2Int.left))
									|| (x > 0 && !Map.Instance.isCellWalkable(Position + Vector2Int.right)))
									continue;
						}
					}
					else if ((x + y) % 2 == 0)	//diagonal point
						continue;
					if ((cb = Map.Instance.FindCell(Position.x + x, Position.y + y)) != null)
						nb.Add(cb);
				}
			return nb;
		}
		[HideInInspector]
		//Used in pathtracing in a* algorithm
		public CellBase pathParent;
		public abstract int MovementPenalty { get; set; }
		public abstract bool Walkable { get; set; }
		#endregion

		public override void Awake()
		{
			base.Awake();
			_renderer = GetComponent<Renderer>();
			_propBlock = new MaterialPropertyBlock();
			onBecomeLit += () => _renderer.enabled = true;
			if (hideOnStart)
				_renderer.enabled = false;
		}

		Color curColor;
		public virtual void Update()
		{
			if (curChangeTime >= 0)
			{
				_renderer.GetPropertyBlock(_propBlock);
				curChangeTime += Time.deltaTime;
				if (colorChangeTime == 0 || colorChangeTime <= curChangeTime)
				{
					_propBlock.SetColor("_BaseColor", lightColor);
					curChangeTime = -1;
				}
				else
				{
					curColor = Color.Lerp(prevColor, lightColor, curChangeTime / colorChangeTime);
					_propBlock.SetColor("_BaseColor", curColor);
				}
				//TODO create shader with [PerRendererData]_Color ("Color", Color) = (1,1,1,1)
				_renderer.SetPropertyBlock(_propBlock);
			}
		}

		public void ApplyLightning()
		{
			prevColor = curColor;
			curChangeTime = 0;
			float l_intensity = Map.Instance.visionMap[Position.x, Position.y];
			//events
			if (l_intensity != lightIntensity)
			{
				if (lightIntensity == 0)
					onBecomeLit?.Invoke();
				else
					onBecomeUnlit?.Invoke();
			}

			lightIntensity = l_intensity;
			if (lightIntensity > Map.Zones.MinVisLight)
				if (AllowHDR)
					lightColor = Map.Instance.lightColorMap[Position.x, Position.y] * lightIntensity / maxIntensity;
				else
					lightColor = Map.Instance.lightColorMap[Position.x, Position.y].Normalized() * Mathf.Clamp01(lightIntensity / maxIntensity);
			else if(MapVisibility == Map.Zones.Seen)
				lightColor = Map.Instance.seenColor;
			else
				lightColor = Map.Instance.ambientLevelColor;
		}

		public virtual void OnClick()
		{
		}
	}
}
