using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelGeneratorScript : MonoBehaviour
{
    private List<TileContainer> _tileContainers;
    
    [SerializeField]
    private List<TileBase> normalSprites;
    [SerializeField]
    private List<TileBase> graySprites;

    [SerializeField]
    private BattleSystemScript battleSystemScript;
    
    [SerializeField]
    private TileBase bossSprite;
    [SerializeField]
    private TileBase startSprite;
    [SerializeField]
    private Tilemap levelTileMap;

    [SerializeField] 
    private GameObject levelObj;
    [SerializeField] 
    private GameObject arenaObj;

    [SerializeField] 
    private GameObject player;
    [SerializeField]
    private TextMeshProUGUI gameTextBar;
    [SerializeField]
    private GameObject background;
    [SerializeField]
    private GameObject dPad;

    private PlayerControllerScript playerScript;

    private Vector3 oldPos;

    public bool isBattle = false;
    
    private const int MAX_LEVEL_SIZE = 18;
    private const int MIN_LEVEL_SIZE = 5;
    private const int MAX_SIDE_LEVEL_SIZE = 5;
    
    private void Awake()
    {
        _tileContainers = new List<TileContainer>();
        playerScript = player.GetComponent<PlayerControllerScript>();
        GenerateTiles();
    }
    
    //generates tile on level start
    private void GenerateTiles()
    {
        //level size
        var roomsCount = Random.Range(MIN_LEVEL_SIZE, MAX_LEVEL_SIZE);
        for (var i = 0; i < roomsCount; i++)
        {
            //generate start tile
            if (i == 0)
            {
                levelTileMap.SetTile(new Vector3Int(i, 0), startSprite);
                _tileContainers.Add(new TileContainer(normalSprites[Random.Range(0, normalSprites.Count)], 
                    new Vector3Int(i, 0), TileContainer.TileContent.Start));
            }
            //generate normal tiles
            else if (i < roomsCount - 1)
            {
                levelTileMap.SetTile(new Vector3Int(i, 0), graySprites[Random.Range(0, graySprites.Count)]);
                _tileContainers.Add(new TileContainer(normalSprites[Random.Range(0, normalSprites.Count)], 
                    new Vector3Int(i, 0), (TileContainer.TileContent)Random.Range(0, 3)));
                
                //calculating chance of additional tiles
                var chanceAddLevel = Random.Range(0, 100);
                switch (chanceAddLevel)
                {
                    case <= 20:
                        var countAddLevel = Random.Range(1, MAX_SIDE_LEVEL_SIZE);
                        for (var f = 1; f <= countAddLevel; f++)
                        {
                            levelTileMap.SetTile(new Vector3Int(i, f), graySprites[Random.Range(0, graySprites.Count)]);
                            _tileContainers.Add(new TileContainer(normalSprites[Random.Range(0, normalSprites.Count)], 
                                new Vector3Int(i, f), (TileContainer.TileContent)Random.Range(0, 3)));
                        }
                        break;
                    case >= 80:
                        var countAddLevelDown = Random.Range(1, MAX_SIDE_LEVEL_SIZE);
                        for (var f = -1; f >= -countAddLevelDown; f--)
                        {
                            levelTileMap.SetTile(new Vector3Int(i, f), graySprites[Random.Range(0, graySprites.Count)]);
                            _tileContainers.Add(new TileContainer(normalSprites[Random.Range(0, normalSprites.Count)], 
                                new Vector3Int(i, f), (TileContainer.TileContent)Random.Range(0, 3)));
                        }
                        break;
                }
            }
            //generate last tile, boss
            else
            {
                levelTileMap.SetTile(new Vector3Int(i, 0), bossSprite);
                _tileContainers.Add(new TileContainer(normalSprites[Random.Range(0, normalSprites.Count)], 
                    new Vector3Int(i, 0), TileContainer.TileContent.Boss));
            }
        }
    }
    
    //change tile from gray to normal
    public void ChangeTile(Vector3Int tileToChange)
    {
        var currentTile = _tileContainers.Find(e => e.tilePosition == tileToChange);
        if (currentTile == null || currentTile.content == TileContainer.TileContent.Boss || currentTile.content == TileContainer.TileContent.Start) return;

        levelTileMap.SetTile(tileToChange, currentTile.normalSprite);
    }

    //check event on tile
    public void CheckEvent(Vector3Int position)
    {
        var tileCont = _tileContainers.Find(e => e.tilePosition == position);
        if (tileCont == null || tileCont.isVisited)
        {
            gameTextBar.text = "";
            return;
        }

        switch (tileCont.content)
        {
            case TileContainer.TileContent.Item:
                tileCont.isVisited = true;
                ChooseItem();
                break;
            case TileContainer.TileContent.Enemy:
                tileCont.isVisited = true;
                gameTextBar.text = "There's an enemy on that tile!";
                StartCoroutine(LevelStartBattle(false));
                break;
            case TileContainer.TileContent.None:
                gameTextBar.text = "This room is empty";
                tileCont.isVisited = true;
                break;
            case TileContainer.TileContent.Boss:
                gameTextBar.text = "Boss fight!";
                StartCoroutine(LevelStartBattle(true));
                tileCont.isVisited = true;
                break;
            default:
               Debug.Log("Switch event default exception");
                break;
        }
    }

    //starts the battle
    private IEnumerator LevelStartBattle(bool isBoss)
    {
        dPad.SetActive(false);
        isBattle = true;
        yield return new WaitForSeconds(1f);
        levelObj.transform.DOScale(0, 1).From(1);
        player.transform.DOScale(0, 1).From(7.5f);
        yield return new WaitForSeconds(1f);
        levelObj.SetActive(false);
        arenaObj.SetActive(true);

        oldPos = player.transform.position;
        
        player.transform.position = battleSystemScript.playerSpawnPos.position;
        arenaObj.transform.DOScale(1, 1).From(0);
        player.transform.DOScale(7.5f, 1).From(0);
        yield return new WaitForSeconds(1f);

        gameTextBar.text = "";
        
        battleSystemScript.StartBattle(player, isBoss, oldPos);
    }

    //ending battle
    public IEnumerator EndBattle()
    {
        isBattle = false;
        yield return new WaitForSeconds(1f);
        arenaObj.transform.DOScale(0, 1).From(1);
        player.transform.DOScale(0, 1).From(7.5f);
        player.transform.position = oldPos;
        yield return new WaitForSeconds(1f);
        levelObj.SetActive(true);
        arenaObj.SetActive(false);
        levelObj.transform.DOScale(1, 1).From(0);
        player.transform.DOScale(7.5f, 1).From(0);
        yield return new WaitForSeconds(1f);
        dPad.SetActive(true);
    }

    //player defeated
    public void EndDefeated()
    {
        SceneManager.LoadScene(0);
        playerScript.Defeat();
    }

    //player wins boss
    public void EndVictory()
    {
        StartCoroutine(ReloadScene());
    }

    //reload scene
    private IEnumerator ReloadScene()
    {
        gameTextBar.text = "You defeat the boss!";
        background.SetActive(true);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(0);
        playerScript.LevelDone();
    }

    //choose the item on tile randomly
    private void ChooseItem()
    {
        switch (Random.Range(0, 3))
        {
            case 0:
                gameTextBar.text = "Item restored HP";
                playerScript.RestoreHP();
                break;
            case 1:
                gameTextBar.text = "Item increased HP";
                playerScript.IncreaseHp();
                break;
            case 2:
                gameTextBar.text = "Item increased strength";
                playerScript.IncreaseStrength();
                break;
            case 3:
                gameTextBar.text = "Item increased agility";
                playerScript.IncreaseAgility();
                break;
            default:
                Debug.Log("Switch event default exception");
                break;
        }
    }
}

