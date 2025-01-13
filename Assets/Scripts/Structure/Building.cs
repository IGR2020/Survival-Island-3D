using UnityEngine;

public class Building : MonoBehaviour
{
    public float snapDist = 1f;
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
}
