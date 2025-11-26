using UnityEngine;

public class FishController : MonoBehaviour
{
    // Fish Stats (kept in case needed later)
    public float baseStrength = 8f;
    public float maxDepth = 10f;
    public float catchSpeed = 2f;
    public float escapeSpeed = 3f;

    // Direction info (used by FishManager only)
    public bool swimLeftToRight = true;
    public float laneY = 0f;     // assigned by FishManager
    public float startX = 0f;    // spawn X position

    // Runtime
    public bool isHooked = false;

    // IMPORTANT:
    // No movement in Update. Fish does not move automatically.

    void Update()
    {
        // Do nothing.
        // Fish stays at spawn position until hooked or externally moved.
    }

    // When hooked (optional if used later)
    public void Hooked()
    {
        isHooked = true;
    }

    // If you pull fish upward (optional)
    public void ApplyPull(float pullForce)
    {
        // Optional gameplay logic for pulling fish
        float upValue = catchSpeed * pullForce * Time.deltaTime;
        transform.position += Vector3.up * upValue;
    }
}
