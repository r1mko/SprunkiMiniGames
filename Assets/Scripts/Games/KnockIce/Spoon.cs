using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Spoon : MonoBehaviour
{
    [SerializeField] private float verticalOffset = 1f;
    [SerializeField] private float verticalSpeed = 1f;

    [SerializeField, Required] private Transform knockTarget;
    [SerializeField] private float knockDuration = 0.3f;
    [SerializeField] private AnimationCurve knockCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Required] private KnockIceGameManager gameManager;

    private bool _movingUp = true;
    private Vector3 _initialLocalPosition;
    private Coroutine _activeRoutine;

    private void Awake()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        _initialLocalPosition = transform.localPosition;
    }

    public void StartSwinging()
    {
        StopActiveRoutine();
        SwitchTo(SwingRoutine());
    }

    public void ResetSpoon()
    {
        StopActiveRoutine();
        transform.localPosition = _initialLocalPosition;
        _movingUp = true;
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
        float bottomY = _initialLocalPosition.y - verticalOffset;
        float topY = _initialLocalPosition.y + verticalOffset;

        while (true)
        {
            float targetY = _movingUp ? topY : bottomY;

            while (Mathf.Abs(transform.localPosition.y - targetY) > 0.01f)
            {
                if (WasPointerPressedThisFrame() && gameManager.CanKnock)
                {
                    SwitchTo(KnockRoutine());
                    yield break;
                }

                Vector3 position = transform.localPosition;
                position.y = Mathf.MoveTowards(position.y, targetY, verticalSpeed * Time.deltaTime);
                transform.localPosition = position;
                yield return null;
            }

            _movingUp = !_movingUp;
        }
    }

    private IEnumerator KnockRoutine()
    {
        gameManager.OnSpoonClicked();

        Vector3 restPosition = transform.position;
        Vector3 outPosition = new Vector3(knockTarget.position.x, restPosition.y, restPosition.z);

        yield return MoveTo(restPosition, outPosition);
        yield return MoveTo(outPosition, restPosition);

        gameManager.OnSpoonKnocked();

        SwitchTo(SwingRoutine());
    }

    private IEnumerator MoveTo(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < knockDuration)
        {
            elapsed += Time.deltaTime;
            float t = knockCurve.Evaluate(Mathf.Clamp01(elapsed / knockDuration));
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
