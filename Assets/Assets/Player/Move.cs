using System;
using System.ComponentModel;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Move : MonoBehaviour
{
    [SerializeField] private InputActionAsset InputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Vector2 moveDir;

    private Rigidbody rb;
    
    [Header("Movement speeds")]
    [Min(0)] [SerializeField] float moveSpeed = 5;
    [SerializeField] float jumpSpeed = 5;

    [Header("Jump Trigger")]
    [Range(0.01f, 1)] [SerializeField] float boxHeight = 1;
    private BoxCollider bottomTrigger;
    [SerializeField] private int BottomCollisions = 0;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideFactor = 0.7f;
    [SerializeField] private LayerMask wallLayer = 1 << 9;

    [Header("Rotation")]
    [Range(0.1f, 10f)] [SerializeField] private float rotationSpeed = 10f;
    private float newAngle = 0f;

    void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    void Awake()
    {
        moveAction = InputActions.FindAction("Move");
        jumpAction = InputActions.FindAction("Jump");

        rb = GetComponent<Rigidbody>();

        rb.linearDamping = 2f;
        rb.angularDamping = 10f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        bottomTrigger = GetComponent<BoxCollider>();
        bottomTrigger.center = -(0.95f + boxHeight / 2) * Vector3.up;
        bottomTrigger.size = new Vector3(0.8f, boxHeight, 0.8f);
        bottomTrigger.isTrigger = true;
    }

    void Update()
    {
        moveDir = moveAction.ReadValue<Vector2>().normalized;

        if (jumpAction.WasPressedThisFrame() && BottomCollisions > 0 && rb.linearVelocity.y <= 0.1f)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        HandleWallSliding();
        Rotate();
    }

    private void HandleWallSliding()
    {
        Vector3 moveVelocity = new Vector3(moveDir.x, 0, moveDir.y).normalized * moveSpeed;
        
        if (WillHitWall(moveVelocity))
        {
            Vector3 slideDirection = Vector3.ProjectOnPlane(moveVelocity, Vector3.up);
            rb.linearVelocity = new Vector3(slideDirection.x, rb.linearVelocity.y, slideDirection.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
    }

    private bool WillHitWall(Vector3 direction)
    {
        return Physics.Raycast(transform.position, direction, 0.3f, wallLayer);
    }   

    private void Rotate()
    {
        if (moveDir.magnitude > 0.1f)
        {
            newAngle = Mathf.Atan2(moveDir.x, moveDir.y) * Mathf.Rad2Deg;
            newAngle = Mathf.LerpAngle(transform.eulerAngles.y, newAngle, rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, newAngle, 0);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider other)
    {
        BottomCollisions += 1;
    }

    void OnTriggerExit(Collider other)
    {
        BottomCollisions -= 1;
    }
}
