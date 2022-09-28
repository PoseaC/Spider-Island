using UnityEngine;
using System.Collections;

public class WeaponTilt : MonoBehaviour
{
    public float maxTilt = 10f;
    public float tiltStrength = 2f;
    void Update()
    {
        float gunTiltX = Mathf.Clamp(Input.GetAxis("Mouse X") * tiltStrength, -maxTilt, maxTilt);
        float gunTiltY = Mathf.Clamp(Input.GetAxis("Mouse Y") * tiltStrength, -maxTilt, maxTilt);
        Vector3 rotation = new Vector3(-gunTiltY, gunTiltX, 0);
        transform.localEulerAngles = rotation;
    }
}
