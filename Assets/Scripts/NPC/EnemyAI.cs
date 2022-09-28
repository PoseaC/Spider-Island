using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
//for the love of god start commenting more often
public class EnemyAI : MonoBehaviour
{
    [Header("Chacteristics")]
    public float maxWanderDistance = 25f;
    public float damage = 10f;
    public float attentionSpan = 2.5f;
    public Animator animator;

    [Header("PathFinding & Movement")]
    public NavMeshAgent agent;
    public LayerMask whatIsGround;

    [Header("Inverse Kinematics Rig")]
    public IKFootSolver[] targets;

    [Header("Ragdoll Components")]
    public Transform[] bodyParts;

    Vector3 spawnPosition;
    bool waiting = true;
    bool attaking = false;
    GameManager manager;
    [HideInInspector] public Transform target;
    [HideInInspector] public bool spawnAvailable = true;
    Dictionary<GameObject, LayerMask> damageDictionary = new Dictionary<GameObject, LayerMask>();
    Dictionary<GameObject, (Vector3, Quaternion)> initialTransform = new Dictionary<GameObject, (Vector3, Quaternion)>();
    private void Start()
    {
        manager = FindObjectOfType<GameManager>();
        spawnPosition = transform.position;
        target = null;
        StartCoroutine(Wander());
        foreach (Transform limb in bodyParts)
        {
            damageDictionary.Add(limb.gameObject, limb.gameObject.layer);
            initialTransform.Add(limb.gameObject, (limb.localPosition, limb.localRotation));
        }
    }
    void Update()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, 10f, whatIsGround);
        agent.baseOffset = -info.distance + 1;
        if (target != null)
        {
            agent.SetDestination(target.position);
            if (agent.remainingDistance < agent.stoppingDistance && !attaking)
            {
                StartCoroutine(Attack());
            }
            if (Vector3.Distance(transform.position, target.position) > manager.playerViewDistance / 2)
            {
                target = null;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, manager.player.position) > manager.playerViewDistance)
            {
                StartCoroutine(GetComponentInChildren<DissolveEffect>().Dissolve(1, 0));
            }
            if (agent.remainingDistance < agent.stoppingDistance && waiting)
            {
                StartCoroutine(Wander());
            }
        }
    }
    void Damage()
    {
        target.GetComponent<PlayerHealth>().TakeDamage(damage);
    }
    IEnumerator Attack()
    {
        if (Vector3.Distance(target.position, transform.position) < agent.stoppingDistance) 
        {
            attaking = true;
            animator.Play("MeleeAttack");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0).Length + .1f);
            attaking = false;
        }
        else
            yield break;
    }
    IEnumerator Wander()
    {
        waiting = false;
        Vector3 targetPosition = Random.insideUnitSphere * maxWanderDistance + spawnPosition;
        NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, maxWanderDistance, 1);
        if (target == null)
            agent.SetDestination(hit.position);
        yield return new WaitForSeconds(attentionSpan);
        waiting = true;
    }
    public void Ragdoll()
    {
        GetComponent<RigBuilder>().enabled = false;
        GetComponent<Animator>().enabled = false;
        foreach (Transform limb in bodyParts)
        {
            limb.GetComponent<Rigidbody>().isKinematic = false;
            limb.gameObject.layer = LayerMask.NameToLayer("Pickable");
        }
        foreach(IKFootSolver target in targets)
        {
            target.enabled = false;
        }
        GetComponent<NavMeshAgent>().isStopped = true;
        target = null;
        FindObjectOfType<Spawner>().Drop(transform.position + Vector3.up * .5f);
        FindObjectOfType<Score>().UpdateScore();
    }
    public void Recycle()
    {
        spawnPosition = transform.position;
        foreach (Transform limb in bodyParts)
        {
            limb.gameObject.layer = damageDictionary[limb.gameObject];
            limb.GetComponent<Rigidbody>().isKinematic = true;
            limb.localRotation = initialTransform[limb.gameObject].Item2;
            limb.localPosition = initialTransform[limb.gameObject].Item1;
        }
        foreach (IKFootSolver target in targets)
        {
            target.enabled = true;
        }
        GetComponent<RigBuilder>().enabled = true;
        GetComponent<Animator>().enabled = true;
        GetComponent<NavMeshAgent>().isStopped = false;
        attaking = false;
        StartCoroutine(Wander());
    }
    private void OnEnable()
    {
        if (!spawnAvailable)
            Recycle();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.transform;
        }
    }
}
