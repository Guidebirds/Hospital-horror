using UnityEngine;

public class HideSpot : MonoBehaviour, IInteractable
{
    public Transform hidePoint;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (hidePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hidePoint.position, 0.1f);
        }
    }
#endif
}