using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region SingleTon

    private static InputManager _instance;
    public static InputManager GetInstance()
    {
        return _instance;
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        _playerInput = GetComponent<PlayerInput>();
        
        SetupInputActions();
    }

    #endregion
   

    #region  InputValues

    private Vector2 _moveInput;
    private bool _jumpJustPressed;
    private bool _jumpBeingHeld;
    private bool _jumpReleased;
    private bool _attackInput;
    private bool _dashInput;
    private bool _menuOpenCloseInput;
    private bool _stompInput;

    private PlayerInput _playerInput;
    
    private InputAction _stompAction;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _dashAction;
    private InputAction _menuOpenCloseAction;

    public Vector2 GetMovementInput()
    {
        return _moveInput;
    }
    public bool JumpJustPressed()
    {
        return _jumpJustPressed;
    }
    public bool JumpBeingHeld()
    {
        return _jumpBeingHeld;
    }
    public bool JumpReleased()
    {
        return _jumpReleased;
    }
    public bool DashInput()
    {
        return _dashInput;
    }
    public bool AttackInput()
    {
        return _attackInput;
    }
    public bool MenuOpenCloseInput()
    {
        return _menuOpenCloseInput;
    }
    public bool StompInput()
    {
        return _stompInput;
    }
    
    #endregion

   
    
    private void SetupInputActions()
    {
        _moveAction = _playerInput.actions["Movement"];
        _dashAction = _playerInput.actions["Dash"];
        _attackAction = _playerInput.actions["Attack"];
        _jumpAction = _playerInput.actions["Jump"];
        _menuOpenCloseAction = _playerInput.actions["MenuOpenClose"];
        _stompAction = _playerInput.actions["Stomp"];
    }

    private void UpdateInputs()
    {
        _stompInput = _stompAction.WasPressedThisFrame();
        _jumpBeingHeld = _jumpAction.IsPressed();
        _jumpReleased = _jumpAction.WasReleasedThisFrame();
        _jumpJustPressed = _jumpAction.WasPressedThisFrame();
        _moveInput = _moveAction.ReadValue<Vector2>();
        _attackInput = _attackAction.WasPressedThisFrame();
        _dashInput = _dashAction.WasPressedThisFrame();
        _menuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();
    }

    private void Update()
    {
        UpdateInputs();
    }
}
