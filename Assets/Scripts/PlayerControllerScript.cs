using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerControllerScript : MonoBehaviour
{
    private PlayerMovement _controls;
    
    [SerializeField]
    private Tilemap levelTilemap;

    [SerializeField] 
    private LevelGeneratorScript levelGeneratorScript;
    
    [SerializeField]
    private Image hpBar;
    [SerializeField]
    private Image expBar;
    [SerializeField]
    private TextMeshProUGUI strengthText;
    [SerializeField]
    private TextMeshProUGUI hpText;
    [SerializeField]
    private TextMeshProUGUI expText;
    [SerializeField]
    private TextMeshProUGUI lvlText;
    [SerializeField]
    private TextMeshProUGUI agilityText;

    public float experience;
    public float strength;
    public float health;
    public int level = 1;
    public int agility;

    private bool isMoving;

    [SerializeField]
    private float maxExp;
    [SerializeField]
    private float maxHp;

    private const float DEFAULT_HP = 10;
    private const float DEFAULT_STRENGTH = 1;
    private const int MAX_LEVEL = 10;

    private void Awake()
    {
        PlayerPrefs.DeleteAll();
        
        _controls = new PlayerMovement();

        if (!PlayerPrefs.HasKey("maxExp"))
        {
            PlayerPrefs.SetFloat("maxExp", DEFAULT_HP);
            PlayerPrefs.SetFloat("maxHp", DEFAULT_HP);
            PlayerPrefs.SetFloat("Health", DEFAULT_HP);
            PlayerPrefs.SetInt("Level", 1);
            PlayerPrefs.Save();
        }
        else
        {
            LoadStats();
        }

        strengthText.text = strength.ToString();
        lvlText.text = level.ToString();
        agilityText.text = agility.ToString();
        expBar.fillAmount = experience / maxExp;
        hpText.text = health + "/" + maxHp;
        expText.text = experience + "/" + maxExp;
        transform.position += Vector3.up;
    }

    private void LoadStats()
    {
        experience = PlayerPrefs.GetFloat("Experience");
        level = PlayerPrefs.GetInt("Level");
        maxExp = PlayerPrefs.GetFloat("maxExp");
        maxHp = PlayerPrefs.GetFloat("maxHP");
        health = PlayerPrefs.GetFloat("Health");
    }

    private void SaveStats()
    {
        PlayerPrefs.SetFloat("Experience", experience);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetFloat("maxExp", maxExp);
        PlayerPrefs.SetFloat("maxHP", maxHp);
        PlayerPrefs.SetFloat("Health", health);
        PlayerPrefs.Save();
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void Start()
    {
        _controls.Main.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
    }

    //moving the player
    private void Move(Vector2 direction)
    {
        if (CanMove(direction) && !levelGeneratorScript.isBattle && !isMoving)
        {
            isMoving = true;
            transform.DOJump(new Vector3(transform.position.x + direction.x, transform.position.y + direction.y), 1, 1, .75f).OnComplete(() =>
            {
                isMoving = false;
                levelGeneratorScript.CheckEvent(Vector3Int.FloorToInt(transform.position));
            });
        }
    }

    //check that player can move on that tile
    private bool CanMove(Vector2 direction)
    {
        var gridPosition = levelTilemap.WorldToCell(transform.position + (Vector3)direction);
        if (levelTilemap.HasTile(gridPosition))
        {
            levelGeneratorScript.ChangeTile(gridPosition);
            return true;
        }
        return false;
    }

    //player under attack
    public bool UnderAttack(float enemyStrength)
    {
        if (enemyStrength < health)
        {
            health -= enemyStrength;
            hpBar.fillAmount = health / maxHp;
            hpText.text = health + "/" + maxHp;
            
            SaveStats();
            
            Debug.Log("Player attacked: " + enemyStrength);
            Debug.Log("Player health: " + health);
            return true;
        }
        
        health = 0;
        hpBar.fillAmount = health / maxHp;
        hpText.text = health + "/" + maxHp;
        return false;
    }

    //if player kills enemy
    public void EnemyDefeat(float exp)
    {
        experience += exp;
        if (experience >= maxExp && level < MAX_LEVEL)
        {
            experience -= maxExp;
            maxExp *= 1.5f;
            level++;
            maxHp *= 1.25f;
            
            SaveStats();
            
            lvlText.text = level.ToString();
            hpBar.fillAmount = health / maxHp;
            hpText.text = health + "/" + maxHp;
        }

        expBar.fillAmount = experience / maxExp;
        expText.text = experience + "/" + maxExp;
    }

    //restore hp
    public void RestoreHP()
    {
        health = maxHp;
        hpBar.fillAmount = health / maxHp;
        hpText.text = health + "/" + maxHp;
        
        SaveStats();
        
        Debug.Log("Hp restored: " + health);
    }

    //increase hp
    public void IncreaseHp()
    {
        maxHp *= 1.25f;
        maxHp = Mathf.Round(maxHp);
        hpBar.fillAmount = health / maxHp;
        hpText.text = health + "/" + maxHp;
        
        SaveStats();
        
        Debug.Log("Hp increased, current hp: " + health + " Max hp: " + maxHp);
    }

    //increase strength
    public void IncreaseStrength()
    {
        strength *= 1.5f;
        strengthText.text = strength.ToString();
        SaveStats();
        Debug.Log("Strength increased, current strength: " + strength);
    }

    public void IncreaseAgility()
    {
        agility++;
        agilityText.text = agility.ToString();
        SaveStats();
        Debug.Log("Agility increased, current agility: " + strength);
    }

    //player die
    public void Defeat()
    {
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetFloat("Experience", experience);
        health = DEFAULT_HP;
        PlayerPrefs.SetFloat("Health", health);
        maxHp = DEFAULT_HP;
        PlayerPrefs.SetFloat("maxHP", maxHp);
        strength = DEFAULT_STRENGTH;
    }

    //player wins the boss
    public void LevelDone()
    {
        SaveStats();
        strength = DEFAULT_STRENGTH;
    }
}
