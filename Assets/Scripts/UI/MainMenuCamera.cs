using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    public float rotationSpeed;
    void FixedUpdate()
    {
        transform.Rotate(rotationSpeed * Time.fixedDeltaTime * Vector3.up);
    }
}
