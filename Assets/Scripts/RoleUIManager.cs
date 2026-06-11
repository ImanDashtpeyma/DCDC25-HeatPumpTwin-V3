using Photon.Pun;
using UnityEngine;

public class RoleUIManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    public GameObject technicianPanel;
    public GameObject engineerPanel;

    void Start()
    {
        if (technicianPanel) technicianPanel.SetActive(false);
        if (engineerPanel)   engineerPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        bool isEngineer = PhotonNetwork.IsMasterClient;
        if (technicianPanel) technicianPanel.SetActive(!isEngineer);
        if (engineerPanel)   engineerPanel.SetActive(isEngineer);
    }
}