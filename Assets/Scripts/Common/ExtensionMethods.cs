using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Ex
{
	//leaves S=V=1
	public static Color InterpolateHueSat(Color a, Color b, float t)
	{
		float dummy, h1, h2;
		Color.RGBToHSV(a, out h1, out dummy, out dummy);
		Color.RGBToHSV(b, out h2, out dummy, out dummy);
		dummy = 0;
		if (h1 < 0.5f)
		{
			h1++; dummy += 0.5f;
		}
		if (h2 < 0.5f)
		{
			h2++; dummy += 0.5f;
		}
		h1 = Mathf.Lerp(h1, h2, t);
		return Color.HSVToRGB(h1-dummy, 1, 1);
	}

	//returns true and point if click ended in this frame
	public static bool OnClick(out Vector2 point, Camera cam)
	{
		point = Vector2.zero;
		if (EventSystem.current.IsPointerOverGameObject())
			return false;
		if (Input.touchCount > 0)
			if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
				return false;
		if (Input.mousePresent)
		{
			if (Input.GetMouseButtonUp(0))
			{
				point = Input.mousePosition;
				return true;
			}
		}
		else
		{
			if (Input.touches.Length > 0)
				if (Input.touches[0].phase == TouchPhase.Ended)
				{
					point = Input.touches[0].position;
					return true;
				}
		}
		return false;
	}

	public static Vector2Int Floor(this Vector2 vec) => 
		new Vector2Int((int)Mathf.Floor(vec.x), (int)Mathf.Floor(vec.y));
	public static Vector2Int Round(this Vector2 vec) =>
		new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));

	//x->x1 y->0 z->y1, returns (x1, y1)
	public static Vector2 ToVector2(this Vector3 vec) => new Vector2(vec.x, vec.z);
	public static Vector3 ToVector3(this Vector2 vec) => new Vector3(vec.x, 0, vec.y);
	public static Vector3 ToVector3(this Vector2Int vec) => new Vector3(vec.x, 0, vec.y);

	public static float Squared(this float f) => f * f;

	public static Color Normalized(this Color c)
	{
		return 4*c/(c.r + c.g + c.b + c.a);
	}
}
