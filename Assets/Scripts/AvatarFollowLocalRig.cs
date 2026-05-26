using Photon.Pun;
using UnityEngine;

public class AvatarFollowLocalRig : MonoBehaviourPun
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Local Rig Targets")]
    public Transform headTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    void LateUpdate()
    {
        if (!photonView.IsMine) return;

        if (headTarget) { head.position = headTarget.position; head.rotation = headTarget.rotation; }
        if (leftHandTarget) { leftHand.position = leftHandTarget.position; leftHand.rotation = leftHandTarget.rotation; }
        if (rightHandTarget) { rightHand.position = rightHandTarget.position; rightHand.rotation = rightHandTarget.rotation; }
    }
}