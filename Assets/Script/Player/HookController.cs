using System.Collections;
using UnityEngine;

public class HookController : MonoBehaviour
{
    [Header("References")]
    public Transform head;
    public Transform rod;
    public Transform hook;
    public Transform hookEndPos;
    public Transform fishKeepPos;

    // === ADDED ===
    public SkillFishing skillFishing;
    public SkillFishingUI ui;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float minLength = 0.5f;
    public float maxLength = 5f;
    private float currentLength = 0.5f;

    private bool isLowering = false;
    private bool isRetracting = false;
    private bool reachedBottom = false;

    private bool isHooking = false;
    private GameObject hookedFish;

    private int score = 0;
    private bool isCollected = false;

    void Start()
    {
        currentLength = minLength;
        UpdateRodLength();
    }

    void Update()
    {
        RotateHead();

        if (!isHooking)
        {
            LoweringSystem();
        }
        else
        {
            BattleControl(); // now fully SkillFishing based
        }

        hook.position = hookEndPos.position;

        if (hookedFish != null && isHooking)
        {
            hookedFish.transform.position = hookEndPos.position;
            hookedFish.transform.rotation = hookEndPos.rotation;
        }
    }

    void RotateHead()
    {
        head.Rotate(0f, 0f, Mathf.Sin(Time.time * 1.5f) * 0.2f);
    }

    void LoweringSystem()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isLowering && !isRetracting)
        {
            isLowering = true;
            reachedBottom = false;
        }

        if (isLowering && !reachedBottom)
        {
            currentLength += moveSpeed * Time.deltaTime;

            if (currentLength >= maxLength)
            {
                currentLength = maxLength;
                reachedBottom = true;
                isLowering = false;
                isRetracting = true;
            }
        }
        else if (isRetracting)
        {
            currentLength -= moveSpeed * Time.deltaTime;

            if (currentLength <= minLength)
            {
                currentLength = minLength;
                ResetLoweringStates();
            }
        }

        UpdateRodLength();
    }

    void ResetLoweringStates()
    {
        isLowering = false;
        isRetracting = false;
        reachedBottom = false;
    }

    // ==========================================================
    // === REPLACED BattleControl() - SkillFishing takes control
    // ==========================================================
    void BattleControl()
    {
        bool pullingUp = Input.GetKey(KeyCode.A);

        var result = skillFishing.TickBattle(pullingUp);

        ui.UpdateBar(skillFishing.GetNormalizedDepth());

        float normDepth = Mathf.Clamp01(skillFishing.depth / skillFishing.maxDepth);
        currentLength = Mathf.Lerp(minLength, maxLength, normDepth);
        UpdateRodLength();

        if (!skillFishing.isHooked)
        {
            if (result == SkillFishing.BattleResult.Win)
            {
                Debug.Log("Player caught the fish!");
                skillFishing.EndBattle();
                ui.Hide();
                isHooking = false;
                StartCoroutine(FishCollectAnimation());
            }
            else if (result == SkillFishing.BattleResult.Lose)
            {
                Debug.Log("Fish escaped!");
                skillFishing.EndBattle();
                ui.Hide();
                ReleaseFish();
            }
        }
    }

    IEnumerator FishCollectAnimation()
    {
        isHooking = false;
        GameObject fish = hookedFish;

        if (fish == null)
        {
            isCollected = false;
            ResetLoweringStates();
            yield break;
        }

        Vector3 start = fish.transform.position;
        Vector3 end = fishKeepPos.position;
        Vector3 controlPoint = start + Vector3.up * 2f;

        float t = 0f;
        Vector3 originalScale = fish.transform.localScale;

        while (t < 1f)
        {
            if (fish == null)
            {
                isCollected = false;
                ResetLoweringStates();
                yield break;
            }

            t += Time.deltaTime * 1.2f;

            Vector3 p1 = Vector3.Lerp(start, controlPoint, t);
            Vector3 p2 = Vector3.Lerp(controlPoint, end, t);
            fish.transform.position = Vector3.Lerp(p1, p2, t);

            float scaleFactor = Mathf.Lerp(1f, 0f, t);
            fish.transform.localScale = originalScale * scaleFactor;

            yield return null;
        }

        score++;
        Debug.Log("Score = " + score);

        if (fish != null)
        {
            Destroy(fish);
        }

        hookedFish = null;
        isCollected = false;
        ResetLoweringStates();
    }

    void UpdateRodLength()
    {
        Vector3 scale = rod.localScale;
        scale.y = currentLength;
        rod.localScale = scale;
        rod.localPosition = Vector3.zero;
    }

    void ReleaseFish()
    {
        if (hookedFish)
        {
            var rb = hookedFish.GetComponent<Rigidbody2D>();
            if (rb) rb.gravityScale = 1f;
        }

        hookedFish = null;
        isHooking = false;
        isCollected = false;

        isLowering = false;
        isRetracting = true;
        reachedBottom = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Fish")) return;
        if (isHooking) return;
        if (!isLowering) return;

        hookedFish = other.gameObject;
        isHooking = true;

        var rb = hookedFish.GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;

        hookedFish.transform.position = hookEndPos.position;
        hookedFish.transform.rotation = hookEndPos.rotation;

        // === ADDED ===
        skillFishing.StartBattle();
        ui.Show();

        Debug.Log("Fish hooked! Skill battle start!");
    }
}
