using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PingPongMover))]
public class ShootNeedle : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float flightFallbackDelay = 3f;
    [SerializeField, Tag] private string obstacleTag = "Obstacle";

    [SerializeField] private float obstacleHitDelay = 0.75f;
    [SerializeField] private float wobbleAngle = 15f;
    [SerializeField] private int wobbleCount = 2;
    [SerializeField] private float wobbleSpeed = 1f;

    private Rigidbody2D _rb;
    private PingPongMover _mover;
    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;
    private bool _isInitialized;
    private bool _isFlying;
    private Coroutine _flightRoutine;

    public event Action<ShootNeedle> Finished;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isFlying || !collision.gameObject.CompareTag(obstacleTag))
        {
            return;
        }

        _isFlying = false;
        Freeze();
        StopFlightRoutine();
        _flightRoutine = StartCoroutine(ObstacleHitRoutine());
    }

    public void StartSliding(float direction)
    {
        EnsureInitialized();
        gameObject.SetActive(true);
        _mover.StartMoving(direction);
    }

    public void Launch()
    {
        _mover.StopMoving();
        _isFlying = true;
        _rb.linearVelocity = Vector2.up * launchSpeed;
        _flightRoutine = StartCoroutine(FlightFallbackRoutine());
    }

    public void ResetNeedle()
    {
        EnsureInitialized();
        StopFlightRoutine();
        _mover.ResetMover();
        _isFlying = false;

        Freeze();
        transform.localPosition = _initialLocalPosition;
        transform.localRotation = _initialLocalRotation;
        gameObject.SetActive(false);
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _rb = GetComponent<Rigidbody2D>();
        _mover = GetComponent<PingPongMover>();
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;
    }

    private void Freeze()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }

    private void StopFlightRoutine()
    {
        if (_flightRoutine != null)
        {
            StopCoroutine(_flightRoutine);
            _flightRoutine = null;
        }
    }

    private IEnumerator FlightFallbackRoutine()
    {
        yield return new WaitForSeconds(flightFallbackDelay);

        _flightRoutine = null;
        Finish();
    }

    private IEnumerator ObstacleHitRoutine()
    {
        float elapsed = 0f;

        while (elapsed < obstacleHitDelay)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / obstacleHitDelay);
            float angle = Mathf.Sin(t * wobbleCount * 2f * Mathf.PI * wobbleSpeed) * wobbleAngle;
            transform.localRotation = _initialLocalRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        transform.localRotation = _initialLocalRotation;
        _flightRoutine = null;
        Finish();
    }

    private void Finish()
    {
        _isFlying = false;
        Freeze();
        gameObject.SetActive(false);
        Finished?.Invoke(this);
    }
}
