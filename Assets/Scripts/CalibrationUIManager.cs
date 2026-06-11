using Photon.Pun;
using UnityEngine;

public class CalibrationUIManager : MonoBehaviourPunCallbacks
{
    public GameObject engineerCalibBtn;
    public GameObject technicianAlignBtn;

    void Start()
    {
        // هر دو مخفی — تا room join بشه
        if (engineerCalibBtn)  engineerCalibBtn.SetActive(false);
        if (technicianAlignBtn) technicianAlignBtn.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        bool isEngineer = PhotonNetwork.IsMasterClient;
        if (engineerCalibBtn)  engineerCalibBtn.SetActive(isEngineer);
        if (technicianAlignBtn) technicianAlignBtn.SetActive(!isEngineer);
    }
}
