using UnityEngine;

// Migrated from Photon PUN 2 (MonoBehaviourPunCallbacks.OnJoinedRoom) to
// Photon Fusion — reacts to AppController.OnRoleResolved instead.
public class CalibrationUIManager : MonoBehaviour
{
    public GameObject engineerCalibBtn;
    public GameObject technicianAlignBtn;

    void Start()
    {
        // هر دو مخفی — تا role resolve بشه
        if (engineerCalibBtn)  engineerCalibBtn.SetActive(false);
        if (technicianAlignBtn) technicianAlignBtn.SetActive(false);
        AppController.OnRoleResolved += HandleRoleResolved;
    }

    void OnDestroy()
    {
        AppController.OnRoleResolved -= HandleRoleResolved;
    }

    void HandleRoleResolved(bool isEngineer)
    {
        if (engineerCalibBtn)  engineerCalibBtn.SetActive(isEngineer);
        if (technicianAlignBtn) technicianAlignBtn.SetActive(!isEngineer);
    }
}
