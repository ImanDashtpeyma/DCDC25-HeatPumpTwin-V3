using Photon.Pun;
using UnityEngine;

public class RoleUIManager : MonoBehaviourPun
{
    [Header("Panels")]
    public GameObject technicianPanel;  // CanvasRoot اول (Power/Pressure)
    public GameObject engineerPanel;    // UI.info → Engineer_Buttons

    void Start()
    {
        bool isEngineer = PhotonNetwork.IsMasterClient;

        if (technicianPanel) technicianPanel.SetActive(!isEngineer);
        if (engineerPanel) engineerPanel.SetActive(isEngineer);
    }
}