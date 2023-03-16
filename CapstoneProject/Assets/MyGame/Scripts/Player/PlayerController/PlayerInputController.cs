using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using Mono.CSharp;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Event = Mono.CSharp.Event;

public class PlayerInputController : MonoBehaviour
{
    //Reference to the whoele inputactionasset which contains everything.
    [SerializeField] private InputActionAsset controls;

    //Reference to the action map inside of the controls inputactionasset. - by default this is player.
    [SerializeField] private InputActionMap _inputActionMap;

    //Reference to a playerinput class which comes with inputsystem,
    //i use this to check if the gamecontroller has changed from keyboard to gamepad.
    [SerializeField] private PlayerInput _playerInput;

    //Reference to the actions inside of the controls - inputactionasset.
    public InputAction move_Action,
        look_Action,
        jump_Action,
        crouch_Action,
        sprint_Action,
        switchPerspectiveCamera_Action,
        interact_Action,
        pause_Action,
        quickslot_1_action,
        quickslot_2_action,
        quickslot_3_action,
        quickslot_4_action,
        console_action,
        dropitem_action,
        inventory_action,
        attack_action;
    
    
    //our reference to the values of the look and move from the action
    public Vector2 move;
    public Vector2 look;

    //the references to the values from the actions when we press or perform an action
    public bool jump,
        interact,
        sprint,
        crouch,
        pause,
        inventory,
        switchperspective,
        dropitem,
        attack,
        console,
        quickslot_1,
        quickslot_2,
        quickslot_3,
        quickslot_4;

    //our current mouse settings
    [Header("Mouse settings")] public bool cursorLocked = false;
    public bool cursorInputForLook = true;
    public float MouseSensitivity = 1f;

    
    
    
    public void OnEnable()
    {
        //Assign the inputactionmap to the player inputaction map that i have setup inside of my input action asset.
        _inputActionMap = controls.FindActionMap("Player");

        //subscribe to the oncontrolschanged event to run our function
        //when the controls type (gamepad or mouse and keyboard) has been changed.
        _playerInput.onControlsChanged += ControlsChanged;

        //by default we lock the cursor so it doesnt move around the screen.
        Cursor.lockState = CursorLockMode.Locked;

        move_Action.performed += OnMove;
        move_Action.canceled += OnEndMove;

        look_Action.performed += OnLook;
        look_Action.canceled += OnEndLook;

        crouch_Action.performed += OnCrouch;
        crouch_Action.canceled += OnEndCrouch;

        jump_Action.performed += OnJump;
        jump_Action.canceled += OnEndJump;

        sprint_Action.performed += OnSprint;
        sprint_Action.canceled += OnEndSprint;

        switchPerspectiveCamera_Action.performed += OnSwitchCameraPerspective;
        switchPerspectiveCamera_Action.canceled += OnEndSwitchCameraPerspective;

        interact_Action.performed += OnInteract;
        interact_Action.canceled += OnEndInteract;

        pause_Action.performed += OnPause;

        quickslot_1_action.performed += OnQuickSlotOne;
        quickslot_1_action.canceled += OnEndQuickSlotOne;

        quickslot_2_action.performed += OnQuickSlotTwo;
        quickslot_2_action.canceled += OnEndQuickSlotTwo;

        quickslot_3_action.performed += OnQuickSlotThree;
        quickslot_3_action.canceled += OnEndQuickSlotThree;

        quickslot_4_action.performed += OnQuickSlotFour;
        quickslot_4_action.canceled += OnEndQuickSlotFour;

        console_action.performed += OnConsole;

        inventory_action.performed += OnInventory;

        dropitem_action.performed += OnDropItem;
        dropitem_action.canceled += OnDropItemEnd;

        attack_action.performed += OnAttack;
        attack_action.canceled += OnEndAttack;
    }

