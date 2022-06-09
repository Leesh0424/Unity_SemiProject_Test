using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class EnemyCtrl : MonoBehaviour
{
    public enum SkullState { None, Idle, Move, Wait, GoTarget, Atk, Damage, Die }

    public SkullState skullState = SkullState.None;
    public float spdMove = 1f;
    public GameObject targetCharactor = null;
    public Transform targetTransform = null;
    public Vector3 posTarget = Vector3.zero;

    private Animation skullAnimation = null;
    private Transform skullTransform = null;

    [Header("애니메이션 클립")]
    public AnimationClip IdleAnimClip = null;
    public AnimationClip MoveAnimClip = null;
    public AnimationClip AtkAnimClip = null;
    public AnimationClip DamageAnimClip = null;
    public AnimationClip DieAnimClip = null;

    [Header("전투속성")]
    public int hp = 100;
    public float AtkRange = 1.5f;
    public GameObject effectDamage = null;
    public GameObject effectDie = null;

    private Tweener effectTweener = null;
    private SkinnedMeshRenderer skinnedMeshRenderer = null;


    void OnAtkAnmationFinished()
    {
        Debug.Log("Atk Animation finished");
    }

    void OnDmgAnmationFinished()
    {
        Debug.Log("Dmg Animation finished");
    }

    void OnDieAnmationFinished()
    {
        Debug.Log("Die Animation finished");
        Instantiate(effectDie, skullTransform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void OnAnimationEvent(AnimationClip clip, string funcName)
    {
        AnimationEvent retEvent = new AnimationEvent();
        retEvent.functionName = funcName;
        retEvent.time = clip.length - 0.1f;
        clip.AddEvent(retEvent);
    }


    void Start()
    {
        skullState = SkullState.Idle;

        skullAnimation = GetComponent<Animation>();
        skullTransform = GetComponent<Transform>();

        skullAnimation[IdleAnimClip.name].wrapMode = WrapMode.Loop;
        skullAnimation[MoveAnimClip.name].wrapMode = WrapMode.Loop;
        skullAnimation[AtkAnimClip.name].wrapMode = WrapMode.Once;
        skullAnimation[DamageAnimClip.name].wrapMode = WrapMode.Once;

        skullAnimation[DamageAnimClip.name].layer = 10;
        skullAnimation[DieAnimClip.name].wrapMode = WrapMode.Once;
        skullAnimation[DieAnimClip.name].layer = 10;

        OnAnimationEvent(AtkAnimClip, "OnAtkAnmationFinished");
        OnAnimationEvent(DamageAnimClip, "OnDmgAnmationFinished");
        OnAnimationEvent(DieAnimClip, "OnDieAnmationFinished");

        skinnedMeshRenderer = skullTransform.Find("UD_light_infantry").GetComponent<SkinnedMeshRenderer>();
    }

    void CkState()
    {
        switch (skullState)
        {
            case SkullState.Idle:
                setIdle();
                break;
            case SkullState.GoTarget:
            case SkullState.Move:
                setMove();
                break;
            case SkullState.Atk:
                StartCoroutine("setAtk");
                break;
            default:
                break;
        }
    }

    void Update()
    {
        CkState();
        AnimationCtrl();
    }

    void setIdle()
    {
        if (targetCharactor == null)
        {
            posTarget = new Vector3(skullTransform.position.x + Random.Range(-10f, 10f),
                                    skullTransform.position.y + 1000f,
                                    skullTransform.position.z + Random.Range(-10f, 10f)
                );
            Ray ray = new Ray(posTarget, Vector3.down);
            RaycastHit infoRayCast = new RaycastHit();
            if (Physics.Raycast(ray, out infoRayCast, Mathf.Infinity) == true)
            {
                posTarget.y = infoRayCast.point.y;
            }
            skullState = SkullState.Move;
        }
        else
        {
            skullState = SkullState.GoTarget;
        }
    }

    void setMove()
    {
        Vector3 distance = Vector3.zero;
        Vector3 posLookAt = Vector3.zero;

        switch (skullState)
        {
            case SkullState.Move:
                if (posTarget != Vector3.zero)
                {
                    distance = posTarget - skullTransform.position;

                    if (distance.magnitude < AtkRange)
                    {
                        StartCoroutine(setWait());
                        return;
                    }

                    posLookAt = new Vector3(posTarget.x, skullTransform.position.y, posTarget.z);
                }
                break;
            case SkullState.GoTarget:
                if (targetCharactor != null)
                {
                    distance = targetCharactor.transform.position - skullTransform.position;
                    if (distance.magnitude < AtkRange)
                    {
                        skullState = SkullState.Atk;
                        return;
                    }
                    posLookAt = new Vector3(targetCharactor.transform.position.x, skullTransform.position.y, targetCharactor.transform.position.z);
                }
                break;
            default:
                break;

        }

        Vector3 direction = distance.normalized;
        direction = new Vector3(direction.x, 0f, direction.z);
        Vector3 amount = direction * spdMove * Time.deltaTime;

        skullTransform.Translate(amount, Space.World);
        skullTransform.LookAt(posLookAt);

    }
    IEnumerator setWait()
    {
        skullState = SkullState.Wait;
        float timeWait = Random.Range(1f, 3f);
        yield return new WaitForSeconds(timeWait);
        skullState = SkullState.Idle;
    }

    void AnimationCtrl()
    {
        switch (skullState)
        {
            case SkullState.Wait:
            case SkullState.Idle:
                skullAnimation.CrossFade(IdleAnimClip.name);
                break;
            case SkullState.Move:
            case SkullState.GoTarget:
                skullAnimation.CrossFade(MoveAnimClip.name);
                break;
            case SkullState.Atk:
                skullAnimation.CrossFade(AtkAnimClip.name);
                break;
            case SkullState.Die:
                skullAnimation.CrossFade(DieAnimClip.name);
                break;
            default:
                break;
        }
    }

    void OnCkTarget(GameObject target)
    {
        targetCharactor = target;
        targetTransform = targetCharactor.transform;
        skullState = SkullState.GoTarget;

    }

    IEnumerator setAtk()
    {
        float distance = Vector3.Distance(targetTransform.position, skullTransform.position);
        if (distance > AtkRange + 0.5f)
        {
            skullState = SkullState.GoTarget;
        }
        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerAtk") == true)
        {
            hp -= 10;
            if (hp > 0)
            {
                Instantiate(effectDamage, other.transform.position, Quaternion.identity);
                skullAnimation.CrossFade(DamageAnimClip.name);
                effectDamageTween();
            }
            else
            {
                skullState = SkullState.Die;
            }
        }
    }

    void effectDamageTween()
    {
        if (effectTweener != null && effectTweener.isComplete == false)
        {
            return;
        }

        Color colorTo = Color.red;

        effectTweener = HOTween.To(skinnedMeshRenderer, 0.2f, new TweenParms()
                                .Prop("color", colorTo)
                                .Loops(1, LoopType.Yoyo)
                                .OnStepComplete(OnDamageTweenFinished)
            );
    }

    void OnDamageTweenFinished()
    {
        skinnedMeshRenderer.material.color = Color.white;
    }


}
