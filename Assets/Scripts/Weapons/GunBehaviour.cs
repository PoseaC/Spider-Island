using UnityEngine;
using TMPro;
using System.Collections;
public class GunBehaviour : MonoBehaviour
{
    [Header("Ammo")]
    public float clipAmmo = 30; //how much ammo in a clip
    public float totalAmmo = 150; //how much ammo held in total

    [Header("Damage")]
    public float damage = 10; //how much damage the gun does
    public float critDamage = 20; //how much crit damage the gun does

    [Header("Randomness")]
    [Range(0, 100)] public float damageVariation = 5; //how much the damage cand vary

    [Header("Characteristics")]
    public float rateOfFire = .1f; //how fast does the gun shoot
    public float range = 100; //how far the raycast checks before spawning a bullet
    public bool isAutomatic = false; //if the button is held down, does the gun keep firing?
    public BulletBehaviour bullet; //the type of bullet of the gun
    public float recoil = 1f; //how much each shot recoils the camera
    public float recoverySpeed = .1f; //how fast does the camera recover from the recoil
    public float bulletForce = 10f; //how hard the bullet will impact the enemy
    //public float spread;

    [Header("Other")]
    public GameObject bulletHole;
    public Animator gunAnimator;
    public ParticleSystem muzzleFlash;
    public TextMeshProUGUI ammoDisplay;
    public TextMeshProUGUI notification;
    public Transform mainCamera;
    public GameObject pointer;

    [Header("Sound")]
    public AudioClip shot;
    public AudioClip reload;

    AudioSource sound;
    float currentAmmo = 30;
    float reloadTime = 1;
    bool fire = false;
    bool canFire = true;
    bool reloading = false;
    void Start()
    {
        foreach (AnimationClip clip in gunAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Reload")
            {
                reloadTime = clip.length;
                break;
            }
        }
        sound = GetComponent<AudioSource>();
    }
    void Update()
    {
        ammoDisplay.text = currentAmmo + " / " + totalAmmo;

        //input
        if (Input.GetButtonDown("Reload") && totalAmmo > 0 && !reloading)
            StartCoroutine(Reload());

        if (Input.GetButtonDown("Fire1"))
            fire = true;

        if (Input.GetButtonUp("Fire1"))
            fire = false;

        if (Input.GetButtonDown("Drop"))
            Drop();
    }
    void FixedUpdate()
    {
        if (fire)
            Fire();
    }
    void Drop()
    {
        FindObjectOfType<Inventory>().DropGun(this);
        transform.parent = null;
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Animator>().enabled = false;
        GetComponent<WeaponTilt>().enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Pickable");
        ammoDisplay.text = "- / -";
        enabled = false;
    }
    void Fire()
    {
        //canFire is used to control the rate of fire and not shoot on every FixedUpdate call
        if (canFire && !reloading)
        {
            if(currentAmmo <= 0)
            {
                StartCoroutine(Reload());
                return;
            }

            GetComponentInParent<CameraMovement>().Recoil(recoil, recoverySpeed);
            currentAmmo -= 1;

            float dmg = damage + Random.Range(-damageVariation, damageVariation) * damage / 100;
            float critDmg = critDamage + Random.Range(-damageVariation, damageVariation) * critDamage/100;

            //pazzaz
            sound.Play();
            muzzleFlash.Play();
            GetComponent<Animator>().enabled = true;
            gunAnimator.SetTrigger("shoot"); 
            GetComponent<LineRenderer>().SetPosition(0, muzzleFlash.transform.position);

            //shoot a raycast forward, if it hit something deal immediate damage, if not spawn a bullet 
            Ray shoot = new Ray(mainCamera.position, mainCamera.forward);
            RaycastHit hit;
            if (Physics.Raycast(shoot, out hit, range))
            {
                GetComponent<LineRenderer>().SetPosition(1, hit.point);

                //push on the hit object if you can
                if(hit.collider.GetComponent<Rigidbody>())
                    hit.collider.GetComponent<Rigidbody>().AddForce(mainCamera.forward * bulletForce, ForceMode.Impulse);

                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Damageable"))
                {
                    hit.collider.GetComponentInParent<NPCHealth>().damageParticle.color = Color.white;
                    hit.collider.GetComponentInParent<NPCHealth>().TakeDamage(dmg, hit.point);
                }
                else if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Critical"))
                {
                    hit.collider.GetComponentInParent<NPCHealth>().damageParticle.color = Color.red;
                    hit.collider.GetComponentInParent<NPCHealth>().TakeDamage(critDmg, hit.point);
                }
                else
                {
                    //oi, don't shoot yourself
                    if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Player"))
                        Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(-Vector3.forward, hit.normal));
                }
            }
            else
            {
                bullet.damage = dmg;
                bullet.critDamage = critDmg;
                bullet.target = mainCamera.position + mainCamera.forward * range;
                bullet.pushBackForce = bulletForce;

                //spawn the bullet a little behind the target otherwise it shoots straight up
                Instantiate(bullet, mainCamera.position + mainCamera.forward * (range - 1), Quaternion.identity);
                GetComponent<LineRenderer>().SetPosition(1, mainCamera.position + mainCamera.forward * range);
            }

            if (!isAutomatic)
                fire = false;

            canFire = false;
            Invoke("FireReady", rateOfFire);
        }
    }
    public void StopAnimator()
    {
        GetComponent<Animator>().enabled = false;
    }
    public IEnumerator Reload()
    {
        reloading = true;
        sound.clip = reload;
        sound.Play();
        notification.text = "Reloding...";
        GetComponent<Animator>().enabled = true;
        gunAnimator.SetTrigger("reloading");

        yield return new WaitForSeconds(reloadTime);//animation length

        if ((totalAmmo - (clipAmmo - currentAmmo)) > 0)
        {
            totalAmmo -= clipAmmo - currentAmmo;
            currentAmmo = clipAmmo;
        }
        else
        {
            currentAmmo += totalAmmo;
            totalAmmo = 0;
        }
        notification.text = "";
        sound.clip = shot;
        reloading = false;
    }
    void FireReady()
    {
        GetComponent<LineRenderer>().SetPosition(1, GetComponent<LineRenderer>().GetPosition(0));
        canFire = true;
    }
}
