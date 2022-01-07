using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera followCamera;

    public float Speed;//캐릭터 속도 변수

    public GameObject[] weapons;//어떤 무기를 쓰는지에대해
    public bool[] hasWeapons;//가지고 있는지에 대해
    public GameObject[] grenades;//공전수류탄의 오브젝트
    public int hasGrenades;//수류탄 가진 개수
    public GameObject grenadeObj;

    public int ammo;
    public int coin;
    public int health;//아이템 소유확인
    

    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;//아이템 최대개수들

    float hAxis;
    float vAxis;
    
    bool wDown;//걷기버튼 변수
    bool jDown;//점프버튼 변수
    bool iDown;//상호작용버튼 변수
    bool sDown1;
    bool sDown2;
    bool sDown3;//스왑장비 버튼 변수들
    bool fDown;//공격버튼 변수
    bool rDown;//재장전버튼 변수
    bool gDown;//수류탄 던지기버튼 변수

    bool isJump;//지금 점프를 하는지에 대해 검사하는 변수
    bool isDodge;//지금 회피를 하는지에 대해 검사하는 변수
    bool isSwap;//지금 무기를 바꾸는중인지에 대해 검사하는 변수
    bool isFireReady = true;//공격준비 완료인지 검사하는 함수
    bool isReload; //장전중인지에 대해 검사하는 함수
    bool isBorder;
    bool isDamage;//무적시간
    bool isShop;


    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;//자식관계에서 활동중 컴포넌트 차일드 활용
    Rigidbody rigid;
    MeshRenderer[] meshs;//플레이어의 몸은 머리,팔,다리가 나뉘어있기에 배열로 가져와야함

    GameObject nearObject;//근처 오브젝트를 담는 변수
    Weapon equipWeapons;

    int equipWeaponIndex = -1;
    float fireDelay;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();//복수물체를 가져올때는 겟컴포넌트s
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

    void GetInput()//다 전역변수로 설정되있기에 여기서 바뀌면 다른것들도 영향
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
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // ★else로 따로 만들어 가야하지 않나 생각중... normalized는 대각선으로 가는 축도 1만 움직이게끔 만들어줌
        if (isDodge)//회피중인 상태일때 isDodge ==true
        {
            moveVec = dodgeVec; //고정되있던 방향으로 이동방향 고정
        }
        if (isSwap || isReload || !isFireReady)
        {
            moveVec = Vector3.zero;
        }
        if (!isBorder)
        {
            transform.position += 
                moveVec * Speed * (wDown ? 0.3f : 1) * Time.deltaTime;
            //바뀐 이동방향과 속도 그리고 걷기를 눌렀는지 안눌렀는지에대해 검사하여 속도를 조정
        }
        


        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()//누른곳으로 캐릭터가 바라보게끔 설정
    {
        //키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //마우스에 의한 회전
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
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isShop)//점프버튼을 누르고 이동하고 있지 않으며 점프중인상태가 아니며 회피중인 상태가 아닐때
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);//임펄스는 즉발적인 힘
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;//true인동안 점프중인 상태
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

        fireDelay += Time.deltaTime;//공격딜레이를 시간초만큼 더한다.
        isFireReady = equipWeapons.rate < fireDelay;//공격속도보다 딜레이가 더 높다면 공격준비!

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
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)//점프버튼을 누르고 이동하고 있으며 점프중인상태가 아니며 회피중인 상태가 아닐때
        {
            dodgeVec = moveVec;//회피방향 고정으로 키보드로 이동할수 없게끔 고정
            Speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;//true인 동안 회피중인 상태

            Invoke("DodgeOut",0.5f);//인보크는 시간차를 두고 실행 0.5초뒤 실행으로 Dodgeout 함수실행
        }
    }

    void DodgeOut()//애니메이션이 종료되는 타이밍에 맞춰 종료 및 속도 정상화, 회피중이 아님으로 바꿈
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

    void SwapOut()//애니메이션이 종료되는 타이밍에 맞춰 종료, 스왑중이 아님으로 바꿈
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
        if (collision.gameObject.tag == "Floor") // (태그)바닥에 닿았다면 점프중이 아님으로 변경(착지 애니메 실행)
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;//물리 회전 속도(충돌하여 회전력이 생기는 버그 방지)
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
