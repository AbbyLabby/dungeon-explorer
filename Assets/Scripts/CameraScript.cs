using DG.Tweening;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private LevelGeneratorScript levelGeneratorScript;

    //camera always looking at player if not battle, and looks at arena if battle
    private void Update()
    {
        if (!levelGeneratorScript.isBattle)
        {
            transform.DOMove(new Vector3(player.position.x, player.position.y, -5), 1f);
        }
        else
        {
            transform.DOMove(new Vector3(0, 0, -5), 1f).SetDelay(1f).Delay();
        }
    }
}
