using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera followCamera;

    public float Speed;//ĳ���� �ӵ� ����

    public GameObject[] weapons;//� ���⸦ ������������
    public bool[] hasWeapons;//������ �ִ����� ����
    public GameObject[] grenades;//��������ź�� ������Ʈ
    public int hasGrenades;//����ź ���� ����
    public GameObject grenadeObj;

    public int ammo;
    public int coin;
    public int health;//������ ����Ȯ��
    

    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;//������ �ִ밳����

    float hAxis;
    float vAxis;
    
    bool wDown;//�ȱ��ư ����
    bool jDown;//������ư ����
    bool iDown;//��ȣ�ۿ��ư ����
    bool sDown1;
    bool sDown2;
    bool sDown3;//������� ��ư ������
    bool fDown;//���ݹ�ư ����
    bool rDown;//��������ư ����
    bool gDown;//����ź �������ư ����

    bool isJump;//���� ������ �ϴ����� ���� �˻��ϴ� ����
    bool isDodge;//���� ȸ�Ǹ� �ϴ����� ���� �˻��ϴ� ����
    bool isSwap;//���� ���⸦ �ٲٴ��������� ���� �˻��ϴ� ����
    bool isFireReady = true;//�����غ� �Ϸ����� �˻��ϴ� �Լ�
    bool isReload; //������������ ���� �˻��ϴ� �Լ�
    bool isBorder;
    bool isDamage;//�����ð�
    bool isShop;


    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;//�ڽİ��迡�� Ȱ���� ������Ʈ ���ϵ� Ȱ��
    Rigidbody rigid;
    MeshRenderer[] meshs;//�÷��̾��� ���� �Ӹ�,��,�ٸ��� �������ֱ⿡ �迭�� �����;���

    GameObject nearObject;//��ó ������Ʈ�� ��� ����
    Weapon equipWeapons;

    int equipWeaponIndex = -1;
    float fireDelay;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();//������ü�� �����ö��� ��������Ʈs
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
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
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
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
        if (isSwap || isReload || !isFireReady)
        {
            moveVec = Vector3.zero;
        }
        if (!isBorder)
        {
            transform.position += 
                moveVec * Speed * (wDown ? 0.3f : 1) * Time.deltaTime;
            //�ٲ� �̵������ �ӵ� �׸��� �ȱ⸦ �������� �ȴ������������� �˻��Ͽ� �ӵ��� ����
        }
        


        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()//���������� ĳ���Ͱ� �ٶ󺸰Բ� ����
    {
        //Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec);

        //���콺�� ���� ȸ��
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }

    }

    void Jump()
    { 
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isShop)//������ư�� ������ �̵��ϰ� ���� ������ �������λ��°� �ƴϸ� ȸ������ ���°� �ƴҶ�
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);//���޽��� ������� ��
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;//true�ε��� �������� ����
        }
    }

    void Grenade()
    {
        if (hasGrenades == 0)
        {
            return;
        }
        if (gDown && !isReload && !isSwap && !isShop)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back *10, ForceMode.Impulse);

                hasGrenades -= 1;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Attack()
    { 
        if (equipWeapons == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;//���ݵ����̸� �ð��ʸ�ŭ ���Ѵ�.
        isFireReady = equipWeapons.rate < fireDelay;//���ݼӵ����� �����̰� �� ���ٸ� �����غ�!

        if (fDown && isFireReady && !isDodge && !isSwap && !isShop)
        {
            equipWeapons.Use();
            anim.SetTrigger(equipWeapons.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapons == null)
        {
            return;
        }
        if (equipWeapons.type == Weapon.Type.Melee)
        {
            return;
        }
        if (ammo <= 0)
        {
            return;
        }

        if (rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut",2.5f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapons.maxAmmo ? ammo : equipWeapons.maxAmmo;
        equipWeapons.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
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
                equipWeapons.gameObject.SetActive(false);
            }

            equipWeaponIndex = weaponIndex;
            equipWeapons = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapons.gameObject.SetActive(true);

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
            else if(nearObject.tag == "Shop")
            { 
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
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

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;//���� ȸ�� �ӵ�(�浹�Ͽ� ȸ������ ����� ���� ����)
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, moveVec * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, moveVec, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxammo)
                    {
                        ammo = maxammo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                    {
                        coin = maxcoin;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxhealth)
                    {
                        health = maxhealth;
                    }
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);

                    hasGrenades += item.value;
                    if (hasGrenades > maxhasGrenades)
                    {
                        hasGrenades = maxhasGrenades;
                    }
                    break;

            }

            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk));
            }

            if (other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }

        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        if (isBossAtk)
        {
            rigid.AddForce(transform.forward * -25,ForceMode.Impulse);
        }

        yield return new WaitForSeconds(1f);

        if (isBossAtk)
        {
            rigid.velocity = Vector3.zero;
        }

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
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
        else if (other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }


}
