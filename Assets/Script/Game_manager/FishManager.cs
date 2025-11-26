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

    // runtime list of slot transforms
    [HideInInspector] public List<Transform> slots = new List<Transform>();
}

public class FishManager : MonoBehaviour
{
    public List<FishLayer> layers = new List<FishLayer>();

    // ---------------------------------------------------------
    void Start()
    {
        InitAllLayers();
    }

    // ---------------------------------------------------------
    void Update()
    {
        UpdateAllSlots();
    }

    // ---------------------------------------------------------
    // Initialize lanes, create slots, then spawn fish pattern
    // ---------------------------------------------------------
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

            // ----------- Create slot objects -----------
            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObj = new GameObject(layer.name + "_Slot_" + i);
                slotObj.transform.parent = this.transform;

                float posX = regionMin + spacing * (i + 0.5f);
                slotObj.transform.position = new Vector3(posX, layer.yPos, 0f);

                layer.slots.Add(slotObj.transform);
            }

            // ----------- NEW: spawn by pattern groups -----------
            SpawnPattern(layer);
        }
    }

    // ---------------------------------------------------------
    // NEW: Spawn pattern = (2-3 fish) + (1-3 empty slots)
    // ---------------------------------------------------------
    void SpawnPattern(FishLayer layer)
    {
        if (layer.fishPrefabs == null || layer.fishPrefabs.Count == 0)
            return;

        int total = layer.slots.Count;
        int index = 0;

        while (index < total)
        {
            // ----------- Pick a fish type for the group -----------
            GameObject prefab = layer.fishPrefabs[Random.Range(0, layer.fishPrefabs.Count)];

            // ----------- Group size: 2-3 fishes -----------
            int fishGroup = Random.Range(2, 4); // 2 or 3
            // ----------- Empty slots: 1-3 -----------
            int emptyGroup = Random.Range(1, 4); // 1, 2, or 3

            // ----------- Spawn fish in adjacent slots -----------
            for (int i = 0; i < fishGroup; i++)
            {
                if (index >= total) return;

                Transform slot = layer.slots[index];
                SpawnSpecificFish(layer, slot, prefab);
                index++;
            }

            // ----------- Leave empty slots -----------
            for (int i = 0; i < emptyGroup; i++)
            {
                if (index >= total) return;
                // no fish in this slot
                index++;
            }
        }
    }

    // ---------------------------------------------------------
    // Spawn one specific fish prefab inside slot
    // ---------------------------------------------------------
    void SpawnSpecificFish(FishLayer layer, Transform slot, GameObject prefab)
    {
        if (prefab == null) return;

        GameObject fish = Instantiate(prefab, slot);
        fish.transform.localPosition = Vector3.zero;

        // Flip based on swim direction
        Vector3 s = fish.transform.localScale;
        float absX = Mathf.Abs(s.x);
        if (absX < 0.0001f) absX = 0.0001f;

        if (layer.swimLeftToRight)
            s.x = absX;
        else
            s.x = -absX;

        fish.transform.localScale = s;

        // ---------------------------------------------------------
        // NEW: Add random animation offset so fish do not move in sync
        // ---------------------------------------------------------

        Animator animator = fish.GetComponent<Animator>();
        if (animator != null)
        {
            // Start animation at a random point in [0,1]
            float randomOffset = Random.Range(0f, 1f);
            animator.Play(0, -1, randomOffset);

            // Also random playback speed (subtle)
            animator.speed = Random.Range(0.9f, 1.2f);
        }

        // Legacy Animation component (if you use it)
        Animation anim = fish.GetComponent<Animation>();
        if (anim != null)
        {
            foreach (AnimationState st in anim)
            {
                st.time = Random.Range(0f, st.length); // random start frame
                st.speed = Random.Range(0.9f, 1.2f);
            }
            anim.Play();
        }
    }


    // ---------------------------------------------------------
    // Move all slots and wrap them
    // ---------------------------------------------------------
    void UpdateAllSlots()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfW = cam.orthographicSize * cam.aspect; // X/2
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

            foreach (var slot in layer.slots)
            {
                if (slot == null) continue;

                Vector3 pos = slot.position;
                pos.x += dir * speed * Time.deltaTime;

                // wrap
                if (pos.x > regionMax)
                {
                    pos.x -= regionWidth;
                }
                else if (pos.x < regionMin)
                {
                    pos.x += regionWidth;
                }

                pos.y = layer.yPos;
                slot.position = pos;
            }
        }
    }
}
