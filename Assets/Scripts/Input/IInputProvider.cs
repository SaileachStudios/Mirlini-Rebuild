using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public interface IInputProvider
    {
        /// <summary>
        /// Returns a normalized 2D input vector (e.g., from keyboard or gyro).
        /// </summary>
        Vector2 GetInput();

        /// <summary>
        /// Returns speed modifier to be used for this provider
        /// </summary>
        float GetSpeedModifier();
    }
}