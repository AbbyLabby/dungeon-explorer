using UnityEngine;
using UnityEngine.Tilemaps;

public class TileContainer
{
    
    //tile states
    public enum TileContent
    {
        None,
        Enemy,
        Item,
        Start,
        Boss
    }

    public TileBase normalSprite;

    public Vector3Int tilePosition;

    public TileContent content;

    //the tile is visited or not
    public bool isVisited = false;
    
    public TileContainer(TileBase _sprite, Vector3Int _position, TileContent tileContent)
    {
        normalSprite = _sprite;
        tilePosition = _position;
        content = tileContent;
    }
    
}
