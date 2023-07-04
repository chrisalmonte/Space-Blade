using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private Rigidbody2D character;
    [SerializeField] private PlayerInput input;

    [Header("Movement Parameters")]
    [SerializeField] private float movForce = 70f;

    private InputAction shoot;
    private InputAction bomb;
    private InputAction bladeMode;
    private Vector2 movementAxis;
    private Vector2 atkDirection;

    public void OnMove(InputValue value) => movementAxis = value.Get<Vector2>();
    public void OnSetAttackDirection(InputValue value) => atkDirection = value.Get<Vector2>();

    private void OnEnable()
    {
        shoot.performed += ctx => Shoot();
        bomb.performed += ctx => Bomb();
    }

    private void OnDisable()
    {
        shoot.performed -= ctx => Shoot();
        bomb.performed -= ctx => Bomb();
    }

    private void Awake()
    {
        shoot = input.actions["Shoot"];
        bomb = input.actions["Bomb"];
        bladeMode = input.actions["Blade"];
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        character.AddForce(movementAxis * movForce);
    }

    private void Shoot() { }
    private void Bomb() { }
}
