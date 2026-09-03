using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Needle : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 10f;

    [SerializeField] private float obstacleHitDelay = 0.75f;
    [SerializeField] private float wobbleAngle = 15f;
    [SerializeField] private int wobbleCount = 2;
    [SerializeField] private float wobbleSpeed = 1f;

    [SerializeField, Required] private PopBalloonGameManager gameManager;

    private Rigidbody2D _rb;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Coroutine _obstacleHitRoutine;
    private bool _hasWobbled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_hasWobbled)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Obstacle"))
        {
            return;
        }

        _hasWobbled = true;
        Freeze();
        _obstacleHitRoutine = StartCoroutine(ObstacleHitRoutine());
    }

    public void Launch()
    {
        _rb.linearVelocity = Vector2.down * launchSpeed;
    }

    public void ResetNeedle()
    {
        StopObstacleWobble();
        _hasWobbled = false;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    public void Freeze()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }

    public void StopObstacleWobble()
    {
        if (_obstacleHitRoutine != null)
        {
            StopCoroutine(_obstacleHitRoutine);
            _obstacleHitRoutine = null;
        }
    }

    private IEnumerator ObstacleHitRoutine()
    {
        float elapsed = 0f;

        while (elapsed < obstacleHitDelay)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / obstacleHitDelay);
            float angle = Mathf.Sin(t * wobbleCount * 2f * Mathf.PI * wobbleSpeed) * wobbleAngle;
            transform.localRotation = _initialRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        transform.localRotation = _initialRotation;
        _obstacleHitRoutine = null;
        gameManager.OnNeedleMissed();
    }
}
