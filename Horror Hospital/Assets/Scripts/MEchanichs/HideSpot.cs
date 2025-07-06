using UnityEngine;

public class HideSpot : MonoBehaviour
{
    [Header("Required")]
    [Tooltip("Where the player ends up (position) once hidden.")]
    public Transform hidePoint;

    [Tooltip("Optional intermediate way-points the player moves through on entry.")]
    public Transform[] entryPoints;

    [Header("Optional")]
    [Tooltip("If assigned, the player’s view will align with this transform’s forward direction after hiding.\n" +
             "Leave empty to keep the old behaviour (use hidePoint rotation or player’s current rotation).")]
    public Transform lookTarget;   // ? NEW

    // ???????????????????????????? Debug Gizmos ???????????????????????????
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // hide point  ? green
        if (hidePoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hidePoint.position, 0.1f);
            Gizmos.DrawLine(hidePoint.position,
                            hidePoint.position + hidePoint.forward * 0.4f);
        }

        // entry points ? yellow
        if (entryPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var pt in entryPoints)
            {
                if (!pt) continue;
                Gizmos.DrawSphere(pt.position, 0.08f);
                Gizmos.DrawLine(pt.position, pt.position + pt.forward * 0.3f);
            }
        }

        // look target  ? cyan
        if (lookTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lookTarget.position, 0.08f);
            Gizmos.DrawLine(lookTarget.position,
                            lookTarget.position + lookTarget.forward * 0.5f);
        }
    }
#endif
}
