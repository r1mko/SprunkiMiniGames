using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MissBoundary : MonoBehaviour
{
    [SerializeField, Required] private PopBalloonGameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Needle>(out _))
        {
            return;
        }

        gameManager.OnNeedleMissed();
    }
}
