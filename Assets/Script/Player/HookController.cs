using System.Collections;
using UnityEngine;
using TMPro;

public class HookController : MonoBehaviour
{
    [Header("Head Rotation Settings")]
    public float rotationSpeed = 1.5f;
    public float rotationAngle = 10f;

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
    private bool isRetracting = false;

    private GameObject hookedObject;

    private int score = 0;
    public TMP_Text scoreText;
    public GameObject starPrefab;
    public RectTransform scoreUIPos;

    private bool inputPressDown = false;
    private bool inputPressUp = false;


    void Start()
    {
        currentLength = minLength;
        UpdateRodLength();

        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    void Update()
    {
        inputPressDown = false;
        inputPressUp = false;
        UpdateInput(); // <--- NEW
        RotateHead();

        if (!isHooking)
            LoweringSystem();
        else
            BattleControl();

        hook.position = hookEndPos.position;

        if (hookedObject != null && isHooking)
        {
            hookedObject.transform.position = hookEndPos.position;
            hookedObject.transform.rotation = hookEndPos.rotation;
        }
    }

    void RotateHead()
    {
        float angle = Mathf.Sin(Time.time * rotationSpeed) * rotationAngle;
        head.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void LoweringSystem()
    {
        // Start lowering hook using Mouse or Touch or Q
        if ((inputPressDown || Input.GetKeyDown(KeyCode.Q)) && !isLowering && !isRetracting)
            isLowering = true;


        if (isLowering)
        {
            currentLength += moveSpeed * Time.deltaTime;

            if (currentLength >= maxLength)
            {
                currentLength = maxLength;
                isLowering = false;

                if (hookedObject == null)
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

    void UpdateInput()
    {
        // Mouse left click
        if (Input.GetMouseButtonDown(0))
            inputPressDown = true;
        if (Input.GetMouseButtonUp(0))
            inputPressUp = true;

        // Touch for phone
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                inputPressDown = true;
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                inputPressUp = true;
        }

        // Keyboard A (fallback)
        if (Input.GetKeyDown(KeyCode.A))
            inputPressDown = true;
        if (Input.GetKeyUp(KeyCode.A))
            inputPressUp = true;
    }


    void BattleControl()
    {
        if (skillFishing == null) return;

        var fishCtrl = hookedObject != null ? hookedObject.GetComponent<FishController>() : null;
        var objCtrl = hookedObject != null ? hookedObject.GetComponent<CaughtObjectController>() : null;

        // Non-fish object handling (shoe, ball, rabbit...)
        if (objCtrl != null)
        {
            if (inputPressDown)
            {
                currentLength -= moveSpeed * Time.deltaTime; // shorten rope
                currentLength = Mathf.Max(minLength, currentLength);
                UpdateRodLength();
            }

            // Check collect condition
            if (currentLength <= minLength + 0.01f)
            {
                StartCoroutine(CollectAnimation());
            }

            return;
        }


        // Fish battle handling
        bool pullingImpulse = inputPressDown;
        var result = skillFishing.TickBattle(pullingImpulse);

        float normDepth = Mathf.Clamp01(skillFishing.depth / skillFishing.maxDepth);
        currentLength = Mathf.Lerp(minLength, maxLength, normDepth);
        UpdateRodLength();

        if (fishCtrl != null)
        {
            ui.UpdateTension(
                skillFishing.tension,
                skillFishing.minSafeTension,
                skillFishing.maxSafeTension,
                result,
                skillFishing.isHooked
            );
        }

        if (!skillFishing.isHooked)
        {
            if (fishCtrl != null)
            {
                if (result == SkillFishing.BattleResult.Win)
                {
                    StartCoroutine(CollectAnimation());
                }
                else if (result == SkillFishing.BattleResult.Lose)
                {
                    ReleaseObject();
                }
            }
        }
    }


    IEnumerator CollectAnimation()
    {
        isHooking = true;
        GameObject obj = hookedObject;

        if (obj == null)
        {
            ResetLine();
            yield break;
        }

        bool isFish = obj.GetComponent<FishController>() != null;

        Vector3 start = obj.transform.position;
        Vector3 end = fishKeepPos.position;
        Vector3 ctrl = start + Vector3.up * 2f;

        float t = 0f;
        Vector3 originalScale = obj.transform.localScale;

        while (t < 1f)
        {
            if (obj == null) yield break;

            t += Time.deltaTime * 1.2f;

            Vector3 p1 = Vector3.Lerp(start, ctrl, t);
            Vector3 p2 = Vector3.Lerp(ctrl, end, t);

            obj.transform.position = Vector3.Lerp(p1, p2, t);

            if (isFish) // Only fish scale to zero
            {
                float scaleFactor = Mathf.Lerp(1f, 0f, t);
                obj.transform.localScale = originalScale * scaleFactor;
            }

            yield return null;
        }

        if (isFish)
        {
            FishController f = obj.GetComponent<FishController>();
            if (f != null)
            {
                score += f.fishScore;
                if (scoreText != null) scoreText.text = score.ToString();

                for (int i = 0; i < 5; i++)
                {
                    GameObject star = Instantiate(starPrefab, hookEndPos.position, Quaternion.identity);
                    var fx = star.GetComponent<StarFlyEffect>();
                    fx.targetUI = scoreUIPos;
                }
            }
        }

        if (obj != null)
            Destroy(obj);

        hookedObject = null;
        ResetLine();
    }


    void ReleaseObject()
    {
        if (hookedObject)
        {
            var rb = hookedObject.GetComponent<Rigidbody2D>();
            if (rb) rb.gravityScale = 1f;
        }

        hookedObject = null;
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
    public void OnLowerStart()
    {
        if (!isLowering && !isRetracting)
        {
            isLowering = true;
        }
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
        if (!isLowering) return;
        if (isRetracting) return;

        var fishCtrl = other.GetComponent<FishController>();
        var objCtrl = other.GetComponent<CaughtObjectController>();

        if (fishCtrl == null && objCtrl == null)
            return;

        hookedObject = other.gameObject;
        isHooking = true;
        isLowering = false;
        isRetracting = false;

        var rb = hookedObject.GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;

        hookedObject.transform.position = hookEndPos.position;
        hookedObject.transform.rotation = hookEndPos.rotation;

        if (fishCtrl != null)
        {
            skillFishing.SetupFishStats(
                fishCtrl.baseStrength,
                fishCtrl.catchSpeed,
                fishCtrl.escapeSpeed
            );
            skillFishing.StartBattle(currentLength);
            ui.Show();
        }
        else
        {
            // Non-fish object hooked: enable pulling by gravity + A press
            
            if (objCtrl != null)
            {
                objCtrl.Hooked(); // enable physics DOWN
                isHooking = true; // allow pull control
                ui.Hide(); // no battle bar
            }
        }

    }
}
