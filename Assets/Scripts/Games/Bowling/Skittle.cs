using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class Skittle : MonoBehaviour
{
    [SerializeField, Required] private BowlingGameManager gameManager;
    [SerializeField] private float knockRotationX = 60f;
    [SerializeField] private float knockDuration = 0.3f;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Coroutine _knockRoutine;
    private bool _isKnocked;

    private void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public void ResetSkittle()
    {
        if (_knockRoutine != null)
        {
            StopCoroutine(_knockRoutine);
            _knockRoutine = null;
        }

        _isKnocked = false;
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isKnocked || !other.CompareTag("Ball"))
        {
            return;
        }

        _isKnocked = true;
        _knockRoutine = StartCoroutine(KnockRoutine());

        gameManager.OnSkittleKnocked();
    }

    private IEnumerator KnockRoutine()
    {
        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.Euler(knockRotationX, 0f, 0f);
        float elapsed = 0f;

        while (elapsed < knockDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / knockDuration);
            transform.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }

        transform.rotation = to;
        _knockRoutine = null;
    }
}
