using UnityEngine;

public class MoveObjectToUIInstant : MonoBehaviour
{
    public RectTransform targetUI;

    void Start()
    {
        if (targetUI == null)
            return;

        // direct world position copy
        Vector3 pos = targetUI.position;

        // keep original Z of object (2D game requirement)
        pos.z = transform.position.z;

        transform.position = pos;
    }
}
