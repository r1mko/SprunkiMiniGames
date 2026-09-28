using NaughtyAttributes;
using UnityEngine;

public class CarTrack : MonoBehaviour
{
    [SerializeField, Required] private Transform straightTarget;
    [SerializeField, Required] private Transform circleCenter;
    [SerializeField] private float radius = 3f;
    [SerializeField] private bool clockwise = true;

    public float Radius => radius;
    public float Direction => clockwise ? -1f : 1f;
    public Vector3 StraightTarget => straightTarget.position;

    public float GetAngle(Vector3 position)
    {
        Vector2 offset = position - circleCenter.position;
        return Mathf.Atan2(offset.y, offset.x);
    }

    public Vector3 GetPoint(float angle)
    {
        return circleCenter.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
    }

    private void OnDrawGizmosSelected()
    {
        const int segments = 64;
        Gizmos.color = Color.yellow;

        if (circleCenter != null)
        {
            Vector3 previous = GetPoint(0f);
            for (int i = 1; i <= segments; i++)
            {
                Vector3 next = GetPoint(i * 2f * Mathf.PI / segments);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        if (straightTarget != null)
        {
            Gizmos.DrawWireSphere(straightTarget.position, 0.1f);
        }
    }
}
