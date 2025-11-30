using System.Collections;
using UnityEngine;
using TMPro; // ADD THIS

public class HookController : MonoBehaviour
{
    [Header("Head Rotation Settings")]
    public float rotationSpeed = 1.5f; // adjustable in Inspector
    public float rotationAngle = 10f;  // adjustable in Inspector

    [Header("References")]
    public Transform head;
    public Transform rod;
    public Transform hook;
    public Transform hookEndPos;
    public Transform fishKeepPos;

    public SkillFishing skillFishing;
    public FishingBarUI ui;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float minLength = 0.5f;
    public float maxLength = 5f;
    private float currentLength = 0.5f;

    private bool isLowering = false;
    private bool isHooking = false;

    // NEW: retracting state
    private bool isRetracting = false;

    private GameObject hookedFish;

    private int score = 0;
    public TMP_Text scoreText; // ADD THIS
    public GameObject starPrefab;
    public RectTransform scoreUIPos; // UI target position

    void Start()
    {
        currentLength = minLength;
        UpdateRodLength();

        // Initialize score UI
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    void Update()
    {
        RotateHead();

        if (!isHooking)
            LoweringSystem();
        else
            BattleControl();

        hook.position = hookEndPos.position;

        if (hookedFish != null && isHooking)
        {
            hookedFish.transform.position = hookEndPos.position;
            hookedFish.transform.rotation = hookEndPos.rotation;
        }
    }

    void RotateHead()
    {
        if (head == null) return;

        float angle = Mathf.Sin(Time.time * rotationSpeed) * rotationAngle;
        head.localRotation = Quaternion.Euler(0f, 0f, angle);
    }


    void LoweringSystem()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isLowering && !isRetracting)
            isLowering = true;

        if (isLowering)
        {
            currentLength += moveSpeed * Time.deltaTime;

            if (currentLength >= maxLength)
            {
                currentLength = maxLength;
                isLowering = false;

                if (hookedFish == null)
                    isRetracting = true;
            }

            UpdateRodLength();
        }

        if (isRetracting)
        {
            currentLength -= moveSpeed * Time.deltaTime;

            if (currentLength <= minLength)
            {
                currentLength = minLength;
                isRetracting = false;
            }

            UpdateRodLength();
        }
    }

    void BattleControl()
    {
        if (skillFishing == null) return;

        bool pullingImpulse = Input.GetKeyDown(KeyCode.A);
        var result = skillFishing.TickBattle(pullingImpulse);

        float normDepth = Mathf.Clamp01(skillFishing.depth / skillFishing.maxDepth);
        currentLength = Mathf.Lerp(minLength, maxLength, normDepth);
        UpdateRodLength();

        ui.UpdateTension(
            skillFishing.tension,
            skillFishing.minSafeTension,
            skillFishing.maxSafeTension,
            result,
            skillFishing.isHooked
        );

        if (!skillFishing.isHooked)
        {
            if (result == SkillFishing.BattleResult.Win)
                StartCoroutine(FishCollectAnimation());
            else if (result == SkillFishing.BattleResult.Lose)
                ReleaseFish();
        }
    }

    IEnumerator FishCollectAnimation()
    {
        isHooking = true;
        GameObject fish = hookedFish;

        if (fish == null)
        {
            ResetLine();
            yield break;
        }

        Vector3 start = fish.transform.position;
        Vector3 end = fishKeepPos.position;
        Vector3 controlPoint = start + Vector3.up * 2f;

        float t = 0f;
        Vector3 originalScale = fish.transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime * 1.2f;

            Vector3 p1 = Vector3.Lerp(start, controlPoint, t);
            Vector3 p2 = Vector3.Lerp(controlPoint, end, t);
            fish.transform.position = Vector3.Lerp(p1, p2, t);

            float scaleFactor = Mathf.Lerp(1f, 0f, t);
            fish.transform.localScale = originalScale * scaleFactor;

            yield return null;
        }

        score++;
        if (scoreText != null)
            scoreText.text = score.ToString(); // UPDATE UI SCORE
        for (int i = 0; i < 5; i++)
        {
            GameObject star = Instantiate(starPrefab, hookEndPos.position, Quaternion.identity);
            var fx = star.GetComponent<StarFlyEffect>();
            fx.targetUI = scoreUIPos;
        }

        Debug.Log("Score = " + score);

        if (fish != null)
            Destroy(fish);

        hookedFish = null;
        ResetLine();
    }

    void ReleaseFish()
    {
        if (hookedFish)
        {
            var rb = hookedFish.GetComponent<Rigidbody2D>();
            if (rb) rb.gravityScale = 1f;
        }

        hookedFish = null;
        ResetLine();
    }

    void ResetLine()
    {
        isHooking = false;
        isRetracting = false;
        currentLength = minLength;
        UpdateRodLength();
        ui.Hide();
    }

    void UpdateRodLength()
    {
        Vector3 scale = rod.localScale;
        scale.y = currentLength;
        rod.localScale = scale;
        rod.localPosition = Vector3.zero;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Fish")) return;
        if (!isLowering) return;
        if (isRetracting) return;

        hookedFish = other.gameObject;
        isHooking = true;
        isLowering = false;
        isRetracting = false;

        var fishCtrl = hookedFish.GetComponent<FishController>();
        skillFishing.SetupFishStats(
            fishCtrl.baseStrength,
            fishCtrl.catchSpeed,
            fishCtrl.escapeSpeed
        );

        skillFishing.StartBattle(currentLength);

        var rb = hookedFish.GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;

        hookedFish.transform.position = hookEndPos.position;
        hookedFish.transform.rotation = hookEndPos.rotation;

        ui.Show();
    }
}
