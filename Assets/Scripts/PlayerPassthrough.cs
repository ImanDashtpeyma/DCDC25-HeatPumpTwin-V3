using Fusion;
using UnityEngine;

// Migrated from Photon PUN 2 (PhotonView.IsMine) to Photon Fusion
// (NetworkObject.HasInputAuthority). Not currently attached to anything in
// the scene — there is no player avatar prefab yet (planned follow-up work);
// this will move onto that prefab once it exists.
public class PlayerPassthrough : NetworkBehaviour
{
    public GameObject passthroughLayer;

    public override void Spawned()
    {
        if (passthroughLayer)
            passthroughLayer.SetActive(HasInputAuthority);
    }
}
