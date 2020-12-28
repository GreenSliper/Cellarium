using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cellarium
{
	public class Movable : Cellular
	{

		[Header("Movement animation")]
		public float MaxSpeed;
		public float AccelerationTime;
		float currentSpeed;
		//when move started
		float moveStartTime;
		//time, needed to stop/accelerate this time
		float curAccTime = 0;
		float curPathDuration;
		public States MoveState = States.Idle;
		[SerializeField]
		List<Vector2Int> path = new List<Vector2Int>();
		public bool IsMoving { get => path.Count > 0; }
		public Vector2Int NextPoint { get => path[0]; }
		public enum States { Idle = 0, Accelerating, Moving, Stopping }

		public delegate void OnMoved();
		//event
		public OnMoved onMoved;

		void GoToNextPoint()
		{
			onMoved?.Invoke();
			//TODO
		}

		public virtual float CalculatePathTime()
		{
			float dist = 0, accDist = 0, result = 0;
			//calc length
			for (int i = 1; i < path.Count; i++)
				dist += (path[i] - path[i - 1]).magnitude;
			accDist = MaxSpeed*AccelerationTime/2;
			if (dist >= accDist * 2)
			{
				//ignore stop path & acceleration path
				dist -= MaxSpeed * AccelerationTime;
				curAccTime = AccelerationTime; //reset
				result = dist / MaxSpeed + 2 * AccelerationTime;
			}
			else {
				//need to stop before maxspeed
				curAccTime = dist / MaxSpeed;
				result = 2 * curAccTime;
			}
			return result;
		}

		public virtual void CreatePath(Vector2Int destination)
		{
			path.Add(Position); path.Add(destination);
			curPathDuration = CalculatePathTime();
			path.RemoveAt(0);
		}

		void Teleport(Vector2Int point)
		{
			onMoved?.Invoke();
			transform.position = new Vector3(point.x * Map.Instance.CellSize, transform.position.y, point.y * Map.Instance.CellSize);
			Position = point;
		}

		public Vector2Int destination;
		public bool dirty = false;

		public void Update()
		{
			if (dirty)
			{
				CreatePath(destination);
				dirty = false;
			}
		}
		private void Start()
		{
			path = PathFinding.FindPath(Position, new Vector2Int(8, 8), true);
			path.Insert(0, Position);
			path.Add(new Vector2Int(8, 8));
			curPathDuration = CalculatePathTime();
		}

		public virtual void FixedUpdate()
		{
			if (IsMoving)
			{
				float lerpVal;
				switch (MoveState)
				{
					case States.Idle: MoveState = States.Accelerating;
						moveStartTime = Time.time;
						goto case States.Accelerating;

					case States.Accelerating:
						lerpVal = (Time.time - moveStartTime) / curAccTime;
						currentSpeed = Mathf.Lerp(0, MaxSpeed, lerpVal);
						if (lerpVal >= 1)
							if (curAccTime < AccelerationTime) //stop before maxspeed
								MoveState = States.Stopping;
							else
								MoveState = States.Moving;	//normal cycle
						break;

					case States.Moving:
						if (Time.time - moveStartTime > curPathDuration - curAccTime)
						{
							MoveState = States.Stopping;
							goto case States.Stopping;
						}
						break;

					case States.Stopping:
						lerpVal = (Time.time - moveStartTime - curPathDuration + curAccTime) / curAccTime;
						currentSpeed = Mathf.Lerp(MaxSpeed, 0, lerpVal);
						if (lerpVal >= 1)
							MoveState = States.Idle;
						break;
				}
				//actual movement
				Vector3 dest = (Map.Instance.FromCECoordinates(NextPoint) - transform.position);
				dest.y = 0;
				transform.position += currentSpeed * Map.Instance.CellSize * dest.normalized * Time.deltaTime;
				if (RefreshPosition())
					onMoved?.Invoke();
				Vector3 dif = transform.position - NextPoint.ToVector3()*Map.Instance.CellSize;
				dif.y = 0;
				if ((dif).sqrMagnitude < 0.01f)
					path.RemoveAt(0);
			}
		}
	}
}