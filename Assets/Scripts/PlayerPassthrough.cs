using Photon.Pun;
using UnityEngine;

public class PlayerPassthrough : MonoBehaviour
{
    public GameObject passthroughLayer;

    void Start()
    {
        passthroughLayer.SetActive(GetComponent<PhotonView>().IsMine);
    }
}