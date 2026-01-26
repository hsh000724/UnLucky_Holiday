using UnityEngine;
using UnityEngine.UI;

public class JoystickSettingsManager : MonoBehaviour
{
    public Dropdown joystickTypeDropdown;

    private const string JoystickTypeKey = "JoystickType";

    void Start()
    {
        joystickTypeDropdown.ClearOptions();
        joystickTypeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Fixed", "Floating", "Dynamic" });

        int savedType = PlayerPrefs.GetInt(JoystickTypeKey, 0);
        joystickTypeDropdown.value = savedType;
        joystickTypeDropdown.onValueChanged.AddListener(OnJoystickTypeChanged);
    }

    public void OnJoystickTypeChanged(int index)
    {
        PlayerPrefs.SetInt(JoystickTypeKey, index);
        PlayerPrefs.Save();
        Debug.Log($"[JoystickSettings] 조이스틱 타입 저장됨: {(JoystickType)index}");
    }
}
