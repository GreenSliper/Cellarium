using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotConstraint : MonoBehaviour
{

	public Transform target;
	public bool X = true;
	public float offsetX;
	[Space()]
	public bool Y = true;
	public float offsetY;
	[Space()]
	public bool Z = true;
	public float offsetZ;

	void Update()
    {
		Vector3 targ = target.eulerAngles;
		Vector3 rot = new Vector3(X ? targ.x + offsetX : transform.eulerAngles.x,
			Y ? targ.y + offsetY : transform.eulerAngles.y,
			Z ? targ.z + offsetZ : transform.eulerAngles.z);
		transform.rotation = Quaternion.Euler(rot);
    }
}
