using UnityEngine;

public class HideSpot : MonoBehaviour
{
    [Header("Core")]
    public Transform hidePoint;
    public Transform[] entryPoints;

    [Header("Facing / Exiting (optional)")]
    public Transform lookTarget;         // camera faces this when hidden

    [Tooltip("Optional look target while sliding into the hide spot")]
    public Transform lookAtWhenEntering;

    public Transform exitPoint;          // where the player winds up after exiting
    public Transform[] exitPath;         // extra way-points on the way out

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        // hide point ─ green
        if (hidePoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hidePoint.position, 0.1f);
            Gizmos.DrawLine(hidePoint.position, hidePoint.position + hidePoint.forward * .4f);
        }

        // entry pts ─ yellow
        if (entryPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var p in entryPoints)
                if (p) { Gizmos.DrawSphere(p.position, .08f); Gizmos.DrawLine(p.position, p.position + p.forward * .3f); }
        }

        // look target ─ cyan
        if (lookTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lookTarget.position, .08f);
            Gizmos.DrawLine(lookTarget.position, lookTarget.position + lookTarget.forward * .5f);
        }

        // exit point  ─ magenta
        if (exitPoint)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(exitPoint.position, Vector3.one * .12f);
            Gizmos.DrawLine(exitPoint.position, exitPoint.position + exitPoint.forward * .5f);
        }

        // exit path ─ orange
        if (exitPath != null)
        {
            Gizmos.color = new Color(1f, .5f, 0f);
            foreach (var p in exitPath)
                if (p) { Gizmos.DrawSphere(p.position, .08f); Gizmos.DrawLine(p.position, p.position + p.forward * .3f); }
        }
    }
#endif
    public virtual Quaternion GetEntryRotation()
    {
        if (entryPoints != null && entryPoints.Length > 0 && entryPoints[0])
            return Quaternion.LookRotation(entryPoints[0].forward, Vector3.up);
        if (lookAtWhenEntering)
            return Quaternion.LookRotation(lookAtWhenEntering.forward, Vector3.up);
        if (lookTarget)
            return Quaternion.LookRotation(lookTarget.forward, Vector3.up);
        return hidePoint ? hidePoint.rotation : transform.rotation;
    }
}