using Photon.Pun;
using TMPro;
using UnityEngine;

public class AvatarNameTag : MonoBehaviourPun
{
    public TMP_Text label;

    void Start()
    {
        if (!label) return;

        // مثلا MasterClient = Engineer
        var role = PhotonNetwork.IsMasterClient == photonView.IsMine ? "Engineer" : "Technician";
        // بالا ممکنه اشتباه شه چون IsMasterClient مربوط به لوکاله
        // بهتر: ownerActorNumber == MasterClient.ActorNumber
        var isEngineer = photonView.OwnerActorNr == PhotonNetwork.MasterClient.ActorNumber;
        label.text = isEngineer ? "Engineer" : "Technician";
    }

    void LateUpdate()
    {
        // رو به دوربین نگاه کن
        if (Camera.main) transform.forward = (transform.position - Camera.main.transform.position).normalized;
    }
}