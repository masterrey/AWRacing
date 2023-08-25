using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Animation
{
    public class AnythingMovementController : MonoBehaviour
    {
        Rigidbody rb;
        Animator animator;
        RunWalkIdleController controller;

        Vector3 movement;
        Vector3 euler;
        Quaternion root = Quaternion.identity;

        //input
        public float horizontalInput = 0;
        public float verticalInput = 0;

        [Header("Speed")]
        public float maxSpeed = 3;
        public float turnSpeed = 2;
        public float jumpHeight;

        bool hasHInput;
        bool hasVInput;
        bool isWalking;
        bool isGround;
        bool doubleJump;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
            controller = GetComponentInChildren<RunWalkIdleController>();

            isGround = true;
        }

        void Update()
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            SetInput();

            Move();

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }

            SetAnim();
        }

        void SetInput()
        {
            movement = transform.forward * verticalInput;
            movement.Normalize();

            euler.Set(0, horizontalInput, 0);
            euler.Normalize();

            hasHInput = !Mathf.Approximately(horizontalInput, 0f);
            hasVInput = !Mathf.Approximately(verticalInput, 0f);

            isWalking = hasHInput || hasVInput;
        }

        void Turn()
        {
            root = Quaternion.Euler((euler * turnSpeed) * Time.deltaTime);
            //rb.MoveRotation(rb.rotation * root);
            rb.rotation *= root;
        }

        void Move()
        {
            if (isWalking)
            {
                movement = movement.normalized * maxSpeed * Time.deltaTime;

                //rb.MovePosition(rb.position + movement);
                rb.velocity = movement;
                Turn();
            }
        }

        void Jump()
        {
            if (isGround == true)
            {
                rb.AddForce(movement + (transform.up * jumpHeight), ForceMode.Impulse);
            }
            else if (doubleJump == true)
            {
                doubleJump = false;
                rb.AddForce(movement + (transform.up * jumpHeight), ForceMode.Impulse);
            }
        }

        void SetAnim()
        {
            /*animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsGround", isGround);*/
            
            controller.BlendAnimationOnSpeed(rb.velocity.magnitude, 0.1f, 0.8f);
        }

        void OnCollisionStay(Collision other)
        {
            if (other.collider.CompareTag("ground"))
            {
                isGround = true;
                doubleJump = true;
            }
        }

        void OnCollisionExit(Collision other)
        {
            if (other.collider.CompareTag("ground"))
            {
                isGround = false;
            }
        }
    }
}