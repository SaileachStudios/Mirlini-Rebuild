using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem { 
    public class PlatformDetector : IPlatformDetector
    {
        private static Platform platform = Platform.Not_found;

        public Platform GetPlatform() {
            if (platform == Platform.Not_found) {
                if(Application.platform == RuntimePlatform.Android) {
                    platform = SystemInfo.supportsGyroscope ? Platform.mobile_with_gyro : Platform.mobile_with_touch;
                }
                else {
                    platform = Platform.windows;
                }
            }

            return platform;
        }
    }
}