using UnityEngine;

public static class AxisExtensions
{
	public static Vector3 AsVector(this Axis axis)
	{
		switch (axis)
		{
			case Axis.X:
				return Vector3.left;
			case Axis.Y:
				return Vector3.up;
			case Axis.Z:
				return Vector3.back;
			default:
				return Vector3.zero;
		}
	}
}
