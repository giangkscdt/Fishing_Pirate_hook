using UnityEngine;

public class StarFlyEffect : MonoBehaviour
{
    public Transform targetUI;
    public float speed = 6f;
    public float rotateSpeed = 180f;
    public float fadeSpeed = 3f;
    public Camera uiCamera; // ADD THIS

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (targetUI == null) return;

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, targetUI.position);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * speed);

        // Rotate
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        // Fade
        Color c = sr.color;
        c.a -= Time.deltaTime * fadeSpeed;
        sr.color = c;

        if (c.a <= 0f)
            Destroy(gameObject);
    }
}
