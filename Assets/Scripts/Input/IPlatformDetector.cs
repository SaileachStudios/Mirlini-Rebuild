using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public enum Platform
    {
        windows,
        mobile_with_gyro,
        mobile_with_touch,
        Not_found
    }

    public interface IPlatformDetector
    {
        Platform GetPlatform();
    }
}