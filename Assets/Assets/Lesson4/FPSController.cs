using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Multiple Jumps")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float coyoteTime = 0.2f;
    
    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    
    private int jumpsRemaining;
    private float lastGroundedTime;
    private float verticalRotation;
    
    private Transform cameraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        cameraTransform.SetParent(transform);
        cameraTransform.localPosition = new Vector3(0, 0.5f, 0);
        cameraTransform.localRotation = Quaternion.identity;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        HandleMouseLook();
        ApplyGravity();
    }

    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            jumpsRemaining = maxJumps;
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        
        if (direction.magnitude >= 0.1f)
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 moveDir = transform.right * direction.x + transform.forward * direction.z;
            controller.Move(moveDir * speed * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        bool jumpInput = Input.GetButtonDown("Jump");
        bool canDoubleJump = jumpsRemaining > 0;
        
        if (jumpInput && canDoubleJump)
        {
            velocity.y = jumpForce;
            jumpsRemaining--;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !hasFocus;
    }
}
