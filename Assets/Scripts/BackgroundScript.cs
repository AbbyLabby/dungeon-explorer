using UnityEngine;

public class BackgroundScript : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform;
    
    private void Update()
    {
        gameObject.transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, 1);
    }
}
