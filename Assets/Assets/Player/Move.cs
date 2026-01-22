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
    [SerializeField] private float wallSlideFactor = 0.7f; // Скольжение 70%
    [SerializeField] private LayerMask wallLayer = 1 << 9;  // Wall layer (9)

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

        // НАСТРОЙКИ физики для скольжения
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
        Moving();
        Rotate();
    }

    private void HandleWallSliding()
    {
        // ПЛАВНОЕ СКОЛЬЖЕНИЕ ПО СТЕНАМ
        Vector3 moveVelocity = new Vector3(moveDir.x, 0, moveDir.y).normalized * moveSpeed;
        
        // Проверяем столкновение со стеной впереди
        if (WillHitWall(moveVelocity))
        {
            // Проецируем движение параллельно стене
            Vector3 slideDirection = Vector3.ProjectOnPlane(moveVelocity, Vector3.up);
            rb.linearVelocity = new Vector3(slideDirection.x, rb.linearVelocity.y, slideDirection.z);
        }
        else
        {
            // Обычное движение
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
    }

    private bool WillHitWall(Vector3 direction)
    {
        // ТОЧНАЯ проверка передвижения на 0.3м вперёд
        return Physics.Raycast(transform.position, direction, 0.3f, wallLayer);
    }

    private void Moving()
    {
        // Физика сама обрабатывает Y (гравитация + прыжки)
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position - ((0.95f + boxHeight / 2) * Vector3.up), new Vector3(0.8f, boxHeight, 0.8f));

        // DEBUG лучи для стен
        Gizmos.color = WillHitWall(transform.forward) ? Color.red : Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 0.3f);
    }
}
