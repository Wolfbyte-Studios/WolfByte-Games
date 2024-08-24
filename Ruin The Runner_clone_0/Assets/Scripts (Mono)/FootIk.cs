using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetGrounder : MonoBehaviour
{
    //Ik Feel Position and Rotation Values
    private Vector3 _rightFootPosition, _leftFootPosition, _leftFootIkPosition, _rightFootIkPosition;

    private Quaternion _leftFootIkRotation,
        _rightFootIkRotation,
        _currentLeftFootIkRotation,
        _currentRightFootIkRotation;
    private float _lastPelvisPositionY, _lastRightFootPositionY, _lastLeftFootPositionY;
    private Animator _animator;
    private Transform _leftFootTransform, _rightFootTransform;

    [Header("Feet Grounder")]
    public bool enableFeetIk = true;
    [Range(0, 2)][SerializeField] private float heightFromGroundRaycast = 1.14f;
    [Range(0, 2)][SerializeField] private float raycastDownDistance = 1.5f;

    [SerializeField] private LayerMask enviromentLayer;
    [SerializeField] private float pelvisOffset = 0f;
    [Range(0, 1)][SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
    [Range(0, 1)] public float feetToIkPositionSpeed = 0.5f;

    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";

    public bool useProIkFeature = false;
    public bool showSolverDebug = true;
    

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (enableFeetIk == false) { return; }
        if (_animator == null) { return; }

        AdjustFeetTarget(ref _rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref _leftFootPosition, HumanBodyBones.LeftFoot);

        //find and raycast to the ground to find positions
        FeetPositionSolver(_rightFootPosition, ref _rightFootIkPosition, ref _rightFootIkRotation);// handle the solver for right foot
        FeetPositionSolver(_leftFootPosition, ref _leftFootIkPosition, ref _leftFootIkRotation);//Left foot Solver


    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (enableFeetIk == false) { return; }
        if (_animator == null) { return; }

        _currentLeftFootIkRotation = _animator.GetIKRotation(AvatarIKGoal.LeftFoot);
        _currentRightFootIkRotation = _animator.GetIKRotation(AvatarIKGoal.RightFoot);

        MovePelvisHeight();

        //right foot Ik position and rotation -- utilise the pro feature here
        _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        if (useProIkFeature)
        {
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _animator.GetFloat(rightFootAnimVariableName));
        }
        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, _rightFootIkPosition, _rightFootIkRotation, ref _lastRightFootPositionY, _currentRightFootIkRotation);

        //Left foot Ik position and rotation -- utilise the pro feature here
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        if (useProIkFeature)
        {
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _animator.GetFloat(leftFootAnimVariableName));
        }
        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, _leftFootIkPosition, _leftFootIkRotation, ref _lastLeftFootPositionY, _currentLeftFootIkRotation);
    }

    private void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder,
        ref float lastFootPositionY, Quaternion currRot)
    {
        Vector3 targetIkPosition = _animator.GetIKPosition(foot);
        Quaternion nextRot = rotationIkHolder * currRot;

        if (positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPosition.y += yVariable;
            lastFootPositionY = yVariable;
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            _animator.SetIKRotation(foot, nextRot);
        }
        _animator.SetIKPosition(foot, targetIkPosition);
    }

    private void MovePelvisHeight()
    {
        if (_rightFootIkPosition == Vector3.zero || _leftFootIkPosition == Vector3.zero ||
            _lastPelvisPositionY == 0)
        {
            _lastPelvisPositionY = _animator.bodyPosition.y;
            return;
        }

        float lOffsetPosition = _leftFootIkPosition.y - transform.position.y;
        float rOffsetPosition = _rightFootIkPosition.y - transform.position.y;

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;
        Vector3 newPelvisPosition = _animator.bodyPosition + Vector3.up * totalOffset;

        newPelvisPosition.y = Mathf.Lerp(_lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);
        _animator.bodyPosition = newPelvisPosition;
        _lastPelvisPositionY = _animator.bodyPosition.y;
    }

    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        //raycast section - locating the feet position via a raycast and solving
        RaycastHit feetOutHit;

        if (showSolverDebug)
        {
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down *
                (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
        }

        if (Physics.Raycast(fromSkyPosition, Vector3.down,
                out feetOutHit, raycastDownDistance + heightFromGroundRaycast, enviromentLayer))
        {
            float rotationStiffnessSlerpValue = Time.deltaTime * 8f;
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
            Quaternion feetHitRot = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal);
            feetIkRotations = Quaternion.Slerp(feetIkRotations, feetHitRot, rotationStiffnessSlerpValue);

            Debug.DrawLine(feetIkPositions, feetOutHit.normal * (raycastDownDistance + heightFromGroundRaycast), Color.magenta);


            return; ;
        }
        feetIkPositions = Vector3.zero;//it didnt work
    }

    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = _animator.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }
}