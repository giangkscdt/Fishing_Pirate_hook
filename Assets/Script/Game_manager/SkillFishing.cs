using UnityEngine;

public class SkillFishing : MonoBehaviour
{
    [Header("Player / Reel")]
    public float reelForceMultiplier = 3.0f;   // how much each tap adds
    public float maxSafeTension = 10f;
    public float minSafeTension = -10f;

    [Header("Runtime Debug")]
    public float playerForce;
    public float fishForce;
    public float tension;
    public float depth;
    public float reelMomentum;                // NEW: build up from taps

    [HideInInspector] public float baseStrength;
    [HideInInspector] public float catchSpeed;
    [HideInInspector] public float escapeSpeed;
    [HideInInspector] public float maxDepth = 10f;

    private float hookTime = 0f;
    public bool isHooked = false;

    public void SetupFishStats(float strength, float catchS, float escapeS)
    {
        baseStrength = Mathf.Max(0.1f, strength);
        catchSpeed = Mathf.Max(0.1f, catchS);
        escapeSpeed = Mathf.Max(0.1f, escapeS);
        maxDepth = 10f;
    }

    public void StartBattle(float initialDepth)
    {
        hookTime = 0f;
        depth = Mathf.Clamp(initialDepth, 0f, maxDepth);
        reelMomentum = 0f;     // reset momentum at start
        isHooked = true;
    }

    public enum BattleResult { None, Win, Lose }

    // pullingImpulse = true ONLY on the frame A is pressed (GetKeyDown)
    public BattleResult TickBattle(bool pullingImpulse)
    {
        if (!isHooked)
            return BattleResult.None;

        hookTime += Time.deltaTime;

        // 1) Build reel momentum from taps (weaker boost)
        if (pullingImpulse)
        {
            reelMomentum += reelForceMultiplier * 0.5f;
        }

        // 2) Momentum decays faster to force fast tapping
        float reelDecay = 3.0f;
        reelMomentum = Mathf.Max(0f, reelMomentum - reelDecay * Time.deltaTime);

        // 3) Player force is based on limited momentum
        playerForce = Mathf.Clamp(reelMomentum, 0f, maxSafeTension * 0.8f);

        // 4) Fish force same as before
        fishForce = ComputeFishForce(hookTime);
        tension = playerForce - fishForce;

        // 5) Depth rules (no tapping = ALWAYS fish advantage)
        if (playerForce <= 0.1f)
        {
            depth += escapeSpeed * Time.deltaTime;
        }
        else if (tension >= minSafeTension)
        {
            depth -= catchSpeed * Time.deltaTime;
        }
        else
        {
            depth += escapeSpeed * Time.deltaTime;
        }

        // 6) Win/Lose check same as your original


        // 6) Depth logic (who is winning)
        // If player is NOT tapping -> always lose depth
        if (playerForce <= 0.01f)
        {
            depth += escapeSpeed * Time.deltaTime;
        }
        else
        {
            // Now tension matters:
            if (tension >= minSafeTension)
                depth -= catchSpeed * Time.deltaTime;
            else
                depth += escapeSpeed * Time.deltaTime;
        }


        // 7) Check win / lose conditions
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

    float ComputeFishForce(float t)
    {
        float s = baseStrength;

        if (t < 1f) return s * 1.0f;  // start
        if (t < 3f) return s * 0.8f;  // easier window
        if (t < 5f) return s * 1.1f;  // small spike
        return s * 0.5f;              // tired at the end
    }
}
