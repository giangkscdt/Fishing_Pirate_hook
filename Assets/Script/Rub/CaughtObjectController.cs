using UnityEngine;

public class CaughtObjectController : MonoBehaviour
{
    public enum ObjectType
    {
        Ball,
        Shoe,
        Rabbit,
        Other
    }

    [Header("Object Type")]
    public ObjectType objectType = ObjectType.Other;

    [Header("Physics Settings")]
    public float weight = 5f;
    public float maxDepth = 10f;
    public float pullSpeed = 2f;

    [Header("Runtime State")]
    public bool isHooked = false;
    private float verticalVelocity = 0f;

    void Update()
    {
        if (isHooked)
        {
            verticalVelocity -= weight * Time.deltaTime;

            Vector3 pos = transform.position;
            pos.y += verticalVelocity * Time.deltaTime;

            pos.y = Mathf.Max(pos.y, -maxDepth);
            transform.position = pos;
        }
        else
        {
            verticalVelocity = 0f;
        }
    }

    public void Hooked()
    {
        isHooked = true;
    }

    public void ApplyPull(float pullForce)
    {
        if (!isHooked) return;
        transform.position += Vector3.up * pullSpeed * pullForce * Time.deltaTime;
    }
}
