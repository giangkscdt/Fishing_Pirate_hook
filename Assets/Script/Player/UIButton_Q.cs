using UnityEngine;
using UnityEngine.UI;

public class UIButton_Q : MonoBehaviour
{
    public Button button;
    public HookController hookController;

    void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnPress);
    }

    void OnPress()
    {
        // Call the same function hooked when pressing Q
        // Simulate lowering start only if allowed
        if (hookController != null)
        {
            hookController.OnLowerStart();
        }
    }
}
