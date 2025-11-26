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
    // Initialize lanes and slots
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
            float regionMax = regionWidth * 0.5f;

            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObj = new GameObject(layer.name + "_Slot_" + i);
                slotObj.transform.parent = this.transform;

                float posX = regionMin + spacing * (i + 0.5f);

                slotObj.transform.position = new Vector3(posX, layer.yPos, 0f);

                layer.slots.Add(slotObj.transform);

                SpawnFishInSlot(layer, slotObj.transform);
            }
        }
    }

    // ---------------------------------------------------------
    // Spawn one fish inside slot
    // ---------------------------------------------------------
    void SpawnFishInSlot(FishLayer layer, Transform slot)
    {
        if (layer.fishPrefabs == null || layer.fishPrefabs.Count == 0)
            return;

        GameObject prefab = layer.fishPrefabs[Random.Range(0, layer.fishPrefabs.Count)];
        if (prefab == null)
            return;

        GameObject fish = Instantiate(prefab, slot);
        fish.transform.localPosition = Vector3.zero;

        Vector3 s = fish.transform.localScale;
        float absX = Mathf.Abs(s.x);
        if (absX < 0.0001f) absX = 0.0001f;

        if (layer.swimLeftToRight)
            s.x = absX;
        else
            s.x = -absX;

        fish.transform.localScale = s;
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
