using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Balloon : MonoBehaviour
{
    [SerializeField, Required] private GameObject poppedBalloon;
    [SerializeField, Required] private PopBalloonGameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Needle>(out _))
        {
            return;
        }

        Pop();
        gameManager.OnBalloonHit();
    }

    public void Pop()
    {
        gameObject.SetActive(false);
        poppedBalloon.SetActive(true);
    }

    public void ResetBalloon()
    {
        gameObject.SetActive(true);
        poppedBalloon.SetActive(false);
    }
}
