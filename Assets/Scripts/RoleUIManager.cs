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

        // This component lives on the HVAC prefab, which only spawns well
        // after colocation + a pin press — by then AppController already
        // resolved the role once, early in the session, and a C# event
        // doesn't replay past invocations to a late subscriber. Without
        // this, neither panel's SetActive ever runs again, so both stay
        // hidden (or, if their references are stale, both stay at whatever
        // the prefab's default active state was) — apply the already-known
        // role immediately instead of waiting for an event that already fired.
        HandleRoleResolved(AppController.IsEngineer);
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
