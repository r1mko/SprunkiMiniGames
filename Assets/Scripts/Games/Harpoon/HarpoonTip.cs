using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HarpoonTip : MonoBehaviour
{
    [SerializeField, Required] private HarpoonGameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager.CanCatch && other.TryGetComponent(out Fish fish))
        {
            gameManager.CatchFish(fish);
        }
    }
}
