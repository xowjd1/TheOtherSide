using UnityEngine;

public class ShovelFinTrigger : MonoBehaviour
{
    bool isTriggerd = false;
    public Rigidbody handShovelrb;
    public BoxCollider handShovelBoxCollider;
    public ShovelUser shovelUser;
    
    private void OnTriggerEnter(Collider other)
    {
        if(!isTriggerd && GameManager.Instance.status == GameStatus.ShovelMission)
        {
            isTriggerd = true;

            GameManager.Instance.status = GameStatus.TapeMission;
            GameManager.Instance.SetCompleteUI();
            GameManager.Instance.OnMissionComplete();

            handShovelBoxCollider.enabled = true;
            handShovelrb.isKinematic = false;
            handShovelrb.useGravity = true;
            handShovelrb.gameObject.transform.SetParent(null);
            
            var su = shovelUser ? shovelUser : other.GetComponentInParent<ShovelUser>();
            if (su)
            {
                su.ShovelEnd();
                su.enabled = false;
            }
        }
    }
}
