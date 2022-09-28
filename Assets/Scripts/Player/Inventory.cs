using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class Inventory : MonoBehaviour
{
    [HideInInspector] public List<GunBehaviour> guns = new List<GunBehaviour>();
    public static GunBehaviour activeGun;
    public TextMeshProUGUI pickupNotification;
    public InventoryManager inventoryDisplay;
    void Start()
    {
        foreach(GunBehaviour gun in guns)
        {
            gun.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.mouseScrollDelta.y >= 1)
            CycleGuns(true);
        if (Input.mouseScrollDelta.y <= -1)
            CycleGuns(false);
    }
    public void Notification(string notif)
    {
        CancelInvoke();
        pickupNotification.text = notif;
        Invoke("ResetNotification", 1);
    }
    void ResetNotification()
    { 
        pickupNotification.text = "";
    }
    void CycleGuns(bool forward)
    {
        if (forward)
        {
            int index = guns.IndexOf(activeGun) + 1;
            if (index >= guns.Count)
                index = 0;
            ChangeGun(guns[index], false);
        }
        else
        {
            int index = guns.IndexOf(activeGun) - 1;
            if (index < 0)
                index = guns.Count - 1;
            ChangeGun(guns[index], false);
        }
    }
    public void ChangeGun(GunBehaviour gun, bool isDropped)
    {
        if(activeGun != null && !isDropped)
            activeGun.gameObject.SetActive(false);
        activeGun = gun;
        activeGun.gameObject.SetActive(true);
        inventoryDisplay.UpdateInventory(guns);
    }

    public void AddGun(GunBehaviour gun)
    {
        guns.Add(gun);
        ChangeGun(gun, false);
    }
    public void DropGun(GunBehaviour gun)
    {
        gun.pointer.SetActive(true);
        guns.Remove(gun);
        if (guns.Count > 0)
            ChangeGun(guns[guns.Count - 1], true);
        else
        {
            activeGun = null;
            inventoryDisplay.UpdateInventory(guns);
        }
    }
}
