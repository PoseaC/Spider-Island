using UnityEngine;

public class BulletHole : MonoBehaviour
{
    private void Awake()
    {
        FindObjectOfType<GameManager>().CheckBullets(this);
        Ray adoption = new Ray(transform.position + transform.up, -transform.up);
        Physics.Raycast(adoption, out RaycastHit hit, 1);
        transform.parent = hit.transform;
    }
}
