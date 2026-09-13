using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    [SerializeField, Required] private Transform topHalf;
    [SerializeField, Required] private Transform bottomHalf;
    [SerializeField] private float separationDistance = 1f;
    [SerializeField] private float separationDuration = 0.4f;
    [SerializeField] private AnimationCurve separationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Required] private SpriteMask topMask;
    [SerializeField, Required] private SpriteMask bottomMask;
    [SerializeField, Required] private GameObject cutLine;

    [SerializeField, Required] private CutHalfGameManager gameManager;

    private Vector3 _topInitialLocalPosition;
    private Vector3 _bottomInitialLocalPosition;
    private Vector3 _topMaskInitialLocalPosition;
    private Vector3 _bottomMaskInitialLocalPosition;
    private Coroutine _separateRoutine;

    private void Awake()
    {
        _topInitialLocalPosition = topHalf.localPosition;
        _bottomInitialLocalPosition = bottomHalf.localPosition;
        _topMaskInitialLocalPosition = topMask.transform.localPosition;
        _bottomMaskInitialLocalPosition = bottomMask.transform.localPosition;
    }

    public void Cut(float knifeWorldX)
    {
        PositionMasksAtCut(knifeWorldX);
        _separateRoutine = StartCoroutine(SeparateRoutine());
    }

    public void RevealCut()
    {
        topMask.gameObject.SetActive(true);
        bottomMask.gameObject.SetActive(true);
        cutLine.SetActive(false);
    }

    public void ResetObject()
    {
        if (_separateRoutine != null)
        {
            StopCoroutine(_separateRoutine);
            _separateRoutine = null;
        }

        topHalf.localPosition = _topInitialLocalPosition;
        bottomHalf.localPosition = _bottomInitialLocalPosition;

        topMask.transform.localPosition = _topMaskInitialLocalPosition;
        bottomMask.transform.localPosition = _bottomMaskInitialLocalPosition;

        topMask.gameObject.SetActive(false);
        bottomMask.gameObject.SetActive(false);
        cutLine.SetActive(true);
    }

    private void PositionMasksAtCut(float knifeWorldX)
    {
        PlaceMaskEdgeAtKnife(topMask.transform, topHalf, knifeWorldX, isLeftEdge: true);
        PlaceMaskEdgeAtKnife(bottomMask.transform, bottomHalf, knifeWorldX, isLeftEdge: false);
    }

    private static void PlaceMaskEdgeAtKnife(Transform mask, Transform parent, float knifeWorldX, bool isLeftEdge)
    {
        float knifeLocalX = parent.InverseTransformPoint(new Vector3(knifeWorldX, mask.position.y, mask.position.z)).x;
        float halfWidth = mask.localScale.x / 2f;

        Vector3 localPosition = mask.localPosition;
        localPosition.x = isLeftEdge ? knifeLocalX + halfWidth : knifeLocalX - halfWidth;
        mask.localPosition = localPosition;
    }

    private IEnumerator SeparateRoutine()
    {
        Vector3 topTarget = _topInitialLocalPosition + Vector3.left * separationDistance;
        Vector3 bottomTarget = _bottomInitialLocalPosition + Vector3.right * separationDistance;
        float elapsed = 0f;

        while (elapsed < separationDuration)
        {
            elapsed += Time.deltaTime;
            float t = separationCurve.Evaluate(Mathf.Clamp01(elapsed / separationDuration));
            topHalf.localPosition = Vector3.Lerp(_topInitialLocalPosition, topTarget, t);
            bottomHalf.localPosition = Vector3.Lerp(_bottomInitialLocalPosition, bottomTarget, t);
            yield return null;
        }

        topHalf.localPosition = topTarget;
        bottomHalf.localPosition = bottomTarget;

        _separateRoutine = null;
        gameManager.OnCutComplete();
    }
}
