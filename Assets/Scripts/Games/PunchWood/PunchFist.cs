using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PunchFist : MonoBehaviour
{
    [SerializeField, Tag] private string obstacleTag = "Obstacle";

    private Rigidbody2D _rb;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float _initialGravityScale;
    private bool _isFlying;
    private Coroutine _flightRoutine;

    public event Action HitObstacle;
    public event Action Landed;

    public float Gravity => Mathf.Abs(Physics2D.gravity.y * _initialGravityScale);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialGravityScale = _rb.gravityScale;
        _rb.gravityScale = 0f;
    }

    private void OnDisable()
    {
        _flightRoutine = null;
        _isFlying = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isFlying || !other.CompareTag(obstacleTag))
        {
            return;
        }

        StopFlightRoutine();
        Freeze();
        HitObstacle?.Invoke();
    }

    public void Launch(float speed)
    {
        _isFlying = true;
        _rb.gravityScale = _initialGravityScale;
        _rb.linearVelocity = Vector2.up * speed;
        _flightRoutine = StartCoroutine(FlightRoutine());
    }

    public void ResetFist()
    {
        StopFlightRoutine();
        Freeze();
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    private IEnumerator FlightRoutine()
    {
        yield return null;

        while (_rb.linearVelocity.y > 0f || transform.position.y > _initialPosition.y)
        {
            yield return null;
        }

        Freeze();
        transform.position = _initialPosition;
        _flightRoutine = null;
        Landed?.Invoke();
    }

    private void Freeze()
    {
        _isFlying = false;
        _rb.gravityScale = 0f;
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
}
