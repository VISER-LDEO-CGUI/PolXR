// using UnityEngine;

// public class ConsoleToGUI : MonoBehaviour
// {
//     public string output = "";
//     public string stack = "";

//     void OnEnable()
//     {
//         Application.logMessageReceived += HandleLog;
//     }

//     void OnDisable()
//     {
//         Application.logMessageReceived -= HandleLog;
//     }

//     void HandleLog(string logString, string stackTrace, LogType type)
//     {
//         output += logString + " ----- " + stackTrace;
//     }

//     void OnGUI()
//     {
//         GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
//            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
//         GUI.TextArea(new Rect(10, 10, 540, 370), output);
//     }
// }
using UnityEngine;
using UnityEngine.UI;

public class ConsoleToGUI : MonoBehaviour
{
    public Text logText;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logText.text += logString + " ----- " + stackTrace + "\n";
    }
}
