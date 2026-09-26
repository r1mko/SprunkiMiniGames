using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableBottle : MonoBehaviour
{
    [SerializeField, Required] private GameObject brokenBottle;
    [SerializeField, Tag] private string obstacleTag = "Obstacle";

    private Vector3 _brokenInitialLocalPosition;
    private Quaternion _brokenInitialLocalRotation;
    private bool _isInitialized;

    public bool IsBroken { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[BreakableBottle] {name}: trigger enter from '{other.name}' (tag '{other.tag}'), broken={IsBroken}, expected tag '{obstacleTag}'", this);

        if (!IsBroken && other.CompareTag(obstacleTag))
        {
            Break();
        }
    }

    public void ResetBottle()
    {
        EnsureInitialized();
        IsBroken = false;

        brokenBottle.transform.localPosition = _brokenInitialLocalPosition;
        brokenBottle.transform.localRotation = _brokenInitialLocalRotation;
        brokenBottle.SetActive(false);
        gameObject.SetActive(true);
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _brokenInitialLocalPosition = brokenBottle.transform.localPosition;
        _brokenInitialLocalRotation = brokenBottle.transform.localRotation;
    }

    private void Break()
    {
        IsBroken = true;
        Debug.Log($"[BreakableBottle] {name}: broken", this);
        gameObject.SetActive(false);
        brokenBottle.SetActive(true);
    }
}
