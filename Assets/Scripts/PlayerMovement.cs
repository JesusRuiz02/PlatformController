using System;
using System.Collections;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")] 
    public PlayerStats playerStats;
    [SerializeField] private Transform fatherTransform;
    [SerializeField] private GameObject _attackCollider;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator _animator;

    public float HorizontalVelocity { get; private set; }
    private bool isFacingRight;
    [Header("GroundCheck")]
    
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _bumpCheck;
    private bool _isGrounded;
    
    private bool _bumpHead;

    [Header("Jumps")] 
    private float _jumpsUsed;
    public float VerticalVelocity { get; private set;}
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private PLAYERSTATE _playerstate = PLAYERSTATE.NORMAL_MOVEMENT;

    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    private float _coyoteTimer; 
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isfalling;

    [Header("dashVars")] 
    private bool _isDashing;
    private bool _isAirDashing;
    private float _dashTimer;
    private float _dashOnGroundTimer;
    private int _numberOfDashUsed;
    private Vector2 _dashDirection;
    private bool _isDashFastFalling;
    private float _dashFastFallTime;
    private float _dashFastFallReleaseSpeed;
    
    private void Awake()
    {
        isFacingRight = true;
    }
    private void Update()
    {
        CounterTimers();
        JumpCheck();
        StompCheck();
        LandCheck();
        DashCheck();
        AttackCheck();
        AnimationProvisionalVoid();
    }

    public PLAYERSTATE GetPlayerState()
    {
        return _playerstate;
    }

    #region Attacking

    private void AttackCheck()
    {
        if (InputManager.GetInstance().AttackInput() && _playerstate != PLAYERSTATE.STOMPING && _playerstate != PLAYERSTATE.ATTACKING)
        {
            Attack();   
        }
    }

    private void Attack()
    {
        StartCoroutine("Attacking");
        _playerstate = PLAYERSTATE.ATTACKING;
    }

    private IEnumerator Attacking()
    {
        _attackCollider.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        _attackCollider.SetActive(false);
        yield return new WaitForSeconds(playerStats.BasicCooldownAttack);
        _playerstate = PLAYERSTATE.NORMAL_MOVEMENT;
    }
    
    

    #endregion


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<IDamagable>() != null && _playerstate == PLAYERSTATE.STOMPING)
        {
           other.gameObject.GetComponent<IDamagable>().OnHit(playerStats.StompDamage);
        }
    }

    private void AnimationProvisionalVoid()
    {
        if(_isfalling || _isFastFalling || _isDashFastFalling || _playerstate == PLAYERSTATE.STOMPING )
        {
            _animator.Play("Falling");
        }
        else if (_isJumping)
        {
            _animator.Play("Jump");
        }
        else if(_playerstate == PLAYERSTATE.ATTACKING)
        {
            _animator.Play("Attack");
        }
        else
        {
            _animator.Play("Idle-Animation");
        }
    }
    private void FixedUpdate()
    {
        CollisionCheck();
        if (_playerstate == PLAYERSTATE.STOMPING)
        {
            if (!_isGrounded)
            {
                Stomp();
                return;
            } 
            
            Landing();
            
        }
        Jump();
        Fall();
        Dash();
        if (_isGrounded)
        {
            Move(playerStats.Acceleration, playerStats.Deceleration, InputManager.GetInstance().GetMovementInput());
        }
        else
        {
            Move(playerStats.AirAcceleration, playerStats.AirDeceleration, InputManager.GetInstance().GetMovementInput());
        }
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -playerStats.MaxFallSpeed, 50f);
        rb.velocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }

    #region Dash

    private void DashCheck()
    {
        if (InputManager.GetInstance().DashInput())
        {
            if (_isGrounded && _dashOnGroundTimer < 0 && !_isDashing)
            {
                InitiateDash();
            }
            else if (!_isGrounded && !_isDashing && _numberOfDashUsed < playerStats.NumberOfDashes)
            {
                _isAirDashing = true;
                InitiateDash();
            }
        }
    }

    private void InitiateDash()
    {
        _dashDirection = InputManager.GetInstance().GetMovementInput();
        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(_dashDirection, playerStats.DashDirections[0]);
        for (int i = 0; i <playerStats.DashDirections.Length; i++)
        {
            if (_dashDirection ==  playerStats.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }

            float distance = Vector2.Distance(_dashDirection, playerStats.DashDirections[i]);
            bool isDiagonal = (Mathf.Abs(playerStats.DashDirections[i].x) == 1 && Mathf.Abs(playerStats.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= playerStats.DashDiagonallyBias;
            }
            else if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = playerStats.DashDirections[i];
            }
        }

        if (closestDirection == Vector2.zero)
        {
            closestDirection = isFacingRight ? Vector2.right : Vector2.left;
        }

        _dashDirection = closestDirection;
        _numberOfDashUsed++;
        _isDashing = true;
        _dashTimer = 0f;
        _dashOnGroundTimer = playerStats.TimeBtwDashesOnGround;
        ResetJumpValues();
    }

    private void Dash()
    {
        if (_isDashing)
        {
            _dashTimer += Time.fixedDeltaTime;
            if (_dashTimer >= playerStats.DashDuration)
            {
                if (_isGrounded)
                {
                    ResetDashes();
                }

                _isAirDashing = false;
                _isDashing = false;

                if (!_isJumping)
                {
                    _dashFastFallTime = 0f;
                    _dashFastFallReleaseSpeed = VerticalVelocity;
                    if (!_isGrounded)
                    {
                        _isDashFastFalling = true;
                    }
                }
                return;
            }
            HorizontalVelocity = playerStats.DashSpeed * _dashDirection.x;
            if (_dashDirection.y != 0f || _isAirDashing )
            {
                VerticalVelocity = playerStats.DashSpeed * _dashDirection.y;
            }
        }
        else if(_isDashFastFalling)
        {
            if (VerticalVelocity > 0f)
            {
                if (_dashFastFallTime < playerStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity = Mathf.Lerp(_dashFastFallReleaseSpeed, 0f, (_dashFastFallTime / playerStats.DashTimeForUpwardsCancel));
                }
                else if(_dashFastFallTime >= playerStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity += playerStats.Gravity * playerStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                VerticalVelocity += playerStats.Gravity * playerStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
        }
    }

    private void ResetDashValues()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }

    private void ResetDashes()
    {
        _numberOfDashUsed = 0;
    }

    #endregion
    #region Jump

    private void ResetJumpValues()
    {
        _isJumping = false;
        _isfalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
    }
    
    private void JumpCheck()
    {
        if (InputManager.GetInstance().JumpJustPressed())
        {
            _jumpBufferTimer = playerStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        if (InputManager.GetInstance().JumpReleased())
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = playerStats.TimeForUpdwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            } 
        }

        
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f)) 
        { 
            InitiateJump(1); 
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        else if(_jumpBufferTimer > 0f && (_isJumping || _isAirDashing || _isDashFastFalling ) && _jumpsUsed < playerStats.JumpsAllowed)
        {
                _isFastFalling = false;
                InitiateJump(1);
                if (_isDashFastFalling)
                {
                    _isDashFastFalling = false;
                }
        }
        else if (_jumpBufferTimer > 0f && _isfalling  && _jumpsUsed < playerStats.JumpsAllowed - 1) 
        {
                InitiateJump(2);
                _isFastFalling = false; 
        }
        
        
    }
    
    private void Jump()
    {
        if (_isJumping)
        {
            if (_bumpHead)
            {
                _isFastFalling = true;
            }

            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(playerStats.InitialJumpVelocity, 0f, VerticalVelocity);
                if (_apexPoint > playerStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < playerStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity -= 0.01f;
                        }
                    }
                }
                else if(!_isFastFalling)
                {
                    VerticalVelocity += playerStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            else if(!_isFastFalling)
            {
                VerticalVelocity += playerStats.Gravity * playerStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
            }
            
            else if (VerticalVelocity < 0f)
            {
                if (_isfalling)
                {
                    _isfalling = true;
                }
            }
        }

        if (_isFastFalling)
        {
            if (_fastFallTime >= playerStats.TimeForUpdwardsCancel)
            {
                VerticalVelocity += playerStats.Gravity * playerStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < playerStats.TimeForUpdwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f,
                    (_fastFallTime / playerStats.TimeForUpdwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }
    }

    private void InitiateJump(int numbersOfJumpUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true; 
        }
        _jumpBufferTimer = 0f;
        _jumpsUsed += numbersOfJumpUsed;
        VerticalVelocity = playerStats.InitialJumpVelocity;
    }
    
    private void Landing()
    {
        _isJumping = false;
        _isfalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _jumpsUsed = 0;
        _playerstate = PLAYERSTATE.NORMAL_MOVEMENT;
        _isPastApexThreshold = false;
        VerticalVelocity = Physics2D.gravity.y;
    }

    #endregion

    #region CollisionCheck
    private void GroundCheck()
    {
        _isGrounded = false;
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(_groundCheck.position, playerStats.GroundDetectionRayLength, playerStats.GroundLayer);
        if (collider2Ds.Length > 0)
        {
            _isGrounded = true;
        }
    }
    private void BumpCheck()
    {
        _bumpHead = false;
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(_bumpCheck.position, playerStats.HeadDetectionRayLength, playerStats.GroundLayer);
        if (collider2Ds.Length > 0)
        {
            _bumpHead = true;
        }
    }
    
    private void CollisionCheck()
    {
        GroundCheck();
        BumpCheck();  
    }
    

    #endregion

    #region LandingCheck

    private void LandCheck()
    {
        if ((_isJumping || _isfalling || _isDashFastFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            ResetDashes();
            ResetJumpValues();
            _jumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;

            if (_isDashFastFalling && _isGrounded)
            {
                ResetDashValues();
                return;
            }
            ResetDashValues();
        }

    }

    private void Fall()
    {
        if (!_isGrounded && !_isJumping)
        {
            if (!_isfalling)
            {
                _isfalling = true;
            }
            VerticalVelocity += playerStats.Gravity * Time.fixedDeltaTime;
        }
    }
    

    #endregion
    #region Stomping

    private void StompCheck()
    {
        if (InputManager.GetInstance().StompInput() && (_playerstate != PLAYERSTATE.STOMPING || _playerstate != PLAYERSTATE.PAUSE) && !_isGrounded)
        {
            _playerstate = PLAYERSTATE.STOMPING;
        }
    }

    private void Stomp()
    {
        _playerstate = PLAYERSTATE.STOMPING;
        rb.velocity = Vector2.zero;
        if (rb.velocity.y >= -playerStats.StompMaxSpeed)
        {
            rb.velocity -= new Vector2(0, playerStats.StompAcceleration);
        }
        
    }
    
    #endregion
    
    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!_isDashing)
        {
            if (Mathf.Abs(moveInput.x) >= playerStats.MoveThreshold )
            {
                TurnCheck(moveInput);
                
                float targetVelocity = 0f;
                targetVelocity = moveInput.x * playerStats.MaxSpeed;
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(moveInput.x) < playerStats.MoveThreshold)
            {
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
            }
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if(!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            fatherTransform.transform.Rotate(0f,180f,0f);
        }
        else
        {
            isFacingRight = false; 
            fatherTransform.transform.Rotate(0f,-180f,0f);
        }
    }
    

    #endregion

    #region Timers

    private void CounterTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = playerStats.JumpCoyoteTime;
        }

        if (_isGrounded)
        {
            _dashOnGroundTimer -= Time.deltaTime;
        }
    }
    

    #endregion
}
public enum PLAYERSTATE //All possible Game States
{
    NORMAL_MOVEMENT,
    STOMPING,
    DEATH,
    PAUSE,
    ATTACKING
    
}
