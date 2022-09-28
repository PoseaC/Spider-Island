using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    Rigidbody bullet; //body of the bullet
    public float force = 300f; //force applied when spawned
    public float lifespan = 5f; //how long before destroying the bullet
    public float pushBackForce = 10f; //if it hit a damageable object, how hard it will push on it
    public BulletHole bulletHole; //model spawned on contact with a non damageable surface
    [HideInInspector] public float damage = 10f; //how much damage the bullet deals
    [HideInInspector] public float critDamage = 20f; //how much damage the bullet deals
    [HideInInspector] public Vector3 target; //where the bullet should face before taking off
    void Start()
    {
        //rotate and add force to the bullet
        bullet = GetComponent<Rigidbody>();
        transform.LookAt(target);
        bullet.AddForce(transform.forward * force, ForceMode.Impulse);
        Destroy(gameObject, lifespan);
    }
    private void Update()
    {
        //constantly throw a raycast forward because at high velocities it's possible the bullet wouldn't detect a collision
        Ray hitScan = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(hitScan, out hit, force * 10))
            Hit(hit.collider, hit.point, hit.normal);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Hit(collision.collider, collision.GetContact(0).point, collision.GetContact(0).normal);
    }
    void Hit(Collider collider, Vector3 collisionPoint, Vector3 normal)
    {
        //don't shoot yourself
        if (collider.CompareTag("Player"))
            return;

        if (collider.CompareTag("Enemy"))
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Critical"))
                collider.GetComponentInParent<NPCHealth>().TakeDamage(critDamage, collisionPoint);
            else if (collider.gameObject.layer == LayerMask.NameToLayer("Damageable"))
                collider.GetComponentInParent<NPCHealth>().TakeDamage(damage, collisionPoint);
            else
            {
                collider.GetComponent<Rigidbody>().AddForce(transform.forward * pushBackForce, ForceMode.Impulse);
                Instantiate(bulletHole, collisionPoint, Quaternion.FromToRotation(-Vector3.forward, normal));
            }
        }
        else
        {
            Instantiate(bulletHole, collisionPoint, Quaternion.FromToRotation(-Vector3.forward, normal));
        }
        Destroy(gameObject);
    }
}
