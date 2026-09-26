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
            var feel = SaileachStudios.Mirlini.Core.GameplayFeel.Load();
            switch (platfromDetector.GetPlatform()) {
                case Platform.mobile_with_touch:
                    return new TouchInputProvider(inputWrapper, feel.TouchMultiplier);
                case Platform.mobile_with_gyro:
                    return new GyroInputProvider(inputWrapper, feel.GyroMultiplier);
                default:
                    return new KeyboardInputProvider(inputWrapper, feel.KeyboardMultiplier);
            }
        }

    }
}