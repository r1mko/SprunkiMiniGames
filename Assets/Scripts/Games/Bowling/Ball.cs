using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField] private float horizontalOffset = 1f;
    [SerializeField] private float horizontalSpeed = 1f;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float launchTorqueImpulse = 5f;
    [SerializeField] private float resolveYThreshold = 5f;
    [SerializeField] private float flyFallbackDelay = 4f;

    [SerializeField, Required] private BowlingGameManager gameManager;

    private Rigidbody2D _rb;
    private bool _movingRight = true;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Coroutine _swingRoutine;
    private Coroutine _flightRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public void StartSwinging()
    {
        StopSwinging();
        _swingRoutine = StartCoroutine(SwingRoutine());
    }

    public void StopSwinging()
    {
        if (_swingRoutine != null)
        {
            StopCoroutine(_swingRoutine);
            _swingRoutine = null;
        }
    }

    public void Hide()
    {
        StopSwinging();
        StopFlight();

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        gameObject.SetActive(false);
    }

    public void PrepareAndActivate()
    {
        PrepareAndActivate(_initialPosition.x, true);
    }

    public void PrepareAndActivate(float launchX, bool movingRight)
    {
        gameObject.SetActive(true);

        StopFlight();
        _movingRight = movingRight;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        Vector3 position = _initialPosition;
        position.x = launchX;
        transform.position = position;
        transform.rotation = _initialRotation;
    }

    private void StopFlight()
    {
        if (_flightRoutine != null)
        {
            StopCoroutine(_flightRoutine);
            _flightRoutine = null;
        }
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
                if (WasPointerPressedThisFrame() && gameManager.CanThrow)
                {
                    Launch();
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

    private void Launch()
    {
        float launchX = transform.position.x;
        bool launchMovingRight = _movingRight;

        _rb.linearVelocity = Vector2.up * launchSpeed;
        _rb.AddTorque(launchTorqueImpulse, ForceMode2D.Impulse);
        _flightRoutine = StartCoroutine(FlightRoutine());
        gameManager.OnBallThrown(launchX, launchMovingRight);
    }

    private IEnumerator FlightRoutine()
    {
        float elapsed = 0f;
        bool resolved = false;

        while (elapsed < flyFallbackDelay)
        {
            if (!resolved && transform.position.y >= resolveYThreshold)
            {
                resolved = true;
                gameManager.OnThrowResolved();
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _flightRoutine = null;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        gameObject.SetActive(false);

        if (!resolved)
        {
            gameManager.OnThrowResolved();
        }
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }
}
