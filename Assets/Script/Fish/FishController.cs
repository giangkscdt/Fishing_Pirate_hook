using UnityEngine;

public class FishController : MonoBehaviour
{
    public int fishScore = 1; // score for this fish

    public float baseStrength = 8f;
    public float maxDepth = 10f;
    public float catchSpeed = 2f;
    public float escapeSpeed = 3f;

    public bool swimLeftToRight = true;
    public float laneY = 0f;
    public float startX = 0f;

    public bool isHooked = false;
    public System.Action<FishController> onOverDistance;

    private float screenWidth = 0f;

    // NEW: animator for battle animation
    private Animator anim;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float halfW = cam.orthographicSize * cam.aspect;
            screenWidth = halfW * 2f;
        }

        // get animator
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isHooked) return;

        float dist = Mathf.Abs(transform.position.x - startX);

        if (dist > screenWidth * 2f)
        {
            if (onOverDistance != null)
                onOverDistance(this);
        }
    }

    public void Hooked()
    {
        isHooked = true;

        // NEW: enter battle animation
        if (anim != null)
            anim.SetBool("isBattle", true);
    }

    // NEW: call this when collecting or releasing
    public void EndBattle()
    {
        isHooked = false;

        if (anim != null)
            anim.SetBool("isBattle", false);
    }

    public void ApplyPull(float pullForce)
    {
        float upValue = catchSpeed * pullForce * Time.deltaTime;
        transform.position += Vector3.up * upValue;
    }
}
