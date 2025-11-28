using System.Collections;
using UnityEngine;

public class HookController : MonoBehaviour
{
    [Header("References")]
    public Transform head;
    public Transform rod;
    public Transform hook;
    public Transform hookEndPos;   // Fish_HookEndPos in hierarchy

    public Transform fishKeepPos;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float minLength = 0.5f;
    public float maxLength = 5f;
    private float currentLength = 0.5f;

    // Lower / retract flags
    private bool isLowering = false;
    private bool isRetracting = false;
    private bool reachedBottom = false;

    // Battle state
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
            BattleControl();
        }

        // Keep hook at line end
        hook.position = hookEndPos.position;

        // While in battle, keep fish snapped to hook end
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
        // Press Q ONCE to start lowering
        if (Input.GetKeyDown(KeyCode.Q) && !isLowering && !isRetracting)
        {
            isLowering = true;
            reachedBottom = false;
        }

        if (isLowering && !reachedBottom)
        {
            // Lower line
            currentLength += moveSpeed * Time.deltaTime;

            if (currentLength >= maxLength)
            {
                currentLength = maxLength;
                reachedBottom = true;

                // Start retract when reach bottom
                isLowering = false;
                isRetracting = true;
            }
        }
        else if (isRetracting)
        {
            // Retract line
            currentLength -= moveSpeed * Time.deltaTime;

            if (currentLength <= minLength)
            {
                currentLength = minLength;
                // Fully back to start, stop any auto movement
                ResetLoweringStates();
            }
        }

        UpdateRodLength();
    }

    void ResetLoweringStates()
    {
        // Clear all lower / retract states
        isLowering = false;
        isRetracting = false;
        reachedBottom = false;
    }

    void BattleControl()
    {
        // Player pull up with A, fish pull down when no input
        if (Input.GetKey(KeyCode.A))
        {
            currentLength -= moveSpeed * Time.deltaTime;
        }
        else
        {
            currentLength += moveSpeed * 0.6f * Time.deltaTime;
        }

        currentLength = Mathf.Clamp(currentLength, minLength, maxLength);
        UpdateRodLength();

        // Catch condition
        if (currentLength <= minLength)
        {
            // Make sure no auto lowering after catch
            ResetLoweringStates();

            if (!isCollected && hookedFish != null)
            {
                isCollected = true;
                StartCoroutine(FishCollectAnimation());
            }

            return;
        }
        // Escape condition
        else if (currentLength >= maxLength)
        {
            ReleaseFish();
        }
    }

    IEnumerator FishCollectAnimation()
    {
        // Stop battle, so Update does not snap fish to hook
        isHooking = false;

        // Local reference to avoid losing fish if hookedFish changes
        GameObject fish = hookedFish;

        if (fish == null)
        {
            // Nothing to animate
            isCollected = false;
            ResetLoweringStates();
            yield break;
        }

        Vector3 start = fish.transform.position;
        Vector3 end = fishKeepPos.position;

        // Simple arc control point
        Vector3 controlPoint = start + Vector3.up * 2f;

        float t = 0f;
        Vector3 originalScale = fish.transform.localScale;

        while (t < 1f)
        {
            if (fish == null)
            {
                // Fish destroyed externally, stop animation
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

        // Increase score and destroy fish
        score++;
        Debug.Log("Score = " + score);

        if (fish != null)
        {
            Destroy(fish);
        }

        hookedFish = null;

        // Reset for next round
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

        // After escape, only retract back, do not lower again
        isLowering = false;
        isRetracting = true;
        reachedBottom = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only hook fish, only when lowering downward
        if (!other.CompareTag("Fish")) return;
        if (isHooking) return;
        if (!isLowering) return;

        hookedFish = other.gameObject;
        isHooking = true;

        var rb = hookedFish.GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;

        hookedFish.transform.position = hookEndPos.position;
        hookedFish.transform.rotation = hookEndPos.rotation;

        Debug.Log("Fish hooked! Battle start!");
    }
}
