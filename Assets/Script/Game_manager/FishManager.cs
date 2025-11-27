using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FishLayer
{
    public string name = "Lane";

    public float yPos = 0f;

    // true = left to right
    public bool swimLeftToRight = true;

    // number of visible slots on screen
    public int visibleSlotCount = 10;

    // custom spacing override (if <= 0 auto is used)
    public float slotSpacing = 0f;

    // movement speed
    public float moveSpeed = 2f;

    // fish prefabs
    public List<GameObject> fishPrefabs;

    // NEW: Defence mode (no group, random fish each slot)
    public bool defence = false;

    // runtime list of slot transforms
    [HideInInspector] public List<Transform> slots = new List<Transform>();
}

public class FishManager : MonoBehaviour
{
    public List<FishLayer> layers = new List<FishLayer>();

    void Start()
    {
        InitAllLayers();
    }

    void Update()
    {
        UpdateAllSlots();
    }

    void InitAllLayers()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("FishManager: No main camera found.");
            return;
        }

        float halfW = cam.orthographicSize * cam.aspect; // X/2

        foreach (var layer in layers)
        {
            layer.slots.Clear();

            int visibleCount = Mathf.Max(1, layer.visibleSlotCount);
            int totalSlots = visibleCount * 3;

            float X = halfW * 2f;
            float spacing = (layer.slotSpacing > 0f)
                ? layer.slotSpacing
                : (X / visibleCount);

            float regionWidth = spacing * totalSlots;
            float regionMin = -regionWidth * 0.5f;

            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObj = new GameObject(layer.name + "_Slot_" + i);
                slotObj.transform.parent = this.transform;

                float posX = regionMin + spacing * (i + 0.5f);
                slotObj.transform.position = new Vector3(posX, layer.yPos, 0f);

                layer.slots.Add(slotObj.transform);
            }

            // Defence mode or Group mode
            if (layer.defence)
                SpawnDefence(layer);
            else
                SpawnPattern(layer);
        }
    }

    // =========================================================
    // DEFENCE MODE: No group, no empty, each slot random fish
    // =========================================================
    void SpawnDefence(FishLayer layer)
    {
        if (layer.fishPrefabs == null || layer.fishPrefabs.Count == 0) return;

        foreach (Transform slot in layer.slots)
        {
            GameObject prefab = layer.fishPrefabs[Random.Range(0, layer.fishPrefabs.Count)];
            SpawnSpecificFish(layer, slot, prefab);
        }
    }

    // =========================================================
    // ORIGINAL Pattern mode (2-3 fish + 1-3 empty)
    // =========================================================
    void SpawnPattern(FishLayer layer)
    {
        if (layer.fishPrefabs == null || layer.fishPrefabs.Count == 0) return;

        int total = layer.slots.Count;
        int index = 0;

        while (index < total)
        {
            GameObject prefab = layer.fishPrefabs[Random.Range(0, layer.fishPrefabs.Count)];
            int fishGroup = Random.Range(1, 4);  // 1 or 3 fishes
            int emptyGroup = Random.Range(1, 2); // 1 to 3 empty slots

            for (int i = 0; i < fishGroup && index < total; i++)
            {
                SpawnSpecificFish(layer, layer.slots[index], prefab);
                index++;
            }

            for (int i = 0; i < emptyGroup && index < total; i++)
            {
                index++; // leave empty
            }
        }
    }

    void SpawnSpecificFish(FishLayer layer, Transform slot, GameObject prefab)
    {
        if (!prefab) return;

        GameObject fish = Object.Instantiate(prefab, slot);
        fish.transform.localPosition = Vector3.zero;

        // Flip based on swim direction
        Vector3 s = fish.transform.localScale;
        float absX = Mathf.Abs(s.x);
        if (absX < 0.0001f) absX = 0.0001f;
        s.x = layer.swimLeftToRight ? absX : -absX;
        fish.transform.localScale = s;

        // Random animation offset (keep for Defence mode)
        Animator animator = fish.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0, -1, Random.Range(0f, 1f));
            animator.speed = Random.Range(0.9f, 1.2f);
        }

        Animation anim = fish.GetComponent<Animation>();
        if (anim != null)
        {
            foreach (AnimationState st in anim)
            {
                st.time = Random.Range(0f, st.length);
                st.speed = Random.Range(0.9f, 1.2f);
            }
            anim.Play();
        }
    }

    void UpdateAllSlots()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfW = cam.orthographicSize * cam.aspect;
        float X = halfW * 2f;

        foreach (var layer in layers)
        {
            int visibleCount = Mathf.Max(1, layer.visibleSlotCount);
            int totalSlots = visibleCount * 3;

            float spacing = (layer.slotSpacing > 0f)
                ? layer.slotSpacing
                : (X / visibleCount);

            float regionWidth = spacing * totalSlots;
            float regionMin = -regionWidth * 0.5f;
            float regionMax = regionWidth * 0.5f;

            float dir = layer.swimLeftToRight ? 1f : -1f;
            float speed = layer.moveSpeed;

            for (int i = 0; i < layer.slots.Count; i++)
            {
                var slot = layer.slots[i];
                if (!slot) continue;

                Vector3 pos = slot.position;

                // Shared X scrolling movement
                pos.x += dir * speed * Time.deltaTime;

                // Loop horizontally
                if (pos.x > regionMax) pos.x -= regionWidth;
                else if (pos.x < regionMin) pos.x += regionWidth;

                if (layer.defence)
                {
                    // === DEFENCE MODE: horizontal scroll + vertical wave ===
                    float t = Time.time + i * 0.3f; // different phase for each slot

                    float waveSpeed = layer.moveSpeed * 1.2f;
                    float waveAmpY = spacing * 0.4f; // height of the wave

                    pos.y = layer.yPos + Mathf.Sin(t * waveSpeed) * waveAmpY;
                }
                else
                {
                    // original straight line
                    pos.y = layer.yPos;
                }



                slot.position = pos;
            }
        }
    }

}
