using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public Rigidbody player; //player, used for horizontal movement of the camera
    public Transform head; //player's head position, this will be where the camera will stay relative to the player

    //mouse sensitivity
    public float horizontalSensitivity = 100f;
    public float verticalSensitivity = 100f;

    //maximum vertical angle the camera can look up or down to
    public float maxAngle = 90f;

    //rotation of the camera
    float yRotation;
    float xRotation;

    float xMouse;
    float yMouse;

    IEnumerator recoveryCoroutine;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        horizontalSensitivity = PlayerPrefs.GetFloat("HorizontalMouse", 50) * 10;
        verticalSensitivity = PlayerPrefs.GetFloat("VerticalMouse", 50) * 10;
    }

    void Update()
    {
        //camera follows the head's position and rotation
        transform.SetPositionAndRotation(head.position, head.rotation); 

        //get the mouse input from the player
        xMouse = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.deltaTime;
        yMouse = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.deltaTime;

        //rotate the head for up and down movement
        yRotation -= yMouse;
        yRotation = Mathf.Clamp(yRotation, -maxAngle, maxAngle);
        head.localRotation = Quaternion.Euler(yRotation, 0, 0);

        //rotate the player for left and right movement
        xRotation += xMouse;
        player.transform.rotation = Quaternion.Euler(0, xRotation, 0);
    }
    public void Recoil(float recoil, float recoveryStep)
    {
        recoveryCoroutine = Recovery(yRotation, recoveryStep, yMouse);
        
        //if the recoil function is called while the camera is still recovering, stop the previous recovery first before recoiling again
        StopCoroutine(recoveryCoroutine);

        //recoil moves the camera up a certain amount, but it shouldn't go over the limit
        if (yRotation - recoil < -maxAngle)
            yRotation = -maxAngle;
        else
            yRotation -= recoil;

        StartCoroutine(recoveryCoroutine);
    }
    IEnumerator Recovery(float startRotation, float recoveryStep, float mousePosition)
    {
        //bring the cursor back down to the initial position
        while(yRotation < startRotation)
        {
            //if the mouse is moved during the recovery stop
            if (mousePosition != yMouse)
                yield break;

            yRotation += recoveryStep;
            yield return null;
        }
    }
}
