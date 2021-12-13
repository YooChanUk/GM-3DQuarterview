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

    bool isJump; //���� ������ �ϴ����� ���� �˻��ϴ� ����
    bool isDodge;//���� ȸ�Ǹ� �ϴ����� ���� �˻��ϴ� ����

    Vector3 moveVec;
    Vector3 dodgeVec;

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

    void GetInput()//�� ���������� �������ֱ⿡ ���⼭ �ٲ�� �ٸ��͵鵵 ����
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // ��else�� ���� ����� �������� �ʳ� ������... normalized�� �밢������ ���� �൵ 1�� �����̰Բ� �������
        if (isDodge)//ȸ������ �����϶� isDodge ==true
        {
            moveVec = dodgeVec; //�������ִ� �������� �̵����� ����
        }
        transform.position += moveVec * Speed * (wDown ? 0.3f : 1) * Time.deltaTime;//�ٲ� �̵������ �ӵ� �׸��� �ȱ⸦ �������� �ȴ������������� �˻��Ͽ� �ӵ��� ����


        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()//���������� ĳ���Ͱ� �ٶ󺸰Բ� ����
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    { 
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)//������ư�� ������ �̵��ϰ� ���� ������ �������λ��°� �ƴϸ� ȸ������ ���°� �ƴҶ�
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);//���޽��� ������� ��
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)//������ư�� ������ �̵��ϰ� ������ �������λ��°� �ƴϸ� ȸ������ ���°� �ƴҶ�
        {
            dodgeVec = moveVec;//ȸ�ǹ��� �������� Ű����� �̵��Ҽ� ���Բ� ����
            Speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;//true�� ���� ȸ������ ����

            Invoke("DodgeOut",0.5f);//�κ�ũ�� �ð����� �ΰ� ���� 0.5�ʵ� �������� Dodgeout �Լ�����
        }
    }

    void DodgeOut()//�ִϸ��̼��� ����Ǵ� Ÿ�ֿ̹� ���� ���� �� �ӵ� ����ȭ, ȸ������ �ƴ����� �ٲ�
    {
        Speed *= 0.5f;
        isDodge = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") // (�±�)�ٴڿ� ��Ҵٸ� �������� �ƴ����� ����(���� �ִϸ� ����)
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }
}
