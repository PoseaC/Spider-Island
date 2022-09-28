using UnityEngine;
using UnityEngine.UI;
public class PickupScript : MonoBehaviour
{
    public float range = 5f; //how far the player can grab objects
    public float strength = 500f; //strength of the spring
    public LayerMask pickableObject; //what objects the player can grab
    public Camera mainCamera; //reference to the player's camera
    public Transform pickupPoint; //where the picked up object will be attached
    public Image pickupIndicator; //indicator when the player can grab an object
    public LineRenderer line; //line to add some extra pazzaz when picking objects
    public Transform inventory;
    float distance = 0; //the distance at which the object was picked up
    public static GameObject pickupObject; //local variable to keep track of the object being picked up
    void Update()
    {
        //shoot a raycast forward to check if there is a pickable object ahead
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, pickableObject))
        {
            //indicate to the player if there is and set the appropriate variables
            pickupIndicator.enabled = true;
            if (Input.GetKeyDown(KeyCode.E))
            {
                pickupObject = hit.collider.gameObject;
                distance = hit.distance;

                //ragdolls are dragged around with springs, objects are just parented
                if (pickupObject.CompareTag("Enemy"))
                {
                    //add a spring joint if the object doesn't face one
                    if (!pickupObject.GetComponent<SpringJoint>())
                        pickupObject.AddComponent<SpringJoint>();

                    //configure the joint
                    SpringJoint connection = pickupObject.GetComponent<SpringJoint>();
                    connection.connectedBody = pickupPoint.GetComponent<Rigidbody>();
                    connection.spring = strength;
                    connection.damper = 1;
                    connection.autoConfigureConnectedAnchor = false;
                    connection.connectedAnchor = Vector3.zero;
                    connection.anchor = transform.InverseTransformPoint(hit.point);
                } 
                else if (pickupObject.CompareTag("Gun"))
                {
                    //configure the gun for use when picked up
                    pickupObject.transform.SetParent(inventory);
                    pickupObject.transform.SetPositionAndRotation(inventory.position, inventory.rotation);
                    pickupObject.GetComponent<Rigidbody>().isKinematic = true;
                    pickupObject.GetComponent<BoxCollider>().enabled = false;
                    pickupObject.GetComponent<GunBehaviour>().enabled = true;
                    pickupObject.GetComponent<GunBehaviour>().pointer.SetActive(false);
                    pickupObject.GetComponent<Animator>().enabled = true;
                    pickupObject.GetComponent<WeaponTilt>().enabled = true;
                    pickupObject.layer = LayerMask.NameToLayer("Overlay");
                    FindObjectOfType<Inventory>().AddGun(pickupObject.GetComponent<GunBehaviour>());
                    pickupObject = null;
                }
                else
                {
                    pickupObject.transform.parent = pickupPoint.transform;
                    pickupObject.GetComponent<Rigidbody>().isKinematic = true;
                    pickupObject.GetComponent<MeshCollider>().enabled = false;
                }
            }
        }
        else
        {
            pickupIndicator.enabled = false;
        }

        if (pickupObject != null)
        {
            if (Input.GetKey(KeyCode.E))
            {
                //move the point attached to keep it in the center of the camera
                if (Physics.Raycast(ray, out RaycastHit hitCheck, distance))
                {
                    pickupPoint.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, hitCheck.distance - 1));
                }
                else
                {
                    pickupPoint.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
                }

                //activate pazzas
                if (pickupObject.CompareTag("Enemy"))
                {
                    line.SetPositions(new Vector3[] { pickupPoint.position, pickupObject.transform.position });
                }
                else
                {
                    pickupObject.transform.position = pickupPoint.transform.position;
                }
            }
            else
            {
                if (pickupObject.CompareTag("Enemy"))
                {
                    //when the player lets go of the object break the joint
                    pickupObject.GetComponent<SpringJoint>().breakForce = 0f;
                }
                else
                {
                    pickupObject.transform.parent = null;
                    pickupObject.GetComponent<Rigidbody>().isKinematic = false;
                    pickupObject.GetComponent<MeshCollider>().enabled = true;
                }
                pickupObject = null;

                //deactivate pazzaz
                line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            }
            pickupIndicator.enabled = false;
        }
        else
        {
            line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        }
    }
}
