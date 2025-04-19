using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class InputProviderFactory
    {
        private IPlatformDetector platfromDetector;
        private IInputWrapper inputWrapper;

        public InputProviderFactory(IPlatformDetector detector, IInputWrapper wrapper) {
            platfromDetector = detector;
            inputWrapper = wrapper;
        }

        public IInputProvider Create() {
            switch (platfromDetector.GetPlatform()) {
                case Platform.mobile_with_touch:
                    return new TouchInputProvider(inputWrapper, 0.2f);
                case Platform.mobile_with_gyro:
                    return new GyroInputProvider(inputWrapper, 2f);
                default:
                    return new KeyboardInputProvider(inputWrapper, 100f);
            }
        }

    }
}