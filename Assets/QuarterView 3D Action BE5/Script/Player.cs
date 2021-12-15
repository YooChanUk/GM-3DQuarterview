using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;//ĳ���� �ӵ� ����
    public GameObject[] weapons;
    public bool[] hasWeapons;

    float hAxis;
    float vAxis;
    
    bool wDown;//�ȱ��ư ����
    bool jDown;//������ư ����
    bool iDown;//��ȣ�ۿ��ư ����
    bool sDown1;
    bool sDown2;
    bool sDown3;//������� ��ư ������

    bool isJump;//���� ������ �ϴ����� ���� �˻��ϴ� ����
    bool isDodge;//���� ȸ�Ǹ� �ϴ����� ���� �˻��ϴ� ����
    bool isSwap;//���� ���⸦ �ٲٴ��������� ���� �˻��ϴ� ����


    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;//�ڽİ��迡�� Ȱ���� ������Ʈ ���ϵ� Ȱ��
    Rigidbody rigid;

    GameObject nearObject;//��ó ������Ʈ�� ��� ����
    GameObject equipWeapons;

    int equipWeaponIndex = -1;

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
        Swap();
        Interation();
    }

    void GetInput()//�� ���������� �������ֱ⿡ ���⼭ �ٲ�� �ٸ��͵鵵 ����
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // ��else�� ���� ����� �������� �ʳ� ������... normalized�� �밢������ ���� �൵ 1�� �����̰Բ� �������
        if (isDodge)//ȸ������ �����϶� isDodge ==true
        {
            moveVec = dodgeVec; //�������ִ� �������� �̵����� ����
        }
        if (isSwap)
        {
            moveVec = Vector3.zero;
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
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)//������ư�� ������ �̵��ϰ� ���� ������ �������λ��°� �ƴϸ� ȸ������ ���°� �ƴҶ�
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);//���޽��� ������� ��
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;//true�ε��� �������� ����
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)//������ư�� ������ �̵��ϰ� ������ �������λ��°� �ƴϸ� ȸ������ ���°� �ƴҶ�
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

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
        {
            return;
        }
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
        {
            return;
        }
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
        {
            return;
        }

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isDodge && !isJump)
        {
            if (equipWeapons != null)
            {
                equipWeapons.SetActive(false);
            }

            equipWeaponIndex = weaponIndex;
            equipWeapons = weapons[weaponIndex];
            equipWeapons.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()//�ִϸ��̼��� ����Ǵ� Ÿ�ֿ̹� ���� ����, �������� �ƴ����� �ٲ�
    {
        isSwap = false;
    }

    void Interation()
    { 
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") // (�±�)�ٴڿ� ��Ҵٸ� �������� �ƴ����� ����(���� �ִϸ� ����)
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;

            Debug.Log(nearObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }


}
