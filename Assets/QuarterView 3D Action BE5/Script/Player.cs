using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;//캐릭터 속도 변수
    float hAxis;
    float vAxis;
    bool wDown;//걷기버튼 변수
    bool jDown;//점프버튼 변수

    bool isJump; //지금 점프를 하는지에 대해 검사하는 변수
    bool isDodge;//지금 회피를 하는지에 대해 검사하는 변수

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;//자식관계에서 활동중 컴포넌트 차일드 활용
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

    void GetInput()//다 전역변수로 설정되있기에 여기서 바뀌면 다른것들도 영향
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // ★else로 따로 만들어 가야하지 않나 생각중... normalized는 대각선으로 가는 축도 1만 움직이게끔 만들어줌
        if (isDodge)//회피중인 상태일때 isDodge ==true
        {
            moveVec = dodgeVec; //고정되있던 방향으로 이동방향 고정
        }
        transform.position += moveVec * Speed * (wDown ? 0.3f : 1) * Time.deltaTime;//바뀐 이동방향과 속도 그리고 걷기를 눌렀는지 안눌렀는지에대해 검사하여 속도를 조정


        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()//누른곳으로 캐릭터가 바라보게끔 설정
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    { 
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)//점프버튼을 누르고 이동하고 있지 않으며 점프중인상태가 아니며 회피중인 상태가 아닐때
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);//임펄스는 즉발적인 힘
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)//점프버튼을 누르고 이동하고 있으며 점프중인상태가 아니며 회피중인 상태가 아닐때
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") // (태그)바닥에 닿았다면 점프중이 아님으로 변경(착지 애니메 실행)
        {
            isJump = false;
            anim.SetBool("isJump", false);
        }
    }
}
