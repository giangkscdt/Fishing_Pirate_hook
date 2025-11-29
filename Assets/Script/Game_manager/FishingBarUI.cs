using UnityEngine;
using UnityEngine.UI;

public class FishingBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image progressImage;           // dynamic: tension color and height
    public Image backImage;               // static: blue tension range background
    public RectTransform fishIcon;
    public SkillFishing skillFishing;

    private float barHeight;
    private bool isActive = false;

    void Start()
    {
        if (progressImage == null || backImage == null)
        {
            Debug.LogError("FishingBarUI missing image references!");
            return;
        }

        // Setup fill
        SetupFill(progressImage);
        SetupFill(backImage);

        // Blue background always 100%
        backImage.fillAmount = 1f;
        backImage.color = Color.blue;

        barHeight = progressImage.rectTransform.rect.height;

        Hide();
    }

    void SetupFill(Image img)
    {
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Vertical;
        img.fillOrigin = (int)Image.OriginVertical.Bottom;
    }

    public void UpdateTension(
        float tension,
        float minSafe,
        float maxSafe,
        SkillFishing.BattleResult state,
        bool isHooked)
    {
        if (!isActive) return;

        float tNorm = Mathf.InverseLerp(minSafe, maxSafe, tension);
        tNorm = Mathf.Clamp01(tNorm); // 0..1

        // After battle done: force top/bottom
        if (!isHooked)
        {
            if (state == SkillFishing.BattleResult.Win)
                tNorm = 1f;
            else if (state == SkillFishing.BattleResult.Lose)
                tNorm = 0f;
        }

        // Fill green/yellow/red bar
        progressImage.fillAmount = tNorm;

        // Move fish icon
        Vector2 anchored = fishIcon.anchoredPosition;
        anchored.y = tNorm * barHeight;
        fishIcon.anchoredPosition = anchored;

        // Color gradient Blue -> Green -> Yellow -> Red
        if (tNorm < 0.25f)
            progressImage.color = Color.Lerp(Color.blue, Color.green, tNorm * 4f);
        else if (tNorm < 0.5f)
            progressImage.color = Color.green;
        else if (tNorm < 0.75f)
            progressImage.color = Color.Lerp(Color.green, Color.yellow, (tNorm - 0.5f) * 4f);
        else
            progressImage.color = Color.Lerp(Color.yellow, Color.red, (tNorm - 0.75f) * 4f);
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
