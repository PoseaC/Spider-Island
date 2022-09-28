using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    AudioSource sound;
    private void Start()
    {
        sound = GetComponent<AudioSource>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        sound.Play();
    }
}
