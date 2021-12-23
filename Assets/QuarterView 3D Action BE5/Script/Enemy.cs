using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C };
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public bool isChase;
    public BoxCollider meleeArea;
    public bool isAttack;
    public GameObject bullet;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;//윈도우 --> AI에서 네비게이션 베이크할것(월드 또는 지형지물 스태틱 상태일것)
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart",2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk",true);
    }

    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);//플레이어를 따라가게 만드는 컴포넌트 사용
            nav.isStopped = !isChase;//쫓고있다면 멈추지않고 안쫓는다면 멈춘다.
        }
        
    }



    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;//충돌시 이동로직 방해 못하게
            rigid.angularVelocity = Vector3.zero;//충돌시 회전로직 방해 못하게
        }
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();//후처리
    }

    void Targeting()
    {
        float targetRadius = 0;
        float targetRange = 0;

        switch (enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;

            case Type.B:
                targetRadius = 1f;
                targetRange = 12f;
                break;

            case Type.C:
                targetRadius = 0.5f;
                targetRange = 25f;
                break;
        }


        RaycastHit[] rayHits =
            Physics.SphereCastAll(transform.position, targetRadius, transform.forward,
                                    targetRange, LayerMask.GetMask("Player"));

        if (rayHits.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }    


        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);


    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec,false));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec,false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec,true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            mat.color = Color.white;
        }
        else
        {
            
            mat.color = Color.grey;
            gameObject.layer = 12;
            isChase = false;
            nav.enabled = false;//네비가 켜진동안 Y축 상승을 하지 않음

            anim.SetTrigger("doDie");
            

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddTorque(reactVec *15 ,ForceMode.Impulse);
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }


            Destroy(gameObject,4);
        }
    }
}
