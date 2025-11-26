using UnityEngine;

public class SpawnPosOffscreenMover : MonoBehaviour
{
    public string leftName = "LeftSpawnPos";
    public string rightName = "RightSpawnPos";

    void Start()
    {
        MoveSpawnPositions();
    }

    void MoveSpawnPositions()
    {
        Camera cam = Camera.main;
        float halfWidth = cam.orthographicSize * cam.aspect;

        // world edges
        float leftOff = -halfWidth - 10f;
        float rightOff = halfWidth + 10f;

        // find objects
        GameObject left = GameObject.Find(leftName);
        GameObject right = GameObject.Find(rightName);

        if (left != null)
        {
            Vector3 pos = left.transform.position;
            pos.x = leftOff;
            left.transform.position = pos;
        }

        if (right != null)
        {
            Vector3 pos = right.transform.position;
            pos.x = rightOff;
            right.transform.position = pos;
        }
    }
}
