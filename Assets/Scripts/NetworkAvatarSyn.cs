using Photon.Pun;
using UnityEngine;

public class NetworkAvatarSync : MonoBehaviourPun, IPunObservable
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    void Start()
    {
        if (photonView.IsMine)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;   // خودت رو قایم کن
        }
    }

    void Awake()
    {
        // اگر لوکال پلیر هستی، می‌تونی رنگ متفاوت بدی
        if (photonView.IsMine)
        {
            SetColor(new Color(0.2f, 0.8f, 0.2f, 1f)); // سبز برای خودت
        }
        else
        {
            SetColor(new Color(0.2f, 0.4f, 0.9f, 1f)); // آبی برای نفر مقابل
        }
    }

    void SetColor(Color c)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.material.color = c;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(head.position);
            stream.SendNext(head.rotation);

            stream.SendNext(leftHand.position);
            stream.SendNext(leftHand.rotation);

            stream.SendNext(rightHand.position);
            stream.SendNext(rightHand.rotation);
        }
        else
        {
            head.position = (Vector3)stream.ReceiveNext();
            head.rotation = (Quaternion)stream.ReceiveNext();

            leftHand.position = (Vector3)stream.ReceiveNext();
            leftHand.rotation = (Quaternion)stream.ReceiveNext();

            rightHand.position = (Vector3)stream.ReceiveNext();
            rightHand.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}