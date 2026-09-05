using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public enum BlockType
{
    Ice,
    Tree,
}

[RequireComponent(typeof(Rigidbody2D))]
public class Block : MonoBehaviour
{
    [SerializeField] private BlockType blockType;
    [SerializeField] private float flySpeed = 5f;
    [SerializeField] private float iceReportDelay = 0.25f;
    [SerializeField] private float disableDelay = 5f;

    [SerializeField, Required] private KnockIceGameManager gameManager;

    public BlockType Type => blockType;

    private Rigidbody2D _rb;
    private RigidbodyType2D _initialBodyType;
    private RigidbodyConstraints2D _initialConstraints;
    private Vector2 _initialPosition;
    private Quaternion _initialRotation;
    private Coroutine _knockedRoutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _initialBodyType = _rb.bodyType;
        _initialConstraints = _rb.constraints | RigidbodyConstraints2D.FreezePositionX;
        _initialPosition = _rb.position;
        _initialRotation = transform.rotation;

        _rb.constraints = _initialConstraints;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_knockedRoutine != null)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Spoon"))
        {
            return;
        }

        _rb.bodyType = RigidbodyType2D.Kinematic;

        if (blockType == BlockType.Tree)
        {
            gameManager.OnTreeKnocked();
        }

        _knockedRoutine = StartCoroutine(KnockedRoutine());
    }

    public void ResetBlock()
    {
        StopFlight();
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        _rb.bodyType = _initialBodyType;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.constraints = _initialConstraints;
        _rb.position = _initialPosition;
        transform.rotation = _initialRotation;

    }

    private void StopFlight()
    {
        if (_knockedRoutine != null)
        {
            StopCoroutine(_knockedRoutine);
            _knockedRoutine = null;
        }
    }

    private IEnumerator KnockedRoutine()
    {
        float elapsed = 0f;
        bool iceReported = blockType != BlockType.Ice;

        while (elapsed < disableDelay)
        {
            transform.position += Vector3.left * (flySpeed * Time.deltaTime);
            elapsed += Time.deltaTime;

            if (!iceReported && elapsed >= iceReportDelay)
            {
                iceReported = true;
                gameManager.OnIceKnocked();
            }

            yield return null;
        }

        _knockedRoutine = null;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        gameObject.SetActive(false);
    }
}
