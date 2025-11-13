using UnityEngine;

public class CamTriggerCol : MonoBehaviour
{
    public bool isTriggerd = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.LogWarning("°É·Áµû!");
            isTriggerd = true;
        
            GameManager.Instance.cc.camStatus = CamStatus.CinematicMode;
            CameraManager.Instance.MoveAndRotate(GameManager.Instance.camPosition.position, GameManager.Instance.camPosition.eulerAngles, 3.0f);
            
        }
    }
}
