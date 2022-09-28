using UnityEngine;
using UnityEngine.AI;

public class IKFootSolver : MonoBehaviour
{
    //for more details watch Unity's official tutorial on procedural walk animation

    public IKFootSolver other; //pair foot of this one
    public LayerMask whatIsGround; //what the foot can be placed on
    public float stepHeight = .5f; //how high the foot rises off the ground when moving
    public float stepSize = 1f; //how far forward should the foot moves
    public float speed = 5f; //how fast the foot moves to it's next position
    [HideInInspector] public bool isMoving = false; //if the foot is currently moving

    AudioSource sound;
    NavMeshAgent agent; //reference to the navigation agent
    Vector3 initialPos, newPos, currentPos, oldPos; //newPos - target position, currentPos - active position, oldPos - last position before moving
    float smoothStep = 1; //interpolation value
    float distanceAboveGround = .5f; //how far down the raycast checks for ground
    private void Start()
    {
        initialPos = transform.localPosition;
        newPos = oldPos = currentPos = transform.position;
        agent = GetComponentInParent<NavMeshAgent>();
        sound = GetComponent<AudioSource>();
        distanceAboveGround = agent.baseOffset + 5f;
    }
    void Update()
    {
        isMoving = smoothStep < 1; //if we are interpolatin between 2 positions it means the foot is moving
        transform.position = currentPos;
        Ray groundCheck = new Ray(transform.parent.position + Vector3.up * distanceAboveGround, Vector3.down); //raycast from above the parent position of the foot ik target
        if(Physics.Raycast(groundCheck, out RaycastHit hit, distanceAboveGround * 2, whatIsGround))
        {
            Debug.DrawRay(transform.parent.position + Vector3.up * distanceAboveGround, Vector3.down * distanceAboveGround * 2);
            if (!other.isMoving && smoothStep >= 1) //move only if the pair foot is not moving and we are not already interpolation towards a new position
            {
                if (Vector3.Distance(oldPos, hit.point) > stepSize)
                {
                    //if the foot is too far from the target position start moving towards the new position
                    //changed transform.parent.position to transform.position, check this if the feet are not moving properly anymore
                    smoothStep = 0;
                    oldPos = newPos;
                    //int direction = transform.InverseTransformPoint(hit.point).x > transform.InverseTransformPoint(oldPos).x || transform.InverseTransformPoint(hit.point).z > transform.InverseTransformPoint(oldPos).z ? 1 : -1;
                    newPos = hit.point + stepSize * transform.forward;
                    if(sound != null)
                        sound.Play();
                }
            }
        }

        if(smoothStep < 1)
        {
            //interpolate between the current position and the target position, using a sin wave to move the foot above the ground
            Vector3 temp = Vector3.Lerp(oldPos, newPos, smoothStep);
            temp.y += Mathf.Sin(smoothStep * Mathf.PI) * stepHeight;
            currentPos = temp;
            smoothStep += Time.deltaTime * speed;
        }
    }
    private void OnEnable()
    {
        transform.localPosition = initialPos;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(newPos, .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(oldPos, .1f);
    }
}
