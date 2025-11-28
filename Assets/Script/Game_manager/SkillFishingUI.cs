using UnityEngine;
using UnityEngine.UI;

public class SkillFishingUI : MonoBehaviour
{
    public Slider bar;

    // Show bar when battle starts
    public void Show()
    {
        if (bar != null)
            bar.gameObject.SetActive(true);
    }

    // Hide bar when battle ends
    public void Hide()
    {
        if (bar != null)
            bar.gameObject.SetActive(false);
    }

    // Update progress fill (0 to 1)
    public void UpdateBar(float value)
    {
        if (bar != null)
            bar.value = Mathf.Clamp01(value);
    }
}
