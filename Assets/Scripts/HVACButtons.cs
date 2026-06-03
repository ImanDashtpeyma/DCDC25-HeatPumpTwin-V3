using UnityEngine;

public class HVACButtons : MonoBehaviour
{
    private TwinNetworkHub _hub;

    void Start()
    {
        _hub = FindObjectOfType<TwinNetworkHub>();
    }

    public void OnApprove() => _hub?.Engineer_Approve();
    public void OnReject() => _hub?.Engineer_Reject();
}