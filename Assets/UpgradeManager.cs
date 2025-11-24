using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public GameObject upgradePanel; // 升级面板
    public Button upgradeSpeedButton; // 速度升级按钮
    public Button upgradeWeaponButton; // 武器升级按钮
    public PlayerMovement playerMovement; // 玩家移动组件
    public PlayerShooting playerShooting; // 玩家射击组件
    public float speedIncrement = 1f; // 速度每次升级增量
    public float fireRateDecrement = 0.1f; // 火速每次升级减少量
    private int speedLevel = 1; // 速度等级
    private int weaponLevel = 1; // 武器等级 (初始 Basic)
    private float baseSpeed; // 存储玩家的初始速度
    private float baseFireRate; // 存储玩家的初始火速

    void Start()
    {
        // 初始禁用面板
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Debug.Log("Upgrade panel initialized and disabled.");
        }
        else
        {
            Debug.LogError("UpgradePanel is not assigned in Inspector!");
        }
        // 为按钮添加点击事件
        if (upgradeSpeedButton != null)
        {
            upgradeSpeedButton.onClick.AddListener(UpgradeSpeed);
            Debug.Log("Upgrade Speed button event listener added.");
        }
        else
        {
            Debug.LogError("UpgradeSpeedButton is not assigned in Inspector!");
        }
        if (upgradeWeaponButton != null)
        {
            upgradeWeaponButton.onClick.AddListener(UpgradeFireRate); // 改为 UpgradeFireRate (只升级射速)
            Debug.Log("Upgrade Fire Rate button event listener added.");
        }
        else
        {
            Debug.LogError("UpgradeWeaponButton is not assigned in Inspector!");
        }
        // 从PlayerMovement和PlayerShooting中读取初始值
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("No PlayerMovement component found in scene!");
            }
            else
            {
                Debug.Log("PlayerMovement found dynamically.");
            }
        }
        if (playerMovement != null)
        {
            baseSpeed = playerMovement.moveSpeed;
            Debug.Log("Base speed retrieved from PlayerMovement: " + baseSpeed);
        }
        else
        {
            Debug.LogError("PlayerMovement reference is null, cannot retrieve base speed!");
        }
        if (playerShooting == null)
        {
            playerShooting = FindObjectOfType<PlayerShooting>();
            if (playerShooting == null)
            {
                Debug.LogError("No PlayerShooting component found in scene!");
            }
            else
            {
                Debug.Log("PlayerShooting found dynamically.");
            }
        }
        if (playerShooting != null)
        {
            baseFireRate = playerShooting.fireRate; // 初始 Basic 0.5f
            Debug.Log("Base fire rate retrieved from PlayerShooting: " + baseFireRate);
        }
        else
        {
            Debug.LogError("PlayerShooting reference is null, cannot retrieve base fire rate!");
        }
    }

    void Update()
    {
        // 按下'U'键显示升级面板
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("U key pressed");
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
                Debug.Log("Upgrade panel enabled.");
            }
        }
        // 按下'Esc'键关闭升级面板
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc key pressed");
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
                Debug.Log("Upgrade panel disabled.");
            }
        }
    }

    void UpgradeSpeed()
    {
        speedLevel++;
        if (playerMovement != null && baseSpeed > 0)
        {
            float newSpeed = baseSpeed + (speedLevel - 1) * speedIncrement; // 基于初始速度增加
            playerMovement.moveSpeed = newSpeed;
            Debug.Log("Speed upgraded to: " + newSpeed);
        }
        else
        {
            if (playerMovement == null)
            {
                Debug.LogError("PlayerMovement reference is null!");
            }
            else if (baseSpeed <= 0)
            {
                Debug.LogError("Base speed is invalid: " + baseSpeed);
            }
        }
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Debug.Log("Upgrade panel disabled after speed upgrade.");
        }
    }

    void UpgradeFireRate() // 修复: 改为 UpgradeFireRate, 只升级射速 (不切换武器)
    {
        if (playerShooting != null && baseFireRate > 0)
        {
            playerShooting.UpgradeFireRate(fireRateDecrement); // 升级当前武器射速
            Debug.Log("Fire rate upgraded by " + fireRateDecrement);
        }
        else
        {
            if (playerShooting == null)
            {
                Debug.LogError("PlayerShooting reference is null!");
            }
            else if (baseFireRate <= 0)
            {
                Debug.LogError("Base fire rate is invalid: " + baseFireRate);
            }
        }
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Debug.Log("Upgrade panel disabled after fire rate upgrade.");
        }
    }
}