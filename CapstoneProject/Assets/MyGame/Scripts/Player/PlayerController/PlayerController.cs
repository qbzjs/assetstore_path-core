using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;


public enum CharacterState
{
    Default,
}

public enum OrientationMethod
{
    TowardsCamera,
    TowardsMovement,
}

public enum BonusOrientationMethod
{
    None,
    TowardsGravity,
    TowardsGroundSlopeAndGravity,
}

public class PlayerController : NetworkBehaviour, ICharacterController
{
    [SerializeField] private PlayerInputController _PlayerInputController;

    public PlayerCharacterCamera CharacterCamera;

    public KinematicCharacterMotor Motor;

    [Header("Stable Movement")] public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15f;
    public float OrientationSharpness = 10f;
    public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

    [Header("Air Movement")] public float MaxAirMoveSpeed = 15f;
    public float AirAccelerationSpeed = 15f;
    public float Drag = 0.1f;

    [Header("Jumping")] public bool AllowJumpingWhenSliding = false;
    public float JumpUpSpeed = 10f;
    public float JumpScalableForwardSpeed = 10f;
    public float JumpPreGroundingGraceTime = 0f;
    public float JumpPostGroundingGraceTime = 0f;

    [Header("Misc")] public List<Collider> IgnoredColliders = new List<Collider>();
    public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
    public float BonusOrientationSharpness = 10f;
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public Transform CameraFollowPoint;
    public float CrouchedCapsuleHeight = 1f;

    public CharacterState CurrentCharacterState { get; private set; }

    private Collider[] _probedColliders = new Collider[8];
    private RaycastHit[] _probedHits = new RaycastHit[8];
    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private Vector3 _internalVelocityAdd = Vector3.zero;
    private bool _shouldBeCrouching = false;
    private bool _isCrouching = false;

    private Vector3 lastInnerNormal = Vector3.zero;
    private Vector3 lastOuterNormal = Vector3.zero;


    private void Awake()
    {
        TransitionToState(CharacterState.Default);

        Motor.CharacterController = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Tell camera to follow transform
        CharacterCamera.SetFollowTransform(CameraFollowPoint);

        // Ignore the character's collider(s) for camera obstruction checks
        CharacterCamera.IgnoredColliders.Clear();
        CharacterCamera.IgnoredColliders.AddRange(GetComponentsInChildren<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void FixedUpdate()
    {
    }

    private void LateUpdate()
    {
    }

    //Eventually can be moved to a seperate class.

    #region HandleCameraStuff

    #endregion


    #region Server Functions

    #endregion

    #region Client Functions

    #endregion

    #region Commands

    #endregion

    #region ClientRPC_Commands

    #endregion

    #region Target_RPC_Commands

    #endregion

    #region General Functions

    private void HandleKinematicsControlValueAssignments()
    {
 
        
        
    }

    /// <summary>
    /// Handles movement state transitions and enter/exit callbacks
    /// </summary>
    public void TransitionToState(CharacterState newState)
    {
        CharacterState tmpInitialState = CurrentCharacterState;
        OnStateExit(tmpInitialState, newState);
        CurrentCharacterState = newState;
        OnStateEnter(newState, tmpInitialState);
    }

    /// <summary>
    /// Event when entering a state
    /// </summary>
    public void OnStateEnter(CharacterState state, CharacterState fromState)
    {
        switch (state)
        {
            case CharacterState.Default:
            {
                break;
            }
        }
    }

    /// <summary>
    /// Event when exiting a state
    /// </summary>
    public void OnStateExit(CharacterState state, CharacterState toState)
    {
        switch (state)
        {
            case CharacterState.Default:
            {
                break;
            }
        }
    }

    #endregion"

    //These are the implementations for the icharacter controller.
    #region ICharacterController_Interface_Implementations

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        throw new System.NotImplementedException();
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    #endregion
}

