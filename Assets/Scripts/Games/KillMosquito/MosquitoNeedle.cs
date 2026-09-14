using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MosquitoNeedle : MonoBehaviour
{
    [SerializeField, Required] private Transform stabTarget;
    [SerializeField] private float stabDuration = 0.15f;
    [SerializeField] private AnimationCurve stabCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Required] private KillMosquitoGameManager gameManager;

    private Vector3 _initialPosition;
    private Coroutine _activeRoutine;

    private void Awake()
    {
        _initialPosition = transform.position;
    }

    public void StartWaiting()
    {
        StopActiveRoutine();
        SwitchTo(WaitRoutine());
    }

    public void ResetNeedle()
    {
        StopActiveRoutine();
        transform.position = _initialPosition;
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

    private IEnumerator WaitRoutine()
    {
        while (true)
        {
            if (WasPointerPressedThisFrame() && gameManager.CanStab)
            {
                SwitchTo(StabRoutine());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator StabRoutine()
    {
        gameManager.OnStabStarted();

        yield return MoveTo(_initialPosition, stabTarget.position);
        yield return MoveTo(stabTarget.position, _initialPosition);

        gameManager.OnStabFinished();
        SwitchTo(WaitRoutine());
    }

    private IEnumerator MoveTo(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < stabDuration)
        {
            elapsed += Time.deltaTime;
            float t = stabCurve.Evaluate(Mathf.Clamp01(elapsed / stabDuration));
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
