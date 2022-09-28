using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 5f; //how far in a direction should the dash take the player
    public float dashForce = 500f; //how big of a speed boost should he get after the dash
    public float dashCooldown = 1.5f; //how long to wait until the player can dash again
    [Range(0,1)] public float threshold = .2f; //how strong th input has to be for the dash to go into the input direction
    public ParticleSystem dashParticle;
    public Slider dashBar;

    PlayerMovement movementScript;
    bool canDash = true;
    private void Start()
    {
        dashBar.maxValue = dashCooldown;
        dashBar.value = dashCooldown;
        movementScript = GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            if (canDash)
                DashAction();
    }
    void DashAction()
    {
        //if there is no input the dash should go forward, use project on plane in case the player dashes on a slope to not go through the ground
        if (movementScript.direction.magnitude <= threshold)
        {
            RaycastHit ground;
            Physics.Raycast(movementScript.groundCheck.position, -transform.up, out ground, .5f);
            movementScript.direction = Vector3.ProjectOnPlane(transform.forward, ground.normal);
        }

        dashParticle.Play();
        movementScript.playerBody.AddForce(movementScript.direction * dashForce, ForceMode.Impulse);

        canDash = false;
        StartCoroutine(DashReady());
    }
    IEnumerator DashReady()
    {
        //refill the dash bar indicator
        float currentCooldown = 0f;
        while (currentCooldown < dashCooldown)
        {
            while(!PauseMenu.isPaused)
            {
                yield return null;
            }
            currentCooldown += Time.fixedDeltaTime;
            dashBar.value = currentCooldown;
            yield return null;
        }
        canDash = true;
    }
}
