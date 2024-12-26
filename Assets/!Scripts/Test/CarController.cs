using System.Collections;
using System.Collections.Generic;
using DilmerGames.Core;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField]
    private float force = 1.0f;

    [SerializeField]
    private float torque = 1.0f;

    [SerializeField]
    private float minSpeedBeforeTorque = 0.3f;


    public Direction CurrentDirection { get; set; }

    public bool IsAutonomous { get; set; } = false;

    private Rigidbody carRigidBody;

    public enum Direction
    {
        MoveForward,
        MoveBackward,
        TurnLeft,
        TurnRight,
        None
    }

    void Awake() 
    {
        carRigidBody = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate() => ApplyMovement();

    public void ApplyMovement()
    {
        if(Input.GetKey(KeyCode.UpArrow) || (CurrentDirection == Direction.MoveForward && IsAutonomous))
        {
           //Debug.Log("Up");
            carRigidBody.AddForce(transform.forward * force, ForceMode.Impulse);
        }

        if(Input.GetKey(KeyCode.DownArrow) || (CurrentDirection == Direction.MoveBackward && IsAutonomous))
        {
            //Debug.Log("Down");

            carRigidBody.AddForce(-transform.forward * force, ForceMode.Impulse);
        }

        if((Input.GetKey(KeyCode.LeftArrow) && canApplyTorque()) || (CurrentDirection == Direction.TurnLeft && IsAutonomous) && canApplyTorque())
        {
            //Debug.Log("Left");

            carRigidBody.AddTorque(transform.up * -torque, ForceMode.Impulse);
        }

        if(Input.GetKey(KeyCode.RightArrow) && canApplyTorque() || (CurrentDirection == Direction.TurnRight && IsAutonomous) && canApplyTorque())
        {
            //Debug.Log("Right");

            carRigidBody.AddTorque(transform.up * torque, ForceMode.Impulse);
        }

        //Debug.Log(carRigidBody.linearVelocity.magnitude);
    }

    public bool canApplyTorque()
    {
        var velocity = carRigidBody.linearVelocity.magnitude;
        return Mathf.Abs(velocity) >= minSpeedBeforeTorque;
    }
}
