using UnityEngine;
using UnityEngine.Networking;

public class GunPositionSync : NetworkBehaviour
{

    //could see this being useful for sending physics transforms when flipping things over
    [SerializeField] Transform cameraTransform; // find where camera is 
    [SerializeField] Transform handMount;  //where am i basing the position of my gun off of
    [SerializeField] Transform gunPivot; //where is the gun?
    [SerializeField] Transform rightHandHold; 
    [SerializeField] Transform leftHandHold;
    [SerializeField] float threshold = 10f; // i dont want to sync the pitch and position of my gun every frame - only if gun has moved more than 10 degrees
    [SerializeField] float smoothing = 5f; //lerp the position

    [SyncVar] float pitch;
    Vector3 lastOffset;
    float lastSyncedPitch;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (isLocalPlayer)
            gunPivot.parent = cameraTransform; // parent it to camera
        else
            lastOffset = handMount.position - transform.position; // record diff in position from the hand amount and the actual player gameobject. every frame, calculate distance of the bob, move gun that much as well
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            pitch = cameraTransform.localRotation.eulerAngles.x; //update pitch based on the camera transform
            if (Mathf.Abs(lastSyncedPitch - pitch) >= threshold) 
            {
                CmdUpdatePitch(pitch); // pass in new pitch
                lastSyncedPitch = pitch; 
            }
        }
        else
        {
            Quaternion newRotation = Quaternion.Euler(pitch, 0f, 0f); // storing the rotation to prevent gimbal lock

            Vector3 currentOffset = handMount.position - transform.position; //how far off am i
            gunPivot.localPosition += currentOffset - lastOffset; // how much has the players object moved
            lastOffset = currentOffset; 

            gunPivot.localRotation = Quaternion.Lerp(gunPivot.localRotation,// dont just set the rotation, cuz it would snap, so lerp, you want to get to the new rotation, 
                                                     newRotation, Time.deltaTime * smoothing); //you use time.delta time to smooth it
        }
    }

    [Command]
    void CmdUpdatePitch(float newPitch)
    {
        pitch = newPitch;
    }

    void OnAnimatorIK() 
    {
        if (!anim) // in case animator needs to be turned off
            return;
        //Start doing the IK stuff, normalyy we use FK, where motion is captured from the root node outward
        //IK starts from extremities moves to root
        //All you need to do is reference the humanoid avatar for IK, since Unity already figures where these are
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f); // setting the weight for the right hand
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f); 
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandHold.position);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandHold.rotation);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandHold.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandHold.rotation);
    }
}