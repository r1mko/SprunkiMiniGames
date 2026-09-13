using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class Knife : MonoBehaviour
{
    [SerializeField] private float horizontalOffset = 1f;
    [SerializeField] private float horizontalSpeed = 1f;

    [SerializeField, Required] private Transform cutTarget;
    [SerializeField] private float riseDuration = 0.15f;
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Required] private Transform centerReference;
    [SerializeField, Required] private CutHalfGameManager gameManager;

    private bool _movingRight = true;
    private Vector3 _initialLocalPosition;
    private Coroutine _activeRoutine;

    private void Awake()
    {
        _initialLocalPosition = transform.localPosition;
    }

    public void StartSwinging()
    {
        StopActiveRoutine();
        SwitchTo(SwingRoutine());
    }

    public void ResetKnife()
    {
        StopActiveRoutine();
        transform.localPosition = _initialLocalPosition;
        _movingRight = true;
    }

    private void StopActiveRoutine()
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }
    }

    private void SwitchTo(IEnumerator routine)
    {
        _activeRoutine = StartCoroutine(routine);
    }

    private IEnumerator SwingRoutine()
    {
        float leftX = -horizontalOffset;
        float rightX = horizontalOffset;

        while (true)
        {
            float targetX = _movingRight ? rightX : leftX;

            while (Mathf.Abs(transform.localPosition.x - targetX) > 0.01f)
            {
                if (WasPointerPressedThisFrame() && gameManager.CanCut)
                {
                    SwitchTo(CutRoutine());
                    yield break;
                }

                Vector3 position = transform.localPosition;
                position.x = Mathf.MoveTowards(position.x, targetX, horizontalSpeed * Time.deltaTime);
                transform.localPosition = position;
                yield return null;
            }

            _movingRight = !_movingRight;
        }
    }

    private IEnumerator CutRoutine()
    {
        Vector3 restPosition = transform.position;
        Vector3 topPosition = new Vector3(restPosition.x, cutTarget.position.y, restPosition.z);

        yield return MoveTo(restPosition, topPosition);

        gameManager.OnKnifeReachedTarget(transform.position.x, horizontalOffset);

        yield return MoveTo(topPosition, restPosition);

        SwitchTo(SwingRoutine());
    }

    private IEnumerator MoveTo(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = riseCurve.Evaluate(Mathf.Clamp01(elapsed / riseDuration));
            transform.position = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }

        transform.position = to;
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }
}
