using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    public float walkMoveSpd = 2.0f;
    public float runMoveSpd = 3.5f;
    public float rotateMoveSpd = 100.0f;
    public float rotateBodySpd = 100.0f;
    public float moveChageSpd = 0.1f;
    [SerializeField] private Animator animator;
    [SerializeField] private float senservity = 100;
    private Vector3 vecNowVelocity = Vector3.zero;
    private Vector3 vecMoveDirection = Vector3.zero;
    private CharacterController controllerCharacter = null;
    private CollisionFlags collisionFlagsCharacter = CollisionFlags.None;
    private float gravity = 9.8f;
    private float verticalSpd = 5f;
    private bool stopMove = false;
    private Rigidbody myRigid;

    #region Animator Hash
    readonly int hashMove = Animator.StringToHash("IsMove");
    readonly int hashAttack = Animator.StringToHash("Attack");
    readonly int hashHorizontal = Animator.StringToHash("Horiznotal");
    readonly int hashVertical = Animator.StringToHash("Vertical");
    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        myRigid = GetComponent<Rigidbody>();
        controllerCharacter = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
        //VecDirectionChangeBody();
        Rotate();
        SetGravity();
        CheckInputKey();
    }

    private void Rotate()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        transform.rotation = transform.rotation * Quaternion.Euler(0, mouseX * senservity * Time.deltaTime, 0);
    }

    private void CheckInputKey()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger(hashAttack);
        }
    }

    void Move()
    {
        if (stopMove == true)
        {
            return;
        }

        Transform CameraTransform = Camera.main.transform;
        Vector3 forward = CameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;

        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        Vector3 targetDirection = horizontal * right + vertical * forward;

        vecMoveDirection = Vector3.RotateTowards(vecMoveDirection, targetDirection, rotateMoveSpd * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        vecMoveDirection = vecMoveDirection.normalized;
        float spd = walkMoveSpd;
        
        Vector3 vecGravity = new Vector3(0f, verticalSpd, 0f);

        Vector3 moveAmount = (vecMoveDirection * spd * Time.deltaTime) + vecGravity;
        collisionFlagsCharacter = controllerCharacter.Move(moveAmount);
        if(horizontal !=0 || vertical != 0)
        {
            animator.SetFloat(hashVertical, vertical);
            animator.SetFloat(hashHorizontal, horizontal);
            animator.SetBool(hashMove, true);
        }
        else
        {
            animator.SetBool(hashMove, false);
        }
    }

    float getNowVelocityVal()
    {
        if (controllerCharacter.velocity == Vector3.zero)
        {
            vecNowVelocity = Vector3.zero;
        }
        else
        {
            Vector3 retVelocity = controllerCharacter.velocity;
            retVelocity.y = 0.0f;
            vecNowVelocity = Vector3.Lerp(vecNowVelocity, retVelocity, moveChageSpd * Time.fixedDeltaTime);
        }
        return vecNowVelocity.magnitude;
    }

    private void OnGUI()
    {
        if (controllerCharacter != null && controllerCharacter.velocity != Vector3.zero)
        {
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 50;
            labelStyle.normal.textColor = Color.white;
            float _getVelocitySpd = getNowVelocityVal();
            GUILayout.Label("현재속도 : " + _getVelocitySpd.ToString(), labelStyle);

            GUILayout.Label("현재벡터 : " + controllerCharacter.velocity.ToString(), labelStyle);

            GUILayout.Label("현재백터 크기 속도 : " + vecNowVelocity.magnitude.ToString(), labelStyle);

        }
    }

    void VecDirectionChangeBody()
    {
        if (getNowVelocityVal() > 0.0f)
        {
            Vector3 newForward = controllerCharacter.velocity;
            newForward.y = 0.0f;
            transform.forward = Vector3.Lerp(transform.forward, newForward, rotateBodySpd * Time.deltaTime);
        }
    }

    void SetGravity()
    {
        if ((collisionFlagsCharacter & CollisionFlags.CollidedBelow) != 0)
        {
            verticalSpd = 0f;
        }
        else
        {
            verticalSpd -= gravity * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("damaged");
        }
    }
}