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
    [SerializeField] private WeaponSystem weapons;

    [Header("Movement Parameters")]
    [SerializeField] private float movForce = 70f;
    
    private InputAction bladeMode;
    private Vector2 movementAxis;

    public void OnMove(InputValue value) => movementAxis = value.Get<Vector2>();    

    private void Awake()
    {        
        bladeMode = input.actions["Blade"];
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        character.AddRelativeForce(movementAxis * movForce);
    }
}
