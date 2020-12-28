using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium
{
	//cause the light source is meant to be cellular
	[RequireComponent(typeof(Cellular))]
	public class LightSource : MonoBehaviour
	{
		public enum LightFunctions {QuadroRoot = -4, QubeRoot = -3, SquareRoot = - 2, Linear = 1, Square, Qube, Quadro, Penta}
		[SerializeField]
		int radius = 3;
		[SerializeField]
		float intensity = 3;
		Cellular cellular;
		bool init = false;
		public LightFunctions lightFunction;
		public Color color;
		public bool Dirty = false;
		#region getters_setters
		public int Radius { get { return radius; }
			set {
				if (radius != value) {
					radius = value;
					Map.Instance.RefreshVisionMap();
				}
			}
		}
		
		public float Intensity
		{
			get { return intensity; }
			set
			{
				if (intensity != value)
				{
					intensity = value;
					Map.Instance.RefreshVisionMap();
				}
			}
		}
		public Vector2Int Position { get { return cellular.Position; } }
		#endregion
		//TODO light tremble effects maybe
		private void Awake()
		{
			if (init)
				return;
			init = true;
			cellular = GetComponent<Cellular>();
			Map.Instance.lights.Add(this);
			if (cellular is Movable)
				((Movable)cellular as Movable).onMoved += Map.Instance.RefreshVisionMap;
		}

		private void Update()
		{
			if (Dirty)
			{
				Dirty = false;
				Map.Instance.RefreshVisionMap();
			}
		}
	}
}