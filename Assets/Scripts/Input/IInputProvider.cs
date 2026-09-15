using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public interface IInputProvider
    {
        /// <summary>
        /// Returns device input with its sensitivity modifier applied. The consumer clamps magnitude to one; it must not normalize sub-unit values.
        /// </summary>
        Vector2 GetInput();

        /// <summary>
        /// Returns speed modifier to be used for this provider
        /// </summary>
        float GetSpeedModifier();
    }
}
