using UnityEngine;

public class HookController : MonoBehaviour
{
    [Header("References")]
    public Transform head;
    public Transform rod;
    public Transform hook;
    public Transform hookEndPos;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float minLength = 0.5f;
    public float maxLength = 5f;
    private float currentLength = 0.5f;

    private bool isLowering = false;
    private bool reachedBottom = false;

    // Battle state
    private bool isHooking = false;
    private GameObject hookedFish;

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
            LowerControl();
        }
        else
        {
            BattleControl();
        }

        // Hook always follows rod end
        hook.position = hookEndPos.position;
    }

    void RotateHead()
    {
        head.Rotate(0f, 0f, Mathf.Sin(Time.time * 1.5f) * 0.2f);
    }

    void LowerControl()
    {
        // Press and hold Q to lower
        isLowering = Input.GetKey(KeyCode.Q);

        if (isLowering && !reachedBottom)
        {
            currentLength += moveSpeed * Time.deltaTime;
            if (currentLength >= maxLength)
            {
                currentLength = maxLength;
                reachedBottom = true;
            }
        }
        else
        {
            // Auto retract if not lowering
            currentLength -= moveSpeed * Time.deltaTime;
            currentLength = Mathf.Clamp(currentLength, minLength, maxLength);

            if (currentLength <= minLength)
            {
                currentLength = minLength;
                reachedBottom = false;
            }
        }

        UpdateRodLength();
    }

    void BattleControl()
    {
        // Player must press A to pull up
        if (Input.GetKey(KeyCode.A))
        {
            currentLength -= moveSpeed * Time.deltaTime;
        }
        else
        {
            // Fish drag down stronger
            currentLength += moveSpeed * 0.6f * Time.deltaTime;
        }

        currentLength = Mathf.Clamp(currentLength, minLength, maxLength);
        UpdateRodLength();

        // End conditions
        if (currentLength <= minLength)
        {
            Debug.Log("Player caught the fish!");
            ReleaseFish();
        }
        else if (currentLength >= maxLength)
        {
            Debug.Log("Fish escaped!");
            ReleaseFish();
        }
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
            hookedFish.transform.SetParent(null);
            var rb = hookedFish.GetComponent<Rigidbody2D>();
            if (rb) rb.gravityScale = 1f;
        }

        hookedFish = null;
        isHooking = false;
        isLowering = false;
        reachedBottom = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Fish")) return;
        if (isHooking) return;      // Already in battle
        if (!isLowering) return;    // Only hook during lowering

        // Hook first fish -> START BATTLE HERE
        hookedFish = other.gameObject;
        isHooking = true;

        other.transform.SetParent(hookEndPos);
        other.transform.localPosition = Vector3.zero;

        var rb = other.GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;

        Debug.Log("Fish hooked! Battle start!");
    }
}
