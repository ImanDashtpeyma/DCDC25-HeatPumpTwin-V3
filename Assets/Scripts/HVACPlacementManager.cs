using Photon.Pun;
using UnityEngine;

public class HVACPlacementManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public GameObject hvacRoot;
    public GameObject previewObject;  // یه cube نیمه شفاف

    [Header("Settings")]
    public float rayDistance = 3f;

    private bool _placed = false;
    private Vector3 _previewPos;
    private bool _validHit = false;

private Vector3 _hitPos;
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_placed) return;

        // Ray از وسط camera به پایین
        var cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            _previewPos = hit.point;
            _validHit = true;

            if (previewObject != null)
            {
                previewObject.SetActive(true);
                previewObject.transform.position = _previewPos;
            }
        }
        else
        {
            _validHit = false;
            if (previewObject != null)
                previewObject.SetActive(false);
        }
    }

    // این رو به یه دکمه UI وصل کن — Engineer فقط
    public void PlaceHVAC()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_placed || !_validHit) return;

        _placed = true;
        if (previewObject != null) previewObject.SetActive(false);

        photonView.RPC(nameof(RPC_PlaceHVAC), RpcTarget.All,
            _previewPos.x, _previewPos.y, _previewPos.z);

        // Colocation anchor همینجا بساز
        if (ColocationManager.Instance != null)
            ColocationManager.Instance.CreateAndShareAnchor(_previewPos);
    }

    [PunRPC]
    void RPC_PlaceHVAC(float x, float y, float z)
    {
        var pos = new Vector3(x, y, z);
        if (hvacRoot != null)
        {
            hvacRoot.transform.position = pos;
            hvacRoot.SetActive(true);
        }
        Debug.Log("✅ HVAC placed at: " + pos);
    }
}