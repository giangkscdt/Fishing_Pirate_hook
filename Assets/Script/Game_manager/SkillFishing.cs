using UnityEngine;

public class SkillFishing : MonoBehaviour
{
    [Header("Player / Reel")]
    public float reelForceMultiplier = 1.0f;
    public float maxSafeTension = 10f;
    public float minSafeTension = -10f;

    [Header("Fish Stats")]
    public float baseStrength = 8f;
    public float maxDepth = 10f;
    public float catchSpeed = 2f;
    public float escapeSpeed = 3f;

    [Header("Runtime Debug")]
    public float reelSpeed;
    public float playerForce;
    public float fishForce;
    public float tension;
    public float depth;

    private float hookTime = 0f;
    public bool isHooked = false;

    void Start()
    {
        ResetFish();
    }

    void ResetFish()
    {
        depth = maxDepth * 0.7f;
        hookTime = 0f;
        isHooked = true;
    }

    void Update()
    {
        if (!isHooked)
            return;

        hookTime += Time.deltaTime;

        float input = Input.GetAxis("Vertical");
        reelSpeed = Mathf.Max(0f, input);

        playerForce = reelSpeed * reelForceMultiplier;

        fishForce = ComputeFishForce(hookTime);

        tension = playerForce - fishForce;

        if (tension > maxSafeTension)
        {
            Debug.Log("Line broke! Too much force.");
            isHooked = false;
            return;
        }
        else if (tension < minSafeTension)
        {
            depth += escapeSpeed * Time.deltaTime;
        }
        else
        {
            depth -= catchSpeed * Time.deltaTime;
        }

        if (depth <= 0f)
        {
            Debug.Log("Fish caught! Skill success.");
            isHooked = false;
        }
        else if (depth >= maxDepth)
        {
            Debug.Log("Fish escaped...");
            isHooked = false;
        }
    }

    float ComputeFishForce(float t)
    {
        float s = baseStrength;

        if (t < 1f) return s * 1.2f;
        if (t < 3f) return s * 0.9f;
        if (t < 5f) return s * 1.3f;
        return s * 0.6f;
    }

    // ==================================================================
    // === ADDED FUNCTIONS BELOW - do not remove original code above ===
    // ==================================================================

    // HookController calls this when battle starts
    public void StartBattle()
    {
        hookTime = 0f;
        depth = maxDepth * 0.7f;
        isHooked = true;
    }

    // HookController calls this when battle ends (Win or Lose)
    public void EndBattle()
    {
        isHooked = false;
    }

    public float GetNormalizedDepth()
    {
        // 1.0 = close to surface (win)
        // 0.0 = deep underwater (lose)
        return 1f - Mathf.Clamp01(depth / maxDepth);
    }

    // Compare HookController request
    public enum BattleResult { None, Win, Lose }

    public BattleResult TickBattle(bool pullingUp)
    {
        if (!isHooked)
            return BattleResult.None;

        hookTime += Time.deltaTime;

        playerForce = pullingUp ? reelForceMultiplier : 0f;
        fishForce = ComputeFishForce(hookTime);

        tension = playerForce - fishForce;

        if (tension > maxSafeTension)
        {
            isHooked = false;
            return BattleResult.Lose;
        }
        else if (tension < minSafeTension)
        {
            depth += escapeSpeed * Time.deltaTime;
        }
        else
        {
            depth -= catchSpeed * Time.deltaTime;
        }

        if (depth <= 0f)
        {
            isHooked = false;
            return BattleResult.Win;
        }
        else if (depth >= maxDepth)
        {
            isHooked = false;
            return BattleResult.Lose;
        }

        return BattleResult.None;
    }
}
