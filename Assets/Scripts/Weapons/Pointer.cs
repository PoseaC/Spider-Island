using UnityEngine;

public class Pointer : MonoBehaviour
{
    public Transform player;
    public Transform item;

    private void Update()
    {
        transform.LookAt(player);
        transform.position = item.position + Vector3.up;
    }
}
