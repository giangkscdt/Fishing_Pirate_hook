using UnityEngine;
using UnityEngine.UI;

public class FishingBarUI : MonoBehaviour
{
    public SkillFishing skillFishing;

    [Header("UI Elements")]
    public RectTransform barArea;      // FishingBar
    public RectTransform hookZone;     // Green moving zone
    public RectTransform fishIcon;     // Moves with fish
    public Image progressFill;         // Vertical catch progress

    [Header("Control Settings")]
    public float hookMoveSpeed = 300f; // Upward speed
    public float fallSpeed = 200f;     // Gravity feel

    private float barHeight;

    void Start()
    {
        barHeight = barArea.rect.height;
    }

    void Update()
    {
        if (!skillFishing.isHooked) return;

        // 1. HookZone movement: only A presses move upward
        Vector2 hz = hookZone.anchoredPosition;
        if (Input.GetKey(KeyCode.A))
            hz.y += hookMoveSpeed * Time.deltaTime;
        else
            hz.y -= fallSpeed * Time.deltaTime;
        hz.y = Mathf.Clamp(hz.y, 0, barHeight);
        hookZone.anchoredPosition = hz;

        // 2. FishIcon vertical follows depth
        float normDepth = Mathf.Clamp01(skillFishing.depth / skillFishing.maxDepth);
        float fishY = barHeight * (1f - normDepth);
        fishIcon.anchoredPosition = new Vector2(fishIcon.anchoredPosition.x, fishY);

        // 3. Catch progress fill
        progressFill.fillAmount = skillFishing.GetNormalizedDepth();
    }
}
