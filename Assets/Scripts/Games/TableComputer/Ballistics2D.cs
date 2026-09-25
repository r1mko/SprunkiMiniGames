using UnityEngine;

public static class Ballistics2D
{
    public static bool TryCalculateLaunchVelocity(Vector2 from, Vector2 to, float angleDegrees, float gravity, out Vector2 velocity)
    {
        velocity = Vector2.zero;

        float g = -gravity;
        float deltaX = to.x - from.x;
        float x = Mathf.Abs(deltaX);
        float y = to.y - from.y;

        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float denominator = 2f * cos * cos * (x * Mathf.Tan(angleRad) - y);
        if (g <= 0f || denominator <= 0f)
        {
            return false;
        }

        float speed = Mathf.Sqrt(g * x * x / denominator);
        float directionX = deltaX >= 0f ? 1f : -1f;
        velocity = new Vector2(cos * directionX, sin) * speed;
        return true;
    }
}
