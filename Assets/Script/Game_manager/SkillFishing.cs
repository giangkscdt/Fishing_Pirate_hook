using UnityEngine;

public class SkillFishing : MonoBehaviour
{
    // -----------------------------
    // Player / Reel Settings
    // -----------------------------
    [Header("Player / Reel")]
    public float reelForceMultiplier = 1.0f;   // How strong the reel pull becomes
    public float maxSafeTension = 10f;    // Upper safe limit
    public float minSafeTension = -10f;   // Lower safe limit

    // -----------------------------
    // Fish Stats
    // -----------------------------
    [Header("Fish Stats")]
    public float baseStrength = 8f;            // Base pulling force of the fish
    public float maxDepth = 10f;           // Maximum depth (fish escapes if deeper than this)
    public float catchSpeed = 2f;            // Speed at which the fish is pulled upward
    public float escapeSpeed = 3f;            // Speed at which the fish pulls downward

    // -----------------------------
    // Runtime Debug (Inspector Only)
    // -----------------------------
    [Header("Runtime Debug")]
    public float reelSpeed;      // Player reel input speed
    public float playerForce;    // Pull force created by the player
    public float fishForce;      // Force created by the fish
    public float tension;        // playerForce - fishForce
    public float depth;          // 0 = at surface, >0 = underwater

    private float hookTime = 0f; // Time since fish was hooked
    private bool isHooked = false;

    void Start()
    {
        ResetFish();
    }

    // Reset fish state and prepare a new catch
    void ResetFish()
    {
        depth = maxDepth * 0.7f;  // Start at 70 percent of max depth
        hookTime = 0f;
        isHooked = true;             // For demo: assume fish is always hooked
    }

    void Update()
    {
        if (!isHooked)
            return;

        hookTime += Time.deltaTime;

        // ------------------------------------
        // 1. Read reel input from player
        // Replace this with real rotary encoder if needed
        // ------------------------------------
        float input = Input.GetAxis("Vertical"); // Range -1 to 1
        reelSpeed = Mathf.Max(0f, input);      // Only upward reel movement matters

        // ------------------------------------
        // 2. Player pull force
        // ------------------------------------
        playerForce = reelSpeed * reelForceMultiplier;

        // ------------------------------------
        // 3. Fish force (skill-based, no RNG)
        // ------------------------------------
        fishForce = ComputeFishForce(hookTime);

        // ------------------------------------
        // 4. Calculate tension
        // Positive = player pulling stronger
        // Negative = fish pulling stronger
        // ------------------------------------
        tension = playerForce - fishForce;

        // ------------------------------------
        // 5. Tension logic (safe zone check)
        // ------------------------------------
        if (tension > maxSafeTension)
        {
            Debug.Log("Line broke! Too much force.");
            isHooked = false;
            return;
        }
        else if (tension < minSafeTension)
        {
            // Fish is stronger, pulling downward
            depth += escapeSpeed * Time.deltaTime;
        }
        else
        {
            // Within safe zone: fish is pulled upward
            depth -= catchSpeed * Time.deltaTime;
        }

        // ------------------------------------
        // 6. Check if fish is caught or escaped
        // ------------------------------------
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

    // ----------------------------------------
    // Fish force pattern (no randomness)
    // Fully deterministic arcade-style behavior
    // ----------------------------------------
    float ComputeFishForce(float t)
    {
        float s = baseStrength;

        if (t < 1f)
        {
            // Initial shock (fish pulls strong at the start)
            return s * 1.2f;
        }
        else if (t < 3f)
        {
            // Stable region
            return s * 0.9f;
        }
        else if (t < 5f)
        {
            // Struggle peak (hardest part)
            return s * 1.3f;
        }
        else
        {
            // Fish getting tired
            return s * 0.6f;
        }
    }
}
