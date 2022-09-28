using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public enum PickupType {Ammo, Health};
    public PickupType item;
    public int amount = 10;
    public float movingSpeed = 1f;
    public float rotationSpeed = 1f;
    public float verticalAmplitude = 1f;
    public Vector3 startPosition;
    public AudioClip sound;
    private void Start()
    {
        startPosition = transform.position;
    }
    private void FixedUpdate()
    {
        transform.position = startPosition + Mathf.Sin(Time.time * movingSpeed * Mathf.PI) * verticalAmplitude * Time.fixedDeltaTime * Vector3.up;
        transform.Rotate(rotationSpeed * Time.fixedDeltaTime * Vector3.up);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(item == PickupType.Health)
            {
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player.currentHealth < player.maxHealth)
                {
                    player.Heal(amount);
                    FindObjectOfType<Inventory>().Notification("+" + amount + " health");
                    other.GetComponent<AudioSource>().clip = sound;
                    other.GetComponent<AudioSource>().Play();
                    gameObject.SetActive(false);
                }
            }
            else if(other.transform.parent.GetComponentInChildren<Inventory>().guns.Count > 0)
            {
                Inventory inventory = other.transform.parent.GetComponentInChildren<Inventory>();
                foreach(GunBehaviour gun in inventory.guns)
                {
                    gun.totalAmmo += amount;
                }
                FindObjectOfType<Inventory>().Notification("+" + amount + " ammo");
                other.GetComponent<AudioSource>().clip = sound;
                other.GetComponent<AudioSource>().Play();
                gameObject.SetActive(false);
            }
        }
    }
}
