using UnityEngine;

namespace KartGame.KartSystems {

    public class KeyboardInput : BaseInput
    {
        public string TurnInputName = "Horizontal";
        public string AccelerateButtonName = "Accelerate";
        public string BrakeButtonName = "Brake";

        public override InputData GenerateInput() {
            if (PlatformChecker.instance.platform == PlatformChecker.Platform.Editor
                || PlatformChecker.instance.platform == PlatformChecker.Platform.Standalone)
            {
                return new InputData
                {
                    Accelerate = Input.GetButton(AccelerateButtonName),
                    Brake = Input.GetButton(BrakeButtonName),
                    TurnInput = Input.GetAxis("Horizontal")
                };
            }
            else if (PlatformChecker.instance.platform == PlatformChecker.Platform.Mobile)
            {
                return new InputData
                {
                    Accelerate = UIManager_Delivering.instance.joystick_Cp.Vertical > 0f,
                    Brake = UIManager_Delivering.instance.joystick_Cp.Vertical < 0f,
                    TurnInput = UIManager_Delivering.instance.joystick_Cp.Horizontal,
                };
            }
            return new InputData
            {
                
            };
        }
    }
}
