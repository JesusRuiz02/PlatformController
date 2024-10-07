using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Basic Movement")] 
    [Range(0f, 1f)] [SerializeField] private float moveThreshold = 0.25f;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float airAcceleration;
    [SerializeField] private float airDeceleration;


    public float MoveThreshold => moveThreshold;
    public float MaxSpeed => maxSpeed;
    public float AirAcceleration => airAcceleration;
    public float AirDeceleration => airDeceleration;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    
    [Header("Health")] 
    [SerializeField] private int maxHealth;

    [Header("Jump Stats")] 
    [SerializeField] private float jumpDuration = 1.5f;
    [SerializeField] private int jumpsAllowed = 2;
    [SerializeField] private float jumpHeight = 6.5f;
   
    [SerializeField] private float maxFallspeed = 100f;
    [Range(0f, 1f)] [SerializeField] private float jumpCoyoteTimer = 0.1f;
    [Range(1f, 1.1f)] [SerializeField] private float jumpHeightCompensationFactor = 1.05f;
    [Range(0.01f, 5f)] [SerializeField] private float gravityReleaseMultiplier = 2f;
   
    public float MaxFallSpeed => maxFallspeed;
    public float GravityReleaseMultiplier => gravityReleaseMultiplier;
    public int JumpsAllowed => jumpsAllowed;
    public float JumpCoyoteTime => jumpCoyoteTimer;

    [Header("Dash Stats")] 
    [Range(1f,200f)] [SerializeField] private float dashSpeed = 40f;
    [Range(0f,1f)] [SerializeField] private float timeBtwDashesOnGround = 0.225f;
    [Range(0f,0.5f)][SerializeField] private float dashDiagonallyBias = 0.4f;
    [SerializeField] private int numberOfDashes = 2;
    [SerializeField] private float dashDuration = 0.11f;


    public float DashSpeed => dashSpeed;
    public float TimeBtwDashesOnGround => timeBtwDashesOnGround;
    public float DashDiagonallyBias => dashDiagonallyBias;
    public int NumberOfDashes => numberOfDashes;
    public float DashDuration => dashDuration;

    [Header("DashCancelTime")]
    [Range(0.01f, 5f)] [SerializeField] private float dashGravityOnReleaseMultiplier = 1f;
    [Range(0.02f, 0.3f)] [SerializeField] private float dashTimeForUpwardsCancel = 0.027f;

    public float DashTimeForUpwardsCancel => dashTimeForUpwardsCancel;
    public float DashGravityOnReleaseMultiplier => dashGravityOnReleaseMultiplier;
    
    public readonly Vector2[] DashDirections = new Vector2[]
    {
        new Vector2(0, 0), //Nothing
        new Vector2(1, 0), //Right
        new Vector2(1, 1).normalized, //TopRight
        new Vector2(0, 1), //Up
        new Vector2(-1, 1).normalized, //TopLeft
        new Vector2(-1, 0), //left
        new Vector2(-1, -1).normalized, //Bottom left
        new Vector2(0, -1), //Bottom
        new Vector2(1, -1).normalized //Bottom Right
    };
    
   

    [Header("Jump Cut")] 
    [Range(0.02f, 0.3f)] [SerializeField] private float timeForUpwardsCancel = 0.025f;

    public float TimeForUpdwardsCancel => timeForUpwardsCancel;

    [Header("JumpApex")] 
    [Range(0.5f, 1f)] [SerializeField] private float apexThreshold = 0.97f;
    [Range(0.01f, 1f)] [SerializeField] private float apexHangTime = 0.075f;

    public float ApexThreshold => apexThreshold;
    public float ApexHangTime => apexHangTime;

    [Header("JumpBuffer")]
    [Range(0f, 1f)] [SerializeField] private float jumpBufferTime = 0.125f;

    public float JumpBufferTime => jumpBufferTime;
    

    [Header("Physics")] 
    private float _gravity;
    private float _initialJumpVelocity;
    private float _adjustedJumpHeight;

    [SerializeField] private float stompMaxSpeed = 100f;
    [SerializeField] private float stompAcceleration = 20f;
    [SerializeField] private float stompDamage = 0f;

    private void OnValidate()
    {
        _adjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
        _gravity = -(2f * _adjustedJumpHeight) / Mathf.Pow(jumpDuration, 2f); 
        _initialJumpVelocity = Mathf.Abs(_gravity)* jumpDuration;
    }

    private void OnEnable()
    {
        _adjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
        _gravity = -(2f * _adjustedJumpHeight) / Mathf.Pow(jumpDuration, 2f); 
         _initialJumpVelocity = Mathf.Abs(_gravity)* jumpDuration;
    }

    public float StompDamage => stompDamage;
    public float StompMaxSpeed => stompMaxSpeed;
    public float InitialJumpVelocity => _initialJumpVelocity;
    public float StompAcceleration => stompAcceleration;
    public float Gravity => _gravity;
    

    [Header("Attacks")]
    [SerializeField] private float basicDamageAttack;
    [SerializeField] private float basicCooldownAttack;

    public float BasicDamageAttack => basicDamageAttack;
    public float BasicCooldownAttack => basicCooldownAttack;

    [Header("Collisions")]
    [SerializeField] private float groundDetectionRayLength;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float headDetectionRayLength = 0.2f;
    
    public float HeadDetectionRayLength => headDetectionRayLength;
    public LayerMask GroundLayer => groundLayer;
    public float GroundDetectionRayLength => groundDetectionRayLength;

}
