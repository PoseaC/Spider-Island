using UnityEngine;

public class DamageParticle : MonoBehaviour
{
    public float popForce;
    public float lifetime;

    [Range(0,1)]
    public float spread = .2f;
    [Range(0, 1)]
    public float textScale = .2f;
    [Range(1, 10)]
    public float popScale = 5;

    Transform player;
    Rigidbody particle;
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>().transform;
        particle = GetComponent<Rigidbody>();
        float distance = Vector3.Distance(player.position, transform.position);
        particle.transform.localScale *= distance * textScale;

        float x = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);
        Vector3 direction = Vector3.up + new Vector3(x, 0, z);
        particle.AddForce(direction * distance * popScale * popForce, ForceMode.Impulse);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.LookAt(player);
    }

}
