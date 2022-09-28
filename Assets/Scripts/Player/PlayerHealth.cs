using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float healthChangeStep = .1f;
    public Slider healthBar;
    public Slider actualHealthBar;
    public GameObject deathScreen;
    public AudioClip hit;
    public AudioClip death;
    AudioSource sound;

    [HideInInspector] public float currentHealth;

    void Start()
    {
        sound = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        healthBar.maxValue = actualHealthBar.maxValue = maxHealth;
        healthBar.value = actualHealthBar.value = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        StopAllCoroutines();
        sound.clip = hit;
        sound.Play();
        actualHealthBar.value = currentHealth - damage;
        StartCoroutine(HealthChange(currentHealth - damage));
    }

    public void Heal(float health)
    {
        if (currentHealth + health > maxHealth)
            StartCoroutine(HealthChange(maxHealth));
        else
            StartCoroutine(HealthChange(currentHealth + health));
    }

    IEnumerator HealthChange(float targetHealth)
    {
        if(targetHealth > currentHealth)
        {
            actualHealthBar.value = targetHealth;
            while(currentHealth < targetHealth)
            {
                currentHealth += healthChangeStep;
                healthBar.value = currentHealth;
                yield return null;
            }
        }
        else
        {
            if (targetHealth <= 0)
                Die();
            while (currentHealth > targetHealth)
            {
                currentHealth -= healthChangeStep;
                healthBar.value = currentHealth;
                yield return null;
            }
        }
    }

    public void Die()
    {
        sound.clip = death;
        sound.Play();
        FindObjectOfType<PauseMenu>().Pause(true);
        FindObjectOfType<Score>().SetHighScore();
        deathScreen.SetActive(true);
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach(EnemyAI enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}
