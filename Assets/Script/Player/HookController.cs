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

    void Start()
    {
        currentLength = minLength;
        UpdateRodLength();
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
        head.Rotate(0f, 0f, Mathf.Sin(Time.time * 1.5f) * 0.2f);
    }

    void LoweringSystem()
    {
        // Start lowering only if not lowering or retracting
        if (Input.GetKeyDown(KeyCode.Q) && !isLowering && !isRetracting)
            isLowering = true;

        // Lower rope
        if (isLowering)
        {
            currentLength += moveSpeed * Time.deltaTime;

            if (currentLength >= maxLength)
            {
                currentLength = maxLength;
                isLowering = false;

                // Auto retract if bottom reached and no fish hooked
                if (hookedFish == null)
                    isRetracting = true;
            }

            UpdateRodLength();
        }

        // Retract rope up
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
        isRetracting = false; // ensure reset to normal state
        currentLength = minLength;
        UpdateRodLength();
        ui.Hide(); // hide tension bar after battle ends
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
        if (isRetracting) return; // Do not hook fish while retracting

        hookedFish = other.gameObject;
        isHooking = true;
        isLowering = false;
        isRetracting = false; // stop retracting since we got a fish

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
