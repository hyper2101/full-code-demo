using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
	public static bool ContainsPoint(List<Vector2> polyPoints, Vector2 p)
	{
		int num = polyPoints.Count - 1;
		bool flag = false;
		int i = 0;
		while (i < polyPoints.Count)
		{
			if (((polyPoints[i].y <= p.y && p.y < polyPoints[num].y) || (polyPoints[num].y <= p.y && p.y < polyPoints[i].y)) && p.x < (polyPoints[num].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[num].y - polyPoints[i].y) + polyPoints[i].x)
			{
				flag = !flag;
			}
			num = i++;
		}
		return flag;
	}

	public static bool IsNan(this Vector3 vec)
	{
		return float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z);
	}

	public static bool LineLineIntersection(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, out Vector3 intersection)
	{
		Vector3 vector = linePoint2 - linePoint1;
		Vector3 vector2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 vector3 = Vector3.Cross(vector, lineVec2);
		if (Mathf.Abs(Vector3.Dot(vector, vector2)) < 0.0001f && vector2.sqrMagnitude > 0.0001f)
		{
			float num = Vector3.Dot(vector3, vector2) / vector2.sqrMagnitude;
			intersection = linePoint1 + lineVec1 * num;
			return true;
		}
		intersection = Vector3.zero;
		return false;
	}

	public static bool IsSameDirection(Vector2 a, Vector2 a_dir, Vector2 b, Vector2 b_dir)
	{
		Vector2 vector = b - a;
		if (Vector2.Dot(a_dir, vector) < 0f)
		{
			vector = -vector;
		}
		return Vector2.Dot(vector, b_dir) >= 0f;
	}

	public static bool IsSameDirectionProjected(Vector3 a, Vector3 a_dir, Vector3 b, Vector3 b_dir)
	{
		return MathHelper.IsSameDirection(MathHelper.To2D(a), MathHelper.To2D(a_dir), MathHelper.To2D(b), MathHelper.To2D(b_dir));
	}

	private static Vector2 To2D(Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	private static Vector3 From2D(Vector2 v)
	{
		return new Vector3(v.x, 0f, v.y);
	}

	public static float PointLineDistance(Vector2 start, Vector2 end, Vector2 point)
	{
		Vector2 vector = MathHelper.ProjectPointOnLine(start, end, point);
		return (point - vector).magnitude;
	}

	public static float PointLineDistanceSqr(Vector2 start, Vector2 end, Vector2 point)
	{
		Vector2 vector = MathHelper.ProjectPointOnLine(start, end, point);
		return (point - vector).sqrMagnitude;
	}

	public static Vector2 ClosestPointOnRectangle(Rect rect, Vector2 point)
	{
		Vector2[] array = new Vector2[]
		{
			new Vector2(rect.xMin, rect.yMin),
			new Vector2(rect.xMax, rect.yMin),
			new Vector2(rect.xMax, rect.yMax),
			new Vector2(rect.xMin, rect.yMax)
		};
		float num = float.MaxValue;
		Vector2 vector = rect.center;
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 vector2 = array[i];
			Vector2 vector3 = array[(i + 1) % 4];
			Vector2 vector4 = MathHelper.ProjectPointOnLine(vector2, vector3, point);
			float magnitude = (point - vector4).magnitude;
			if (magnitude < num)
			{
				num = magnitude;
				vector = vector4;
			}
		}
		return vector;
	}

	public static Vector2 ProjectPointOnLine(Vector2 start, Vector2 end, Vector2 point)
	{
		Vector2 vector = end - start;
		Vector2 normalized = vector.normalized;
		float num = Vector2.Dot(point - start, normalized);
		num = Mathf.Clamp(num, 0f, vector.magnitude);
		return start + normalized * num;
	}

	public static float ProjectedPointLineDistance(Vector3 start, Vector3 end, Vector3 point)
	{
		return MathHelper.PointLineDistance(MathHelper.To2D(start), MathHelper.To2D(end), MathHelper.To2D(point));
	}

	public static bool GetProjectedLineSegmentIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersection)
	{
		Vector2 vector = MathHelper.To2D(p1);
		Vector2 vector2 = MathHelper.To2D(p2);
		Vector2 vector3 = MathHelper.To2D(p3);
		Vector2 vector4 = MathHelper.To2D(p4);
		Vector2 vector5;
		float num;
		bool flag = MathHelper.LineSegmentsIntersection(vector, vector2, vector3, vector4, out vector5, out num);
		intersection = MathHelper.From2D(vector5);
		return flag;
	}

	public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection, out float u)
	{
		intersection = Vector2.zero;
		float num = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
		u = 0f;
		if (num == 0f)
		{
			return false;
		}
		u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / num;
		float num2 = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / num;
		if (u < 0f || u > 1f || num2 < 0f || num2 > 1f)
		{
			return false;
		}
		intersection.x = p1.x + u * (p2.x - p1.x);
		intersection.y = p1.y + u * (p2.y - p1.y);
		return true;
	}

	public static float Angle(Vector2 pos1, Vector2 pos2)
	{
		Vector2 vector = pos2 - pos1;
		Vector2 vector2 = new Vector2(1f, 0f);
		float num = Vector2.Angle(vector, vector2);
		if (Vector3.Cross(vector, vector2).z > 0f)
		{
			num = 360f - num;
		}
		return num;
	}

	public static float SqrDistance(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public static float EvaluateFunctionWithControlPoints(float x1, float y1, float x2, float y2, float t)
	{
		return MathHelper.CubicBezier(Vector2.zero, new Vector2(x1, y1), new Vector2(x2, y2), Vector2.one, t).y;
	}

	public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		return (1f - t) * MathHelper.QuadBezier(p0, p1, p2, t) + t * MathHelper.QuadBezier(p1, p2, p3, t);
	}

	public static Vector2 QuadBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
	{
		t = Mathf.Clamp01(t);
		return Mathf.Pow(1f - t, 2f) * p0 + 2f * (1f - t) * t * p1 + Mathf.Pow(t, 2f) * p2;
	}

	public static Color DebugColor;
}
