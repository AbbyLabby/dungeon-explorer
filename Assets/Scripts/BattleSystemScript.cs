using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystemScript : MonoBehaviour
{
    private GameObject player;
    private Vector3 oldPos;

    [SerializeField]
    private LevelGeneratorScript levelGeneratorScript;
    
    public List<GameObject> enemyPrefabs;
    
    public List<GameObject> bossPrefabs;
    
    public Transform playerSpawnPos;
    
    [SerializeField]
    private Transform enemySpawnPos;
    
    private PlayerControllerScript playerControllerScript;
    private EnemyScript enemyScript;

    public BattleState state;

    private GameObject enemyGO;

    private bool isBoss;

    
    //starting battle, set battle state to start
    public void StartBattle(GameObject player, bool isBoss, Vector3 oldPos)
    {
        state = BattleState.START;
        this.player = player;
        this.oldPos = oldPos;
        this.isBoss = isBoss;

        StartCoroutine(SetupBattle());
    }

    
    //setup battle, spawn enemy, set battle state to player turn
    private IEnumerator SetupBattle()
    {
        playerControllerScript = player.GetComponent<PlayerControllerScript>();

        if (!isBoss)
        {
            enemyGO = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], enemySpawnPos);
        }
        else
        {
            enemyGO = Instantiate(bossPrefabs[Random.Range(0, bossPrefabs.Count)], enemySpawnPos);
        }
        enemyScript = enemyGO.GetComponent<EnemyScript>();
        
        yield return new WaitForSeconds(2f);

        if (playerControllerScript.agility > enemyScript.agility)
        {
            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    //player turn, check if enemy die - player win, if not - enemy turn
    private IEnumerator PlayerTurn()
    {
        var isDead = enemyScript.UnderAttack(playerControllerScript.strength);

        player.transform.DOMove(new Vector3(0, -.5f, 0),.5f).OnComplete(() =>
        {
            player.transform.DOMove(playerSpawnPos.position, .5f);
        });

        yield return new WaitForSeconds(2f);

        if(!isDead)
        {
            state = BattleState.WON;
            EndBattle();
        } else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    //enemy turn, check if player die - player lose, if not - player turn
    private IEnumerator EnemyTurn()
    {

        var isDead = playerControllerScript.UnderAttack(enemyScript.strength);
        
        enemyGO.transform.DOMove(new Vector3(0, -.5f, 0), .5f).OnComplete(() =>
        {
            enemyGO.transform.DOMove(enemySpawnPos.position, .5f);
        });

        yield return new WaitForSeconds(2f);

        if(!isDead)
        {
            state = BattleState.LOST;
            EndBattle();
        } else
        {
            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerTurn());
        }
    }
    
    //end battle
    private void EndBattle()
    {
        if(state == BattleState.WON && isBoss)
        {
            Debug.Log("Player Win the boss!");
            playerControllerScript.EnemyDefeat(15);
            levelGeneratorScript.EndVictory();
        } 
        else if(state == BattleState.WON && !isBoss)
        {
            Debug.Log("Player Win!");
            playerControllerScript.EnemyDefeat(5);
            Destroy(enemyScript.gameObject);
            StartCoroutine(levelGeneratorScript.EndBattle());
        }        
        else if (state == BattleState.LOST)
        {
            levelGeneratorScript.EndDefeated();
            Debug.Log("Enemy Win!");
        }
    }
}
