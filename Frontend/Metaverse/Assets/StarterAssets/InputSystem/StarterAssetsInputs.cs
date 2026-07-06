using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        // Reference to the Spawn Transform (child of Cube)

        // Method that checks if input is enabled globally
        private bool IsInputEnabled()
        {
            return GameManager.isInputEnabled; // Check the global flag
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            if (IsInputEnabled()) MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (IsInputEnabled() && cursorInputForLook) LookInput(value.Get<Vector2>());
        }

        public void OnJump(InputValue value)
        {
            if (IsInputEnabled()) JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            if (IsInputEnabled()) SprintInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        private void Update()
        {
            // Check if the player's y position is below 0 (or a threshold you prefer)
            if (transform.position.y <= -2f || NetworkTerrainManager.toSpawn)
            {
                // Call the teleport function
                TeleportToSpawn();
                NetworkTerrainManager.toSpawn = false; // Reset the flag after teleporting
            }
        }

        public void TeleportToSpawn()
        {

            Vector3 spawnPosition = GlobalUserData.currentSpawnPoint;
            if (spawnPosition != null)
            {
                // Teleport the player to the Spawn position
                transform.position = spawnPosition;

                // Optionally reset other states (like grounded, jump, etc.)
                jump = false;
                sprint = false;
            }
            else
            {
                Debug.LogWarning("Spawn Transform not set.");
            }
        }
    }
}
