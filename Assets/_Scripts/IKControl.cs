using UnityEngine;
using System.Collections;

//Uses Unity's IK controls to move the hand to where the wand is (approximately)
public class IKControl : MonoBehaviour
{
	public Animator animator;				//The player's animator, for applying IK transformations
	public GameObject rightHandTarget;		//The tracker object used to IK the right hand
	public GameObject leftHandTarget;		//The tracker object used to IK the left hand
	public Transform cameraTransform;		//The camera
	public GameObject rightHand;			//The bone for the right hand, so we can rotate the hand/geiger counter manually
	private float distanceValue;			//The distance between the hand and the camera, used for slerping the rotation of the geiger counter

	public void OnAnimatorIK()
	{
		//Override Right Hand Animation with PPT Tracking
		if (rightHandTarget != null)
		{
			animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
			animator.SetIKPosition (AvatarIKGoal.RightHand, rightHandTarget.transform.position);

            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.transform.parent.rotation); // set the rotation of the hand
		}
		else
		{
			animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 0);
		}

		//Override Left Hand Animation with PPT Tracking
		if (leftHandTarget != null)
		{
			animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
			animator.SetIKPosition (AvatarIKGoal.LeftHand, leftHandTarget.transform.position);

            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.transform.parent.rotation);
		}
		else
		{
			animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 0);
		}

		//Enable head look-at function to give shoulder and chest directionality
		if (cameraTransform != null)
		{
			animator.SetLookAtWeight (1);
			animator.SetLookAtPosition (cameraTransform.position + cameraTransform.forward);
		}
		else
		{
			animator.SetLookAtWeight (0);
		}
	}

    private void Start()
    {
        
    }

    private void Update()
    {
        if (rightHandTarget == null || leftHandTarget == null || cameraTransform == null)
        {
            findAllComponents();
        }
    }

    void LateUpdate()
	{
		//Apply some rotation functions to the hand to have it rotate properly to face the camera when it's close to it.
		if (rightHand != null)
		{
			Quaternion originalRot = rightHand.transform.rotation;
			rightHand.transform.LookAt (cameraTransform.position + Vector3.up);
			rightHand.transform.Rotate (0, 90, 0, Space.Self);

			distanceValue = Vector3.Distance (rightHand.transform.position, cameraTransform.position);

			rightHand.transform.rotation = Quaternion.Slerp (originalRot, rightHand.transform.rotation, Map.map (distanceValue, 0.25f, 0.45f, 1, 0));
			rightHand.transform.Rotate (0, Map.map (distanceValue, 0.25f, 0.45f, 0, -40), 0, Space.Self);
		}
	}

    private void findAllComponents()
    {
        // find all the relevant components
        this.rightHandTarget = GameObject.Find("rightHandTarget");
        this.leftHandTarget = GameObject.Find("leftHandTarget");
        this.cameraTransform = GameObject.Find("Camera (eye)").transform;
    }
}

//Maps one range to another range. The most useful function ever made!
public class Map : MonoBehaviour
{
    public static float map(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }
}
