using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

public class MockPlatformDetector : IPlatformDetector
{
    private Platform platform = Platform.Not_found;

    public MockPlatformDetector(Platform platformToDetect) {
        platform = platformToDetect;
    }

    public Platform GetPlatform() {
        return platform;
    }
}
