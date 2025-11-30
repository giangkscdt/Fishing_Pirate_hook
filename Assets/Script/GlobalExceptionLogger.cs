using UnityEngine;
using System;

public class GlobalExceptionLogger : MonoBehaviour
{
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
        if (type == LogType.Exception)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.fff");

            Debug.LogError(
                "[Exception Logged at " + time + "]\n" +
                "Message: " + logString + "\n" +
                "Stack Trace:\n" + stackTrace
            );
        }
    }
}
