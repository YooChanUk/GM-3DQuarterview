using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;//ĳ���� �ӵ� ����
    float hAxis;
    float vAxis;
    bool wDown;//�ȱ��ư ����
    bool jDown;//������ư ����

    bool isJump; //���� ������ �ϴ��������� �˻��ϴ� ����
    bool isDodge;

    Vector3 moveVec;
    Animator anim;//�ڽİ��迡�� Ȱ���� ������Ʈ ���ϵ� Ȱ��
    Rigidbody rigid;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        transform.position += moveVec * Speed * (wDown ? 0.3f : 1) * Time.deltaTime;


        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    { 
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);//���޽��� ������� ��
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            Speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut",0.5f);
        }
    }

    void DodgeOut()
    {
        Speed *= 0.5f;
        isDodge = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }
}
