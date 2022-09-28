using UnityEngine;
using UnityEngine.UI;

public class InventoryGun : MonoBehaviour
{
    public Text nameDisplay;
    public Image selectedIcon;

    public void SetGun(string name, bool active)
    {
        selectedIcon.enabled = active;
        nameDisplay.text = name;
    }
}
