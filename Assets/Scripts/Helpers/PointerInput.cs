using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class PointerInput
{
    public static bool WasPressedThisFrame()
    {
        return TryGetPressedThisFrame(out _);
    }

    public static bool TryGetPressedThisFrame(out Vector2 screenPosition)
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null)
        {
            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    screenPosition = touch.position.ReadValue();
                    return true;
                }
            }
        }

        screenPosition = default;
        return false;
    }

    public static bool WasReleasedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current == null)
        {
            return false;
        }

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (touch.press.wasReleasedThisFrame)
            {
                return true;
            }
        }

        return false;
    }
}
