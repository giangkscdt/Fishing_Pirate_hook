using UnityEngine;

public class SkillFishing : MonoBehaviour
{
    [Header("Player / Reel")]
    public float reelForceMultiplier = 5.0f;   // how much each tap adds
    public float maxSafeTension = 10f;         // line break threshold
    public float minSafeTension = -10f;        // slack threshold

    [Header("Runtime Debug")]
    public float playerForce;
    public float fishForce;
    public float tension;
    public float depth;
    public float reelMomentum;                // build up from taps

    [HideInInspector] public float baseStrength;
    [HideInInspector] public float catchSpeed;
    [HideInInspector] public float escapeSpeed;
    [HideInInspector] public float maxDepth = 10f;

    private float hookTime = 0f;
    public bool isHooked = false;
    public System.Action OnLineBreak;
    // New variables you must add inside SkillFishing:
    public float fishMaxHealth = 100f;   // fish starting health
    public float fishHealth = 100f;      // runtime health



    public void SetupFishStats(float strength, float catchS, float escapeS)
    {
        baseStrength = Mathf.Max(0.1f, strength);
        catchSpeed = Mathf.Max(0.1f, catchS);
        escapeSpeed = Mathf.Max(0.1f, escapeS);
        maxDepth = 10f;
    }

    public void StartBattle(float initialDepth, FishController fishCtrl)
    {
        hookTime = 0f;

        depth = Mathf.Clamp(initialDepth, 0f, maxDepth);
        reelMomentum = 0f;

        isHooked = true;
        fishHealth = fishMaxHealth; // reset HP
        // Call the actual animation trigger function
        if (fishCtrl != null)
            fishCtrl.Hooked();

    }



    public enum BattleResult { None, Win, Lose }

    // pullingImpulse = true on the exact frame A is pressed (GetKeyDown)
    public BattleResult TickBattle(bool pullingImpulse)
    {
        if (!isHooked)
            return BattleResult.None;

        hookTime += Time.deltaTime;

        // 1) Tapping adds momentum
        if (pullingImpulse)
        {
            reelMomentum += reelForceMultiplier * 0.9f;
        }

        // 2) Momentum decays quickly
        float reelDecay = 2.0f;
        reelMomentum = Mathf.Max(0f, reelMomentum - reelDecay * Time.deltaTime);

        // 3) Player force can exceed the safe tension a little.
        // This allows the player to break the line by pulling too much.
        playerForce = Mathf.Clamp(reelMomentum, 0f, maxSafeTension * 1.2f);


        // 4) Fish force
        fishForce = ComputeFishForce(hookTime);
        // Combined tension model: both fish force and player force stress the line.
        // Using Abs makes sure direction does not matter.
        tension = Mathf.Abs(playerForce + fishForce);


        // >>> NEW: Instant lose if tension exceeds max safe tension <<<
        if (tension > maxSafeTension)
        {
            isHooked = false;

            // Fire line break event
            if (OnLineBreak != null)
                OnLineBreak();

            return BattleResult.Lose;
        }



        // 5) Depth update rules
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

        // === Apply damage to fish if player pulls correctly ===
        // pullingImpulse means player pressed A
        // tension inside safe range means correct pull

        if (pullingImpulse && tension > minSafeTension && tension < maxSafeTension)
        {
            float dmg = catchSpeed * 8f;
            fishHealth -= dmg;

            // Check if fish is defeated
            if (fishHealth <= 0f)
            {
                fishHealth = 0f;
                isHooked = false;
                return BattleResult.Win;
            }
        }

        // 6) Check win/lose by depth
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
    public void SetPlayerReelInput(float input)
    {
        playerForce = input;
    }

    float ComputeFishForce(float t)
    {
        // Base fish pulling strength
        float s = baseStrength;

        // Health factor: fish becomes weaker when health is low
        // Example:
        // 100% health -> factor = 1.0
        //  50% health -> factor = 0.5
        //   0% health -> factor = 0.2 (still pulls a bit)
        float healthFactor = Mathf.Clamp(fishHealth / fishMaxHealth, 0.2f, 1.0f);

        // Time based pulling pattern
        if (t < 1f) s *= 1.0f;  // strong start
        else if (t < 3f) s *= 0.8f;  // easier window
        else if (t < 5f) s *= 1.1f;  // power spike
        else s *= 0.5f;  // tired after long battle

        // Apply health reduction to final force
        return s * healthFactor;
    }
}
