﻿using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using MRTKExtensions.Gesture;
using UnityEngine;

// adapted from work of Joost van Schaik <https://github.com/LocalJoost/DYIPinchGrab>
namespace DyiPinchGrab
{
    public class DyiHandManipulation : MonoBehaviour
    {
        [SerializeField]
        private TrackedHandJoint trackedHandJoint = TrackedHandJoint.IndexMiddleJoint;

        [SerializeField]
        private float grabDistance = 0.1f;

        [SerializeField]
        private Handedness trackedHand = Handedness.Both;

        [SerializeField]
        private bool trackPinch = true;

        [SerializeField]
        private bool trackGrab = true;

        public float bounceDelay = 0.125f;
        private float lastBounce = -1.0f;
        private Rigidbody rb;
        

        private IMixedRealityHandJointService handJointService;

        private IMixedRealityHandJointService HandJointService =>
            handJointService ??
            (handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>());

        private MixedRealityPose? previousLeftHandPose;

        private MixedRealityPose? previousRightHandPose;

        private void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (rb != null && rb.velocity != Vector3.zero) return;


            var leftHandPose = GetHandPose(Handedness.Left, previousLeftHandPose != null);
            var rightHandPose = GetHandPose(Handedness.Right, previousRightHandPose != null);
            {
                var jointTransform = HandJointService.RequestJointTransform(trackedHandJoint, trackedHand);
                if (rightHandPose != null && previousRightHandPose != null)
                {
                    if (leftHandPose != null && previousLeftHandPose != null)
                    {
                        // fight! pick the closest one
                        var isRightCloser = Vector3.Distance(rightHandPose.Value.Position, jointTransform.position) <
                                            Vector3.Distance(leftHandPose.Value.Position, jointTransform.position);

                        ProcessPoseChange(
                            isRightCloser ? previousRightHandPose : previousLeftHandPose,
                            isRightCloser ? rightHandPose : leftHandPose);
                    }
                    else
                    {
                        ProcessPoseChange(previousRightHandPose, rightHandPose);
                    }
                }
                else if (leftHandPose != null && previousLeftHandPose != null)
                {
                    ProcessPoseChange(previousLeftHandPose, leftHandPose);
                }
            }
            previousLeftHandPose = leftHandPose;
            previousRightHandPose = rightHandPose;
        }

        private MixedRealityPose? GetHandPose(Handedness hand, bool hasBeenGrabbed)
        {
            if ((trackedHand & hand) == hand)
            {
                if (HandJointService.IsHandTracked(hand) &&
                    ((GestureUtils.IsPinching(hand) && trackPinch) ||
                     (GestureUtils.IsGrabbing(hand) && trackGrab)))
                {
                    var jointTransform = HandJointService.RequestJointTransform(trackedHandJoint, hand);
                    var palmTransForm = HandJointService.RequestJointTransform(TrackedHandJoint.Palm, hand);
                    
                    if(hasBeenGrabbed || 
                       Vector3.Distance(gameObject.transform.position, jointTransform.position) <= grabDistance)
                    {
                        return new MixedRealityPose(jointTransform.position, palmTransForm.rotation);
                    }
                }
            }

            return null;
        }
        
        private void ProcessPoseChange(MixedRealityPose? previousPose, MixedRealityPose? currentPose)
        {
            //instead of calculating the delta position and rotation, snap the gameobject to where the tracked hand is
            //in this case, position is from IndexMiddleJoint, and rotation is from Palm
            gameObject.transform.position = currentPose.Value.Position;
            gameObject.transform.rotation = currentPose.Value.Rotation;
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "femur" && Time.time > lastBounce + bounceDelay)
            { 
                collision.rigidbody.AddForce(collision.contacts[0].normal * -1.0f);
                lastBounce = Time.time;
            }
        }
    }
}