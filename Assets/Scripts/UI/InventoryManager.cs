using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public InventoryGun[] inventory;
    private void Start()
    {
        foreach (InventoryGun gun in inventory)
        {
            gun.gameObject.SetActive(false);
        }
    }
    public void UpdateInventory(List<GunBehaviour> guns)
    {
        foreach(InventoryGun gun in inventory)
        {
            gun.gameObject.SetActive(false);
        }
        for(int i = 0; i < guns.Count; i++)
        {
            inventory[i].gameObject.SetActive(true);
            bool active = false;
            if (guns[i] == Inventory.activeGun)
                active = true;
            inventory[i].SetGun(guns[i].name, active);
        }
    }
}
