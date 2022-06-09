using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    public float walkMoveSpd = 2.0f;
    public float runMoveSpd = 3.5f;
    public float rotateMoveSpd = 100.0f;
    public float rotateBodySpd = 2.0f;
    public float moveChageSpd = 0.1f;
    private Vector3 vecNowVelocity = Vector3.zero;
    private Vector3 vecMoveDirection = Vector3.zero;
    private CharacterController controllerCharacter = null;
    private CollisionFlags collisionFlagsCharacter = CollisionFlags.None;
    private float gravity = 9.8f;
    private float verticalSpd = 5f;
    private bool stopMove = false;
    private Rigidbody myRigid;

    [Header("애니메이션 속성")]
    public AnimationClip animationClipIdle = null;
    public AnimationClip animationClipWalk = null;
    public AnimationClip animationClipRun = null;
    public AnimationClip animationClipAtkStep_1 = null;
    public AnimationClip animationClipAtkStep_2 = null;
    public AnimationClip animationClipAtkStep_3 = null;
    public AnimationClip animationClipAtkStep_4 = null;

    private Animation animationPlayer = null;

    public enum PlayerState { None, Idle, Walk, Run, Attack, Skill }

    [Header("캐릭터상태")]
    public PlayerState playerState = PlayerState.None;

    public enum PlayerAttackState { atkStep_1, atkStep_2, atkStep_3, atkStep_4 }

    public PlayerAttackState playerAttackState = PlayerAttackState.atkStep_1;

    public bool flagNextAttack = false;

    void Start()
    {
        controllerCharacter = GetComponent<CharacterController>();
        animationPlayer = GetComponent<Animation>();
        animationPlayer.playAutomatically = false;
        animationPlayer.Stop();

        playerState = PlayerState.Idle;

        animationPlayer[animationClipIdle.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipWalk.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipWalk.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipAtkStep_1.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_2.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_3.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_4.name].wrapMode = WrapMode.Once;

        SetAnimationEvent(animationClipAtkStep_1, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_2, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_3, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_4, "OnPlayerAttackFinshed");
    }

    void Update()
    {
        Move();
        vecDirectionChangeBody();
        AnimationClipCtrl();
        ckAnimationState();
        InputAttackCtrll();
        setGravity();
        AtkComponentCtrl();
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

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 targetDirection = horizontal * right + vertical * forward;

        vecMoveDirection = Vector3.RotateTowards(vecMoveDirection, targetDirection, rotateMoveSpd * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        vecMoveDirection = vecMoveDirection.normalized;
        float spd = walkMoveSpd;

        if (playerState == PlayerState.Run)
        {
            spd = runMoveSpd;
        }
        else if (playerState == PlayerState.Walk)
        {
            spd = walkMoveSpd;
        }
        Vector3 vecGravity = new Vector3(0f, verticalSpd, 0f);


        Vector3 moveAmount = (vecMoveDirection * spd * Time.deltaTime) + vecGravity;
        collisionFlagsCharacter = controllerCharacter.Move(moveAmount);
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

    void vecDirectionChangeBody()
    {
        if (getNowVelocityVal() > 0.0f)
        {
            Vector3 newForward = controllerCharacter.velocity;
            newForward.y = 0.0f;
            transform.forward = Vector3.Lerp(transform.forward, newForward, rotateBodySpd * Time.deltaTime);

        }
    }


    void playAnimationByClip(AnimationClip clip)
    {
        animationPlayer.GetClip(clip.name);
        animationPlayer.CrossFade(clip.name);
    }

    void AnimationClipCtrl()
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                playAnimationByClip(animationClipIdle);
                break;
            case PlayerState.Walk:
                playAnimationByClip(animationClipWalk);
                break;
            case PlayerState.Run:
                playAnimationByClip(animationClipRun);
                break;
            case PlayerState.Attack:
                stopMove = true;
                AtkAnimationCrtl();
                break;
        }
    }

    void ckAnimationState()
    {
        float nowSpd = getNowVelocityVal();

        switch (playerState)
        {
            case PlayerState.Idle:
                if (nowSpd > 0.0f)
                {
                    playerState = PlayerState.Walk;
                }
                break;
            case PlayerState.Walk:
                if (nowSpd > 2.0f)
                {
                    playerState = PlayerState.Run;
                }
                else if (nowSpd < 0.01f)
                {
                    playerState = PlayerState.Idle;
                }
                break;
            case PlayerState.Run:
                if (nowSpd < 0.5f)
                {
                    playerState = PlayerState.Walk;
                }

                if (nowSpd < 0.01f)
                {
                    playerState = PlayerState.Idle;
                }
                break;
            case PlayerState.Attack:
                break;
            case PlayerState.Skill:
                break;
        }
    }

    void InputAttackCtrll()
    {
        if (Input.GetMouseButton(0) == true)
        {
            Debug.Log("InputAttackCtrll : " + playerState);
            if (playerState != PlayerState.Attack)
            {
                playerState = PlayerState.Attack;
                playerAttackState = PlayerAttackState.atkStep_1;
            }
            else
            {
                switch (playerAttackState)
                {
                    case PlayerAttackState.atkStep_1:
                        if (animationPlayer[animationClipAtkStep_1.name].normalizedTime > 0.01f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    case PlayerAttackState.atkStep_2:
                        if (animationPlayer[animationClipAtkStep_2.name].normalizedTime > 0.05f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    case PlayerAttackState.atkStep_3:
                        if (animationPlayer[animationClipAtkStep_3.name].normalizedTime > 0.5f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    case PlayerAttackState.atkStep_4:
                        if (animationPlayer[animationClipAtkStep_4.name].normalizedTime > 0.5f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        if (Input.GetMouseButtonDown(1) == true)
        {
            if (playerState == PlayerState.Attack)
            {
                playerAttackState = PlayerAttackState.atkStep_1;
                flagNextAttack = false;
            }
            playerState = PlayerState.Skill;
        }
    }

    void OnPlayerAttackFinshed()
    {
        if (flagNextAttack == true)
        {
            flagNextAttack = false;
            Debug.Log(playerAttackState);
            switch (playerAttackState)
            {
                case PlayerAttackState.atkStep_1:
                    playerAttackState = PlayerAttackState.atkStep_2;
                    Debug.Log(playerAttackState);
                    break;
                case PlayerAttackState.atkStep_2:
                    playerAttackState = PlayerAttackState.atkStep_3;
                    break;
                case PlayerAttackState.atkStep_3:
                    playerAttackState = PlayerAttackState.atkStep_4;
                    break;
                case PlayerAttackState.atkStep_4:
                    playerAttackState = PlayerAttackState.atkStep_1;
                    break;
            }
        }
        else
        {

            stopMove = false;

            playerState = PlayerState.Idle;

            playerAttackState = PlayerAttackState.atkStep_1;
        }
    }

    void SetAnimationEvent(AnimationClip animationclip, string funcName)
    {
        AnimationEvent newAnimationEvent = new AnimationEvent();
        newAnimationEvent.functionName = funcName;
        newAnimationEvent.time = animationclip.length - 0.15f;
        animationclip.AddEvent(newAnimationEvent);
    }

    void AtkAnimationCrtl()
    {
        //만약 공격상태가?
        switch (playerAttackState)
        {
            case PlayerAttackState.atkStep_1:
                playAnimationByClip(animationClipAtkStep_1);
                break;
            case PlayerAttackState.atkStep_2:
                playAnimationByClip(animationClipAtkStep_2);
                break;
            case PlayerAttackState.atkStep_3:
                playAnimationByClip(animationClipAtkStep_3);
                break;
            case PlayerAttackState.atkStep_4:
                playAnimationByClip(animationClipAtkStep_4);
                break;
        }
    }

    void setGravity()
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

    void AtkComponentCtrl()
    {
        switch (playerState)
        {
            case PlayerState.Attack:
            case PlayerState.Skill:
                break;
            default:
                break;
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