    public void OnDisable()
    {
        //unsubscribe to the oncontrolschanged event to avoid memory leaks this will disconnect it from the function.
        _playerInput.onControlsChanged -= ControlsChanged;

        move_Action.performed -= OnMove;
        move_Action.canceled -= OnEndMove;

        look_Action.performed -= OnLook;
        look_Action.canceled -= OnEndLook;

        crouch_Action.performed -= OnCrouch;
        crouch_Action.canceled -= OnEndCrouch;

        jump_Action.performed -= OnJump;
        jump_Action.canceled -= OnEndJump;

        sprint_Action.performed -= OnSprint;
        sprint_Action.canceled -= OnEndSprint;

        switchPerspectiveCamera_Action.performed -= OnSwitchCameraPerspective;
        switchPerspectiveCamera_Action.canceled -= OnEndSwitchCameraPerspective;

        interact_Action.performed -= OnInteract;
        interact_Action.canceled -= OnEndInteract;

        pause_Action.performed -= OnPause;

        quickslot_1_action.performed -= OnQuickSlotOne;
        quickslot_1_action.canceled -= OnEndQuickSlotOne;

        quickslot_2_action.performed -= OnQuickSlotTwo;
        quickslot_2_action.canceled -= OnEndQuickSlotTwo;

        quickslot_3_action.performed -= OnQuickSlotThree;
        quickslot_3_action.canceled -= OnEndQuickSlotThree;

        quickslot_4_action.performed -= OnQuickSlotFour;
        quickslot_4_action.canceled -= OnEndQuickSlotFour;

        console_action.performed -= OnConsole;

        inventory_action.performed -= OnInventory;

        dropitem_action.performed -= OnDropItem;
        dropitem_action.canceled -= OnDropItemEnd;

        attack_action.performed -= OnAttack;
        attack_action.canceled -= OnEndAttack;
    }

    private void ControlsChanged(PlayerInput playerInput)
    {
        if (playerInput.currentControlScheme.Equals("GamePad"))
        {
            Debug.Log("I am a gamepad.");
        }
        else if (playerInput.currentControlScheme.Equals("Main"))
        {
            Debug.Log("I am a Keyboard and mouse.");
        }
    }

    private void OnDeviceHaBeenChanged(InputUser iu, InputUserChange iuc, InputDevice id)
    {
        if (id.device.IsActuated() && id.device.Equals(Gamepad.current))
        {
            Debug.Log("i am currently a gamepad.");
        }
        else
        {
            Debug.Log("i am currently a keyboard.");
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        MoveInput(ctx.action.ReadValue<Vector2>());
    }

    public void OnEndMove(InputAction.CallbackContext ctx)
    {
        MoveInput(new Vector2(0, 0));
    }



    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (cursorInputForLook)
        {
            LookInput(ctx.action.ReadValue<Vector2>());
        }
    }

    public void OnEndLook(InputAction.CallbackContext ctx)
    {
        //LookInput(new Vector2(0, 0));
    }


    public void OnJump(InputAction.CallbackContext ctx)
    {
        JumpInput(ctx.action.IsPressed());
    }


    public void OnEndJump(InputAction.CallbackContext ctx)
    {
        JumpInput(false);
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        SprintInput(ctx.action.IsPressed());
    }

    public void OnEndSprint(InputAction.CallbackContext ctx)
    {
        SprintInput(false);
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        CrouchInput(ctx.action.IsPressed());
    }


    public void OnEndCrouch(InputAction.CallbackContext ctx)
    {
        CrouchInput(false);
    }

    public void OnEndSwitchCameraPerspective(InputAction.CallbackContext ctx)
    {
        SwitchCameraPerspectiveInput(ctx.action.IsPressed());
    }

    public void OnSwitchCameraPerspective(InputAction.CallbackContext ctx)
    {
        SwitchCameraPerspectiveInput(false);
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        pause = !pause;
        PauseInput(pause);
    }

    public void OnInventory(InputAction.CallbackContext ctx)
    {
        inventory = !inventory;
        InventoryInput(inventory);
    }

