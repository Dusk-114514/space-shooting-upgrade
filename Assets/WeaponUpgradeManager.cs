using UnityEngine;
using UnityEngine.UI;

public class WeaponUpgradeManager : MonoBehaviour
{
    public GameObject weaponUpgradePanel;
    private PlayerShooting playerShooting;

    void Start()
    {
        playerShooting = FindObjectOfType<PlayerShooting>();
        if (weaponUpgradePanel != null) weaponUpgradePanel.SetActive(false);
        Debug.Log("WeaponUpgradeManager initialized. Panel: " + (weaponUpgradePanel != null) + ", Shooting: " + (playerShooting != null));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (weaponUpgradePanel != null)
            {
                weaponUpgradePanel.SetActive(!weaponUpgradePanel.activeSelf);
                Debug.Log("Panel toggled: " + weaponUpgradePanel.activeSelf);
            }
        }
    }

    public void SelectBasicWeapon()
    {
        if (playerShooting != null) playerShooting.SetWeapon(1);
        Debug.Log("Selected Basic Weapon");
        if (weaponUpgradePanel != null) weaponUpgradePanel.SetActive(false);
    }

    public void SelectShotgunWeapon()
    {
        if (playerShooting != null) playerShooting.SetWeapon(2);
        Debug.Log("Selected Shotgun Weapon");
        if (weaponUpgradePanel != null) weaponUpgradePanel.SetActive(false);
    }

    public void SelectLaserWeapon()
    {
        if (playerShooting != null) playerShooting.SetWeapon(3);
        Debug.Log("Selected Laser Weapon");
        if (weaponUpgradePanel != null) weaponUpgradePanel.SetActive(false);
    }
}