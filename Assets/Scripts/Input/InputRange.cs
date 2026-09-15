using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public static class InputRange
    {
        // Providers apply their device multiplier first. Preserve sub-unit strength.
        public static Vector2 Clamp(Vector2 input) {
            if (float.IsNaN(input.x) || float.IsNaN(input.y) || float.IsInfinity(input.x) || float.IsInfinity(input.y))
                return Vector2.zero;
            return Vector2.ClampMagnitude(input, 1f);
        }
    }
}