    public void OnDropItem(InputAction.CallbackContext ctx)
    {
        DropItemInput(dropitem);
    }

    public void OnDropItemEnd(InputAction.CallbackContext ctx)
    {
        DropItemInput(false);
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        InteractInput(ctx.action.IsPressed());
    }

    public void OnEndInteract(InputAction.CallbackContext ctx)
    {
        InteractInput(false);
    }

    public void OnQuickSlotOne(InputAction.CallbackContext ctx)
    {
        QuickSlotOneInput(ctx.action.IsPressed());
    }

    public void OnEndQuickSlotOne(InputAction.CallbackContext ctx)
    {
        QuickSlotOneInput(false);
    }

    public void OnQuickSlotTwo(InputAction.CallbackContext ctx)
    {
        QuickSlotTwoInput(ctx.action.IsPressed());
    }

    public void OnEndQuickSlotTwo(InputAction.CallbackContext ctx)
    {
        QuickSlotTwoInput(false);
    }

    public void OnQuickSlotThree(InputAction.CallbackContext ctx)
    {
        QuickSlotThreeInput(ctx.action.IsPressed());
    }

    public void OnEndQuickSlotThree(InputAction.CallbackContext ctx)
    {
        QuickSlotThreeInput(false);
    }

    public void OnQuickSlotFour(InputAction.CallbackContext ctx)
    {
        QuickSlotFourInput(ctx.action.IsPressed());
    }

    public void OnEndQuickSlotFour(InputAction.CallbackContext ctx)
    {
        QuickSlotFourInput(false);
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        AttackInput(ctx.action.IsPressed());
    }

    public void OnEndAttack(InputAction.CallbackContext ctx)
    {
        AttackInput(false);
    }

    public void OnConsole(InputAction.CallbackContext ctx)
    {
        console = !console;
        ConsoleInput(console);
    }

    public void DropItemInput(bool newDropItemState)
    {
        dropitem = newDropItemState;
    }

    public void SwitchCameraPerspectiveInput(bool newCameraSwitchPerspective)
    {
        switchperspective = newCameraSwitchPerspective;
    }

    public void InteractInput(bool newInteractState)
    {
        interact = newInteractState;
    }

    public void QuickSlotOneInput(bool newQuickSlotOneState)
    {
        quickslot_1 = newQuickSlotOneState;
    }

    public void QuickSlotTwoInput(bool newQuickSlotTwoState)
    {
        quickslot_2 = newQuickSlotTwoState;
    }

    public void QuickSlotThreeInput(bool newQuickSlotThreeState)
    {
        quickslot_3 = newQuickSlotThreeState;
    }

    public void QuickSlotFourInput(bool newQuickSlotFourState)
    {
        quickslot_4 = newQuickSlotFourState;
    }

    public void AttackInput(bool newAttackState)
    {
        attack = newAttackState;
    }

    public void ConsoleInput(bool newConsoleState)
    {
        if (newConsoleState)
        {
//            _playerUIController.OpenConsole();
        }
        else
        {
//            _playerUIController.CloseConsole();
        }

        //_gameManager.SetCursorState(false);
    }

    public void MoveInput(Vector2 moveInput)
    {
        move = moveInput;
    }

    public void LookInput(Vector2 Lookinput)
    {
        look = Lookinput;
    }

    public void CrouchInput(bool newCrouchState)
    {
        crouch = newCrouchState;
    }

    public void InventoryInput(bool newInventoryState)
    {
        if (newInventoryState)
        {
//            _playerInvnentory.OpenInventory();
        }
        else
        {
            //  _playerInvnentory.CloseInventory();
        }

        //_gameManager.SetCursorState(false);
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void PauseInput(bool newPauseState)
    {
        if (newPauseState)
        {
//            _gameManager.PauseGame();
        }
        else
        {
            //  _gameManager.UnpauseGame();
        }

        //_gameManager.SetCursorState(newPauseState);
    }
}