using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FishLayer
{
    public string name = "Lane";
    public float yPos = 0f;
    public bool swimLeftToRight = true;
    public int visibleSlotCount = 10;
    public float slotSpacing = 0f;
    public float moveSpeed = 2f;
    public List<GameObject> fishPrefabs;
    public bool defence = false;
    [HideInInspector] public List<Transform> slots = new List<Transform>();
}

public class FishManager : MonoBehaviour
{
    public List<FishLayer> layers = new List<FishLayer>();

    public Transform LeftSpawnPos;
    public Transform RightSpawnPos;

    void Start()
    {
        InitAllLayers();
    }

    void Update()
    {
        UpdateAllSlots();
    }

    bool SlotIsTooCloseToOtherFish(FishLayer layer, Transform slot)
    {
        foreach (Transform other in layer.slots)
        {
            if (other == slot) continue;

            if (other.childCount > 0)
            {
                GameObject fish = other.GetChild(0).gameObject;
                SpriteRenderer sr = fish.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float minDistance = sr.bounds.size.x * 1.2f;
                    if (Mathf.Abs(slot.position.x - other.position.x) < minDistance)
                        return true;
                }
            }
        }
        return false;
    }

    void InitAllLayers()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfW = cam.orthographicSize * cam.aspect;
        float X = halfW * 2f;

        foreach (var layer in layers)
        {
            layer.slots.Clear();

            int visibleCount = Mathf.Max(1, layer.visibleSlotCount);
            int totalSlots = visibleCount * 3;

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
                slotObj.transform.position =
                    new Vector3(posX, layer.yPos, 0f);

                layer.slots.Add(slotObj.transform);
            }

            if (layer.defence)
                SpawnDefence(layer);
            else
                SpawnPattern(layer);
        }
    }

    void SpawnDefence(FishLayer layer)
    {
        if (layer.fishPrefabs.Count == 0) return;

        foreach (Transform slot in layer.slots)
        {
            if (SlotIsTooCloseToOtherFish(layer, slot))
                continue;

            GameObject prefab =
                layer.fishPrefabs[Random.Range(0, layer.fishPrefabs.Count)];
            SpawnSpecificFish(layer, slot, prefab);
        }
    }

    void SpawnPattern(FishLayer layer)
    {
        if (layer.fishPrefabs.Count == 0) return;

        int total = layer.slots.Count;
        int index = 0;
        GameObject lastPrefab = null;

        while (index < total)
        {
            GameObject prefab;
            int safety = 20;
            do
            {
                prefab = layer.fishPrefabs[Random.Range(0, layer.fishPrefabs.Count)];
                safety--;
            }
            while (safety > 0 && prefab == lastPrefab);

            lastPrefab = prefab;

            int fishGroup = Random.Range(1, 4);
            int emptyGroup = Random.Range(1, 3);

            for (int i = 0; i < fishGroup && index < total; i++)
            {
                Transform slot = layer.slots[index];

                if (!SlotIsTooCloseToOtherFish(layer, slot))
                    SpawnSpecificFish(layer, slot, prefab);

                index++;
            }

            for (int i = 0; i < emptyGroup && index < total; i++)
                index++;
        }
    }

    void SpawnSpecificFish(FishLayer layer, Transform slot, GameObject prefab)
    {
        if (!prefab) return;

        GameObject fish = Instantiate(prefab, slot);
        fish.transform.localPosition = Vector3.zero;

        Vector3 s = fish.transform.localScale;
        float absX = Mathf.Abs(s.x);
        if (absX < 0.0001f) absX = 0.0001f;
        s.x = layer.swimLeftToRight ? absX : -absX;
        fish.transform.localScale = s;

        Animator anim = fish.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(0, -1, Random.Range(0f, 1f));
            anim.speed = Random.Range(0.9f, 1.2f);
        }
    }

    void UpdateAllSlots()
    {
        if (LeftSpawnPos == null || RightSpawnPos == null) return;

        foreach (var layer in layers)
        {
            float dir = layer.swimLeftToRight ? 1f : -1f;
            float speed = layer.moveSpeed * Time.deltaTime;

            foreach (Transform slot in layer.slots)
            {
                if (slot == null) continue;

                Vector3 pos = slot.position;
                pos.x += dir * speed;

                if (layer.swimLeftToRight)
                {
                    if (pos.x > RightSpawnPos.position.x)
                        pos.x = LeftSpawnPos.position.x;
                }
                else
                {
                    if (pos.x < LeftSpawnPos.position.x)
                        pos.x = RightSpawnPos.position.x;
                }

                if (layer.defence)
                {
                    float t = Time.time + slot.GetSiblingIndex();
                    pos.y = layer.yPos + Mathf.Sin(t * 2.5f) * 0.35f;
                }
                else pos.y = layer.yPos;

                slot.position = pos;
            }
        }
    }
}
