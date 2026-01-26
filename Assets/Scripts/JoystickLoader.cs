using UnityEngine;

public class JoystickLoader : MonoBehaviour
{
    public VariableJoystick variableJoystick;
    private const string JoystickTypeKey = "JoystickType";

    void Start()
    {
        int savedType = PlayerPrefs.GetInt(JoystickTypeKey, 0);
        JoystickType joystickType = (JoystickType)savedType;
        variableJoystick.SetMode(joystickType);

        Debug.Log($"[JoystickLoader] 저장된 조이스틱 타입 적용됨: {joystickType}");
    }
}
