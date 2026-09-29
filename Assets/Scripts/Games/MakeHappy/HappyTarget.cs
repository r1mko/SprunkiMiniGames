using System;
using UnityEngine;

[Serializable]
public struct HappyTarget
{
    public float fromAngle;
    public float toAngle;
    public SpriteRenderer sadFace;
    public SpriteRenderer happyFace;

    public bool Contains(float angle)
    {
        float from = Mathf.Repeat(fromAngle, 360f);
        float to = Mathf.Repeat(toAngle, 360f);

        return from <= to
            ? angle >= from && angle <= to
            : angle >= from || angle <= to;
    }

    public void SetHappy(bool isHappy)
    {
        sadFace.enabled = !isHappy;
        happyFace.enabled = isHappy;
    }
}
