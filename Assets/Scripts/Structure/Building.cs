using UnityEngine;

public class Building : MonoBehaviour
{
    public float snapDist = 1f;
    public bool snapToGrid = true;
    public bool snapY;
    public LayerMask buildingMask;

    public Quaternion SnapBuildRotation(Vector3 buildPoint, Quaternion buildRot)
    {
        Collider[] collisions = Physics.OverlapSphere(buildPoint, snapDist, buildingMask);

        if (collisions.Length < 1) return buildRot;

        Transform nearestBuild = collisions[0].transform;
        buildRot = nearestBuild.rotation;
        return buildRot;
    }

    public Quaternion SnapBuildRotation(Transform buildingTransform)
    {
        return SnapBuildRotation(buildingTransform.position, buildingTransform.rotation);
    }

    public Vector3 SnapToGrid(Vector3 buildPoint, Vector3 size)
    {
        if (!snapToGrid) { return buildPoint; }
        buildPoint.x = Mathf.RoundToInt(buildPoint.x / size.x) * size.x;
        buildPoint.z = Mathf.RoundToInt(buildPoint.z / size.z) * size.z;
        if (snapY) buildPoint.y = Mathf.RoundToInt(buildPoint.y / size.y) * size.y;
		return buildPoint;
	}

    public Vector3 SnapToGrid(Transform buildTransform)
    {
        BoxCollider size = buildTransform.GetComponent<BoxCollider>();
        if (size == null) {return buildTransform.position; }
        return SnapToGrid(buildTransform.position, size.size);
    }
}
