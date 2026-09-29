using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableWood : MonoBehaviour
{
    [SerializeField, Required] private GameObject intactWood;
    [SerializeField, Required] private GameObject brokenWood;

    public bool IsBroken { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsBroken && other.GetComponentInParent<PunchFist>() != null)
        {
            Break();
        }
    }

    public void ResetWood()
    {
        IsBroken = false;
        intactWood.SetActive(true);
        brokenWood.SetActive(false);
    }

    private void Break()
    {
        IsBroken = true;
        intactWood.SetActive(false);
        brokenWood.SetActive(true);
    }
}
