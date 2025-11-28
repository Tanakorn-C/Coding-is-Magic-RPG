using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Spawning")] // (เพิ่ม Header ใหม่)
        [SerializeField] private Transform defaultSpawnPoint; // (เพิ่มตัวแปรนี้)

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runSpeed = 8f; 
        private bool isRunning = false;
        private Vector2 movementDirection;
        private Vector2 currentInput; 

        [Header("Animations")]
        [SerializeField] private Animator anim;
        private string lastDirection = "Down";
        private string currentAnimationState;
        
        private Rigidbody2D rb;
        private InputAction runAction;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // --- (START) แก้ไขโลจิกการเกิด (Spawning Logic) ---
            // (นี่คือส่วนที่แก้ไข)
            GameDataPersistenceMain data = GameDataPersistenceMain.Instance;
            
            // 1. เช็คว่าเพิ่งกลับมาจากฉากต่อสู้หรือไม่ (เช็คจาก "sceneToReturnTo")
            if (data != null && !string.IsNullOrEmpty(data.sceneToReturnTo))
            {
                // ถ้าใช่ (กลับจากต่อสู้) ให้ใช้โลจิกเดิมของคุณ
                if (data.justWonBattle)
                {
                    transform.position = data.winSpawnPosition;
                }
                else
                {
                    transform.position = data.loseSpawnPosition;
                }
                
                // (สำคัญ) เคลียร์ค่าสถานะ เพื่อให้โหลดฉากครั้งต่อไปเป็นปกติ
                data.sceneToReturnTo = null;
                data.justWonBattle = false;
            }
            else
            {
                // 2. ถ้าไม่ใช่ (คือการโหลดฉากมาตามปกติ)
                // ให้ใช้โลจิกของ PlayerSpawner (คือไปจุดเกิดเริ่มต้น)
                if (defaultSpawnPoint != null)
                {
                    transform.position = defaultSpawnPoint.position;
                    transform.rotation = defaultSpawnPoint.rotation;
                }
                else
                {
                    Debug.LogWarning("PlayerMovement: ไม่ได้ตั้งค่า Default Spawn Point!");
                }
            }
            // --- (END) แก้ไขโลจิกการเกิด ---


            // --- (START) โค้ด Input ของคุณ (ไม่ต้องแก้ไข) ---
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                runAction = playerInput.actions.FindAction("Player/Run");
                if (runAction == null)
                {
                    Debug.LogError("FATAL ERROR: Could not find Input Action 'Player/Run'.");
                }
            }
            else
            {
                Debug.LogError("FATAL ERROR: PlayerInput component not found on Player.");
            }
            // --- (END) โค้ด Input ของคุณ ---
        }

        // 
        // ... (โค้ดที่เหลือทั้งหมดของคุณ: OnEnable, OnDisable, OnRunPerformed, Update, FixedUpdate, ฯลฯ) ...
        // ... (ไม่ต้องแก้ไขอะไรในส่วนที่เหลือของไฟล์นี้ครับ) ...
        // 

        // --- (START) NEW ROBUST RUN LOGIC ---
        private void OnEnable()
        {
            if (runAction != null)
            {
                runAction.Enable(); 
                runAction.performed += OnRunPerformed;
                runAction.canceled += OnRunCanceled;
            }
        }

        private void OnDisable()
        {
            if (runAction != null)
            {
                runAction.performed -= OnRunPerformed;
                runAction.canceled -= OnRunCanceled;
                runAction.Disable();
            }
        }

        private void OnRunPerformed(InputAction.CallbackContext context)
        {
            isRunning = true;
        }

        private void OnRunCanceled(InputAction.CallbackContext context)
        {
            isRunning = false;
        }
        // --- (END) NEW ROBUST RUN LOGIC ---

        private void Update()
        {
            UpdateAnimationDirection(); 
            HandleAnimations();     
        }

        private void FixedUpdate()
        {
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            rb.linearVelocity = movementDirection * currentSpeed;
        }
        
        private void OnRun(InputValue value)
        {
            // (เราไม่ได้ใช้ Logic นี้แล้ว)
        }
        
        private void HandleAnimations()
        {
            if (anim == null) return;

            string animationName = "";

            if (movementDirection == Vector2.zero)
                animationName = "Idle";
            else if (isRunning)
                animationName = "Running";
            else
                animationName = "Walking";

            string newAnimationState = animationName + lastDirection;
            if (currentAnimationState == newAnimationState)
            {
                return; 
            }

            anim.Play(newAnimationState);
            currentAnimationState = newAnimationState;
        }
        
        private void UpdateAnimationDirection()
        {
            if (currentInput == Vector2.zero)
            {
                return; // Keep the last direction when idle
            }

            const float threshold = 0.3f; 

            bool right = currentInput.x > threshold;
            bool left = currentInput.x < -threshold;
            bool up = currentInput.y > threshold;
            bool down = currentInput.y < -threshold;

            if (up && right)
            {
                lastDirection = "DiagUpRight";
            }
            else if (up && left)
            {
                lastDirection = "DiagUpLeft";
            }
            else if (down && right)
            {
                lastDirection = "DiagDownRight";
            }
            else if (down && left)
            {
                lastDirection = "DiagDownLeft";
            }
            else if (up)
            {
                lastDirection = "Up";
            }
            else if (down)
            {
                lastDirection = "Down";
            }
            else if (right)
            {
                lastDirection = "Right";
            }
            else if (left)
            {
                lastDirection = "Left";
            }
        }

        #region Input
        private void OnMove(InputValue value)
        {
            currentInput = value.Get<Vector2>().normalized;
            movementDirection = currentInput;
        }
        #endregion
    }
}