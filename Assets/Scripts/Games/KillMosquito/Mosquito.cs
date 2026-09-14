using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Mosquito : MonoBehaviour
{
    [SerializeField, Required] private SpriteRenderer spriteRenderer;
    [SerializeField, Required] private Sprite aliveSprite;
    [SerializeField, Required] private Sprite deadSprite;

    [SerializeField, Required] private KillMosquitoGameManager gameManager;

    [SerializeField] private float xOffset = 1f;
    [SerializeField] private float flySpeed = 1f;
    [SerializeField] private bool startMovingRight;

    public bool IsDead { get; private set; }

    private Vector3 _initialLocalPosition;
    private float _initialAbsScaleX;
    private bool _movingRight;
    private Coroutine _flyRoutine;

    private void Awake()
    {
        _initialLocalPosition = transform.localPosition;
        _initialAbsScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead || !other.TryGetComponent<MosquitoNeedle>(out _))
        {
            return;
        }

        Kill();
        gameManager.OnMosquitoHit();
    }

    public void StartFlying()
    {
        StopFlying();
        _movingRight = startMovingRight;
        SetFacing(_movingRight);
        _flyRoutine = StartCoroutine(FlyRoutine());
    }

    public void ResetMosquito()
    {
        StopFlying();
        transform.localPosition = _initialLocalPosition;
        spriteRenderer.sprite = aliveSprite;
        IsDead = false;
    }

    public void Kill()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        StopFlying();
        spriteRenderer.sprite = deadSprite;
    }

    private void StopFlying()
    {
        if (_flyRoutine != null)
        {
            StopCoroutine(_flyRoutine);
            _flyRoutine = null;
        }
    }

    private IEnumerator FlyRoutine()
    {
        float leftX = -xOffset;
        float rightX = xOffset;

        while (true)
        {
            float targetX = _movingRight ? rightX : leftX;

            while (Mathf.Abs(transform.localPosition.x - targetX) > 0.01f)
            {
                Vector3 position = transform.localPosition;
                position.x = Mathf.MoveTowards(position.x, targetX, flySpeed * Time.deltaTime);
                transform.localPosition = position;
                yield return null;
            }

            _movingRight = !_movingRight;
            SetFacing(_movingRight);
        }
    }

    private void SetFacing(bool movingRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = movingRight ? -_initialAbsScaleX : _initialAbsScaleX;
        transform.localScale = scale;
    }
}
