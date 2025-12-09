using UnityEngine;
using System.Reflection;

public class MissingReferenceFinder : MonoBehaviour
{
    public bool autoScan = true;

    void Start()
    {
        if (autoScan)
            ScanAllComponents();
    }

    [ContextMenu("Scan All Components Now")]
    public void ScanAllComponents()
    {
        MonoBehaviour[] list = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour mb in list)
        {
            if (mb != null)
                CheckScriptFields(mb);
        }

        Debug.Log("Scan completed: MissingReferenceFinder");
    }

    void CheckScriptFields(MonoBehaviour script)
    {
        FieldInfo[] fields = script.GetType().GetFields(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // Only check serialized fields
            bool isSerialized =
                field.IsPublic ||
                field.GetCustomAttribute<SerializeField>() != null;

            if (!isSerialized)
                continue;

            // Only UnityEngine.Object fields (includes prefabs, components, etc.)
            if (!typeof(Object).IsAssignableFrom(field.FieldType))
                continue;

            Object obj = field.GetValue(script) as Object;

            if (obj == null)
            {
                Debug.LogError("Missing Reference Detected!" +
                    "\nGameObject: " + script.gameObject.name +
                    "\nScript: " + script.GetType().Name +
                    "\nField: " + field.Name,
                    script.gameObject);
            }
        }
    }
}
