using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    public float strength;
    public float health;
    public float maxHP;
    public int agility;

    [SerializeField]
    private Image hpBar;
    [SerializeField] 
    private TextMeshProUGUI strengthText;
    [SerializeField] 
    private TextMeshProUGUI hpText;
    [SerializeField]
    private TextMeshProUGUI agilityText;

    private void Awake()
    {
        strengthText.text = strength.ToString();
        agilityText.text = agility.ToString();
        hpText.text = health + "/" + maxHP;
    }

    //enemy under attack
    public bool UnderAttack(float playerStrength)
    {
        if (playerStrength < health)
        {
            health -= playerStrength;
            hpBar.fillAmount = health / maxHP;
            hpText.text = health + "/" + maxHP;
            Debug.Log("Enemy attacked: " + playerStrength);
            Debug.Log("Enemy health: " + health);
            return true;
        }
        health = 0;
        hpBar.fillAmount = health / maxHP;
        hpText.text = health + "/" + maxHP;
        return false;
    }
}
