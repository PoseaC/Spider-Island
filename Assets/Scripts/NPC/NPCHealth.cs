using UnityEngine;
using TMPro;

public class NPCHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float dissolveDelay = 10f;
    public GameObject damageParticleParent; //rigidbody that parents the text with the damage done
    [HideInInspector] public TextMeshProUGUI damageParticle; //damage text itself

    float currentHealth;
    bool isDead = false;
    bool alreadyDied = false;

    void Start()
    {
        damageParticle = damageParticleParent.GetComponentInChildren<TextMeshProUGUI>();
        currentHealth = maxHealth;
    }
    private void Update()
    {
        if (isDead)
            Die();
    }
    public void TakeDamage(float damage, Vector3 damagePoint)
    {
        damageParticle.text = ((int) damage).ToString();
        Instantiate(damageParticleParent, damagePoint, Quaternion.identity);

        currentHealth -= damage;
        if (currentHealth <= 0)
            isDead = true;

        EnemyAI AI = GetComponent<EnemyAI>();
        if(AI.target == null)
        {
            AI.target = FindObjectOfType<PlayerMovement>().transform;
        }
    }

    public void Heal(float health)
    {
        currentHealth += health;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
    private void OnEnable()
    {
        currentHealth = maxHealth;
        alreadyDied = false;
        isDead = false;
    }
    public void Die()
    {
        if (alreadyDied)
            return;

        StartCoroutine(GetComponentInChildren<DissolveEffect>().Dissolve(1, dissolveDelay));
        GetComponent<EnemyAI>().Ragdoll();
        alreadyDied = true;
    }
}
