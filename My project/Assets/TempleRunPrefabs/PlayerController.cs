using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


namespace TempleRun
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private float initialPlayerSpeed = 4f;

        [SerializeField]
        private float maximumPlayerSpeed = 30f;

        [SerializeField]
        private float playerSpeedIncreaseRate = .1f;

        [SerializeField]
        private float jumpHeight = 1.0f;

        [SerializeField]
        private float initialGravityValue = -9.81f;

        [SerializeField]
        private LayerMask groundLayer;

        [SerializeField]
        private LayerMask turnLayer;

        private float playerSpeed;
        private float gravity;
        private Vector3 movementDirection = Vector3.forward;
        private Vector3 playerVelocity;

        private PlayerInput playerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;

        private CharacterController characterController;

        [SerializeField]
        private UnityEvent<Vector3> turnEvent;

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            characterController = GetComponent<CharacterController>();

            turnAction = playerInput.actions["Turn"];
            jumpAction = playerInput.actions["Jump"];
            slideAction = playerInput.actions["Slide"];

            turnAction.Enable();
            jumpAction.Enable();
            slideAction.Enable();
        }

        void Start()
        {
            playerSpeed = initialPlayerSpeed;
            gravity = initialGravityValue;
        }

        void Update()
        {
            characterController.Move(playerSpeed * Time.deltaTime * transform.forward);

            if (IsGrounded() && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }

            playerVelocity.y += gravity * Time.deltaTime;
            characterController.Move(playerVelocity * Time.deltaTime);
        }

        void OnEnable()
        {
            turnAction.performed += PlayerTurn;
            slideAction.performed += PlayerSlide;
            jumpAction.performed += PlayerJump;
        }

        void OnDisable()
        {
            turnAction.performed -= PlayerTurn;
            slideAction.performed -= PlayerSlide;
            jumpAction.performed -= PlayerJump;
        }

        private void PlayerJump(InputAction.CallbackContext context)
        {
            if (!IsGrounded()) { return; }

            playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
            characterController.Move(playerVelocity * Time.deltaTime);
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {
        }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();
            var turnPosition = CheckTurn(context.ReadValue<float>());
            if(!turnPosition.HasValue) { return; }

            var targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * movementDirection;
            turnEvent.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPosition);
        }

        private void Turn(float turnValue, Vector3? turnPosition)
        {
            if(!turnPosition.HasValue) { return; }
            var tempPlayerPosition = new Vector3(turnPosition.Value.x, transform.position.y, turnPosition.Value.z);
            characterController.enabled = false;
            transform.position = tempPlayerPosition;
            characterController.enabled = true;

            var targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            movementDirection = transform.forward.normalized;
        }

        private Vector3? CheckTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType tileType = tile.type;
                if ((tileType == TileType.LEFT && turnValue == -1) ||
                (tileType == TileType.RIGHT && turnValue == 1) ||
                tileType == TileType.SIDEWAYS)
                {
                    return tile.pivot.position;
                }
            }

            return null;
        }

        private bool IsGrounded(float length = .2f)
        {
            Vector3 raycastOriginFirst = transform.position;
            raycastOriginFirst.y -= characterController.height / 2f;
            raycastOriginFirst.y += .1f;

            Vector3 raycastOriginSecond = raycastOriginFirst;
            raycastOriginFirst -= transform.forward * .2f;
            raycastOriginSecond += transform.forward * .2f;

            return Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) ||
            Physics.Raycast(raycastOriginSecond, Vector3.down, out RaycastHit hit2, length, groundLayer);
        }
    }
}