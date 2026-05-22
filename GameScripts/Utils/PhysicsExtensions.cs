using System;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsExtensions
{
	public static bool BoxCast(BoxCollider box, Vector3 direction, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		box.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return Physics.BoxCast(vector, vector2, direction, quaternion, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static bool BoxCast(BoxCollider box, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		box.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return Physics.BoxCast(vector, vector2, direction, out hitInfo, quaternion, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static RaycastHit[] BoxCastAll(BoxCollider box, Vector3 direction, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		box.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return Physics.BoxCastAll(vector, vector2, direction, quaternion, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static int BoxCastNonAlloc(BoxCollider box, Vector3 direction, RaycastHit[] results, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		box.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return Physics.BoxCastNonAlloc(vector, vector2, direction, results, quaternion, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static bool CheckBox(BoxCollider box, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		box.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return Physics.CheckBox(vector, vector2, quaternion, layerMask, queryTriggerInteraction);
	}

	public static Collider[] OverlapBox(BoxCollider box, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		box.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return Physics.OverlapBox(vector, vector2, quaternion, layerMask, queryTriggerInteraction);
	}

	public static int OverlapBoxNonAlloc(BoxCollider box, Collider[] results, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		box.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		return Physics.OverlapBoxNonAlloc(vector, vector2, results, quaternion, layerMask, queryTriggerInteraction);
	}

	public static int OverlapTwoBoxNonAlloc(BoxCollider box, BoxCollider box2, Collider[] results, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Bounds bounds = box.bounds;
		Bounds bounds2 = box2.bounds;
		bounds.Encapsulate(bounds2.min);
		bounds.Encapsulate(bounds2.max);
		return Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, results, Quaternion.identity, layerMask, queryTriggerInteraction);
	}

	public static void ToWorldSpaceBox(this BoxCollider box, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation)
	{
		orientation = box.transform.rotation;
		center = box.transform.TransformPoint(box.center);
		Vector3 vector = PhysicsExtensions.AbsVec3(box.transform.lossyScale);
		halfExtents = Vector3.Scale(vector, box.size) * 0.5f;
	}

	public static void ToWorldSpaceBox2(this BoxCollider box, out Vector3 halfExtents)
	{
		Vector3 lossyScale = box.transform.lossyScale;
		Vector3 vector = new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
		halfExtents = Vector3.Scale(vector, box.size) * 0.5f;
	}

	public static bool SphereCast(SphereCollider sphere, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		float num;
		sphere.ToWorldSpaceSphere(out vector, out num);
		return Physics.SphereCast(vector, num, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static RaycastHit[] SphereCastAll(SphereCollider sphere, Vector3 direction, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		float num;
		sphere.ToWorldSpaceSphere(out vector, out num);
		return Physics.SphereCastAll(vector, num, direction, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static int SphereCastNonAlloc(SphereCollider sphere, Vector3 direction, RaycastHit[] results, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		float num;
		sphere.ToWorldSpaceSphere(out vector, out num);
		return Physics.SphereCastNonAlloc(vector, num, direction, results, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static bool CheckSphere(SphereCollider sphere, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		float num;
		sphere.ToWorldSpaceSphere(out vector, out num);
		return Physics.CheckSphere(vector, num, layerMask, queryTriggerInteraction);
	}

	public static Collider[] OverlapSphere(SphereCollider sphere, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		float num;
		sphere.ToWorldSpaceSphere(out vector, out num);
		return Physics.OverlapSphere(vector, num, layerMask, queryTriggerInteraction);
	}

	public static int OverlapSphereNonAlloc(SphereCollider sphere, Collider[] results, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		float num;
		sphere.ToWorldSpaceSphere(out vector, out num);
		return Physics.OverlapSphereNonAlloc(vector, num, results, layerMask, queryTriggerInteraction);
	}

	public static void ToWorldSpaceSphere(this SphereCollider sphere, out Vector3 center, out float radius)
	{
		center = sphere.transform.TransformPoint(sphere.center);
		radius = sphere.radius * PhysicsExtensions.MaxVec3(PhysicsExtensions.AbsVec3(sphere.transform.lossyScale));
	}

	public static bool CapsuleCast(CapsuleCollider capsule, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		float num;
		capsule.ToWorldSpaceCapsule(out vector, out vector2, out num);
		return Physics.CapsuleCast(vector, vector2, num, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static RaycastHit[] CapsuleCastAll(CapsuleCollider capsule, Vector3 direction, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		float num;
		capsule.ToWorldSpaceCapsule(out vector, out vector2, out num);
		return Physics.CapsuleCastAll(vector, vector2, num, direction, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static int CapsuleCastNonAlloc(CapsuleCollider capsule, Vector3 direction, RaycastHit[] results, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		float num;
		capsule.ToWorldSpaceCapsule(out vector, out vector2, out num);
		return Physics.CapsuleCastNonAlloc(vector, vector2, num, direction, results, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static bool CheckCapsule(CapsuleCollider capsule, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		float num;
		capsule.ToWorldSpaceCapsule(out vector, out vector2, out num);
		return Physics.CheckCapsule(vector, vector2, num, layerMask, queryTriggerInteraction);
	}

	public static Collider[] OverlapCapsule(CapsuleCollider capsule, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		float num;
		capsule.ToWorldSpaceCapsule(out vector, out vector2, out num);
		return Physics.OverlapCapsule(vector, vector2, num, layerMask, queryTriggerInteraction);
	}

	public static int OverlapCapsuleNonAlloc(CapsuleCollider capsule, Collider[] results, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		Vector3 vector;
		Vector3 vector2;
		float num;
		capsule.ToWorldSpaceCapsule(out vector, out vector2, out num);
		return Physics.OverlapCapsuleNonAlloc(vector, vector2, num, results, layerMask, queryTriggerInteraction);
	}

	public static void ToWorldSpaceCapsule(this CapsuleCollider capsule, out Vector3 point0, out Vector3 point1, out float radius)
	{
		Vector3 vector = capsule.transform.TransformPoint(capsule.center);
		radius = 0f;
		float num = 0f;
		Vector3 vector2 = PhysicsExtensions.AbsVec3(capsule.transform.lossyScale);
		Vector3 vector3 = Vector3.zero;
		switch (capsule.direction)
		{
		case 0:
			radius = Mathf.Max(vector2.y, vector2.z) * capsule.radius;
			num = vector2.x * capsule.height;
			vector3 = capsule.transform.TransformDirection(Vector3.right);
			break;
		case 1:
			radius = Mathf.Max(vector2.x, vector2.z) * capsule.radius;
			num = vector2.y * capsule.height;
			vector3 = capsule.transform.TransformDirection(Vector3.up);
			break;
		case 2:
			radius = Mathf.Max(vector2.x, vector2.y) * capsule.radius;
			num = vector2.z * capsule.height;
			vector3 = capsule.transform.TransformDirection(Vector3.forward);
			break;
		}
		if (num < radius * 2f)
		{
			vector3 = Vector3.zero;
		}
		point0 = vector + vector3 * (num * 0.5f - radius);
		point1 = vector - vector3 * (num * 0.5f - radius);
	}

	public static void SortClosestToFurthest(RaycastHit[] hits, int hitCount = -1)
	{
		if (hitCount == 0)
		{
			return;
		}
		if (hitCount < 0)
		{
			hitCount = hits.Length;
		}
		Array.Sort<RaycastHit>(hits, 0, hitCount, PhysicsExtensions.ascendDistance);
	}

	private static Vector3 AbsVec3(Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	private static float MaxVec3(Vector3 v)
	{
		return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
	}

	private static PhysicsExtensions.AscendingDistanceComparer ascendDistance = new PhysicsExtensions.AscendingDistanceComparer();

	private class AscendingDistanceComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit h1, RaycastHit h2)
		{
			if (h1.distance < h2.distance)
			{
				return -1;
			}
			if (h1.distance <= h2.distance)
			{
				return 0;
			}
			return 1;
		}
	}
}
