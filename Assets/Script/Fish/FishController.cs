using UnityEngine;

public class FishController : MonoBehaviour
{
    // Stats (not used in movement, kept for battle logic)
    public float baseStrength = 8f;
    public float maxDepth = 10f;
    public float catchSpeed = 2f;
    public float escapeSpeed = 3f;

    // Direction info (set by FishManager)
    public bool swimLeftToRight = true;
    public float laneY = 0f;
    public float startX = 0f;

    // Runtime state
    public bool isHooked = false;

    // Callback to notify FishManager when fish over-swims
    public System.Action<FishController> onOverDistance;

    // Screen width in world units
    private float screenWidth = 0f;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float halfW = cam.orthographicSize * cam.aspect;
            screenWidth = halfW * 2f;
        }
    }

    void Update()
    {
        if (isHooked) return;

        float dist = Mathf.Abs(transform.position.x - startX);

        // If over 2 times screen width -> tell FishManager
        if (dist > screenWidth * 2f)
        {
            if (onOverDistance != null)
                onOverDistance(this);
        }
    }

    // Called when fish is hooked
    public void Hooked()
    {
        isHooked = true;
    }

    // Optional: fish move when pulled upward
    public void ApplyPull(float pullForce)
    {
        float upValue = catchSpeed * pullForce * Time.deltaTime;
        transform.position += Vector3.up * upValue;
    }
}
