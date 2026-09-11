using UnityEngine;

// Migrated from Photon PUN 2 (MonoBehaviourPunCallbacks.OnJoinedRoom) to
// Photon Fusion — reacts to AppController.OnRoleResolved instead.
public class RoleUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject technicianPanel;
    public GameObject engineerPanel;

    void Start()
    {
        if (technicianPanel) technicianPanel.SetActive(false);
        if (engineerPanel)   engineerPanel.SetActive(false);
        AppController.OnRoleResolved += HandleRoleResolved;
    }

    void OnDestroy()
    {
        AppController.OnRoleResolved -= HandleRoleResolved;
    }

    void HandleRoleResolved(bool isEngineer)
    {
        if (technicianPanel) technicianPanel.SetActive(!isEngineer);
        if (engineerPanel)   engineerPanel.SetActive(isEngineer);
    }
}
