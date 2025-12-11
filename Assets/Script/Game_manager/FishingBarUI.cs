using UnityEngine;
using UnityEngine.UI;

public class FishingBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image progressImage;           // reserved for another parameter (for example green bar)
    public Image backImage;               // BLUE bar that shows tension (0%..100%)
    public RectTransform fishIcon;        // icon that moves along the bar
    public SkillFishing skillFishing;

    private float barHeight;
    private bool isActive = false;

    void Start()
    {
        if (backImage == null)
        {
            Debug.LogError("FishingBarUI: backImage is not assigned.");
            return;
        }

        // progressImage is optional, so no hard error if null

        // Setup fill type for both images (vertical from bottom)
        if (progressImage != null)
            SetupFill(progressImage);

        SetupFill(backImage);

        // Tension bar (blue) starts empty (0%)
        backImage.fillAmount = 0f;
        backImage.color = Color.blue;

        // Optional: prepare progressImage but do not use it yet
        if (progressImage != null)
        {
            progressImage.fillAmount = 0f;
            progressImage.color = Color.green;
        }

        // Cache bar height from the tension bar rect
        barHeight = backImage.rectTransform.rect.height;

        Hide();
    }

    void SetupFill(Image img)
    {
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Vertical;
        img.fillOrigin = (int)Image.OriginVertical.Bottom;
    }

    // tension: current tension value
    // minSafe: tension value at 0% of bar (for example -10)
    // maxSafe: tension value at 100% of bar (for example +10)
    // New method: update fish strength and move icon top-to-bottom
    public void UpdateFishStrength(float strength, float maxStrength)
    {
        if (!isActive || fishIcon == null)
            return;

        float sNorm = Mathf.InverseLerp(0f, maxStrength, strength);
        sNorm = Mathf.Clamp01(sNorm);

        float yPos = sNorm * barHeight;

        Vector2 anchored = fishIcon.anchoredPosition;
        anchored.y = yPos;
        fishIcon.anchoredPosition = anchored;
    }


    public void UpdateTension(
        float tension,
        float minSafe,
        float maxSafe,
        SkillFishing.BattleResult state,
        bool isHooked)
    {
        if (!isActive || backImage == null)
            return;

        // Map tension -> 0..1
        // tension == minSafe -> 0
        // tension == maxSafe -> 1
        float tNorm = Mathf.InverseLerp(minSafe, maxSafe, tension);
        tNorm = Mathf.Clamp01(tNorm);

        // When the battle is finished and hook is released,
        // snap the bar to top/bottom depending on result.
        if (!isHooked)
        {
            if (state == SkillFishing.BattleResult.Win)
                tNorm = 1f;
            else if (state == SkillFishing.BattleResult.Lose)
                tNorm = 0f;
        }

        // Apply to BLUE tension bar
        backImage.fillAmount = tNorm;

        // Move fish icon along the bar height (bottom -> top)
        //if (fishIcon != null)
        //{
        //    Vector2 anchored = fishIcon.anchoredPosition;
        //    anchored.y = tNorm * barHeight;
        //    fishIcon.anchoredPosition = anchored;
        //}

        // Color gradient for tension:
        // low = blue, medium = green/yellow, high = red
        if (tNorm < 0.25f)
        {
            // 0..0.25: blue to green
            backImage.color = Color.Lerp(Color.blue, Color.green, tNorm * 4f);
        }
        else if (tNorm < 0.5f)
        {
            // 0.25..0.5: solid green
            backImage.color = Color.green;
        }
        else if (tNorm < 0.75f)
        {
            // 0.5..0.75: green to yellow
            backImage.color = Color.Lerp(Color.green, Color.yellow, (tNorm - 0.5f) * 4f);
        }
        else
        {
            // 0.75..1.0: yellow to red
            backImage.color = Color.Lerp(Color.yellow, Color.red, (tNorm - 0.75f) * 4f);
        }

        // NOTE: progressImage is not changed here.
        // You can use it later for another parameter.
    }


    public void Show()
    {
        gameObject.SetActive(true);
        isActive = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        isActive = false;
    }
}
