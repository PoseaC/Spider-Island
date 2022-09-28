using UnityEngine;
using System.Collections;
public class DissolveEffect : MonoBehaviour
{
    public SkinnedMeshRenderer rend;
    public float speed = .01f;
    EnemyAI parent;
    private void Start()
    {
        parent = GetComponentInParent<EnemyAI>();
    }
    private void OnEnable()
    {
        StartCoroutine(Dissolve(0, 0));
    }
    public IEnumerator Dissolve(int target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(PickupScript.pickupObject == transform.parent.gameObject)
        {
            PickupScript.pickupObject = null;
        }
        float dissolvingState = rend.material.GetFloat("VisibilityStage");
        if (dissolvingState > target)
        {
            parent.spawnAvailable = false;
            parent.enabled = true;
            parent.gameObject.SetActive(true);
            while (rend.material.GetFloat("VisibilityStage") > target)
            {
                dissolvingState -= speed;
                rend.material.SetFloat("VisibilityStage", dissolvingState);
                yield return null;
            }
        } 
        else
        {
            while (rend.material.GetFloat("VisibilityStage") < target)
            {
                dissolvingState += speed;
                rend.material.SetFloat("VisibilityStage", dissolvingState);
                yield return null;
            }
            parent.target = null;
            parent.gameObject.SetActive(false);
            parent.enabled = false;
            parent.spawnAvailable = true;
        }
    }
}
