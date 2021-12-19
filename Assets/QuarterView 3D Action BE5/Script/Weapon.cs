using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    public int damage;//데미지
    public float rate;//공격속도
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        //yield return null;//1프레임 대기

        yield return new WaitForSeconds(0.1f);//n초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;

        //yield break;//이 이후 아래쪽 로직은 비활성화

    }

    IEnumerator Shot()
    {
        //총알 발사
        GameObject instantBullt = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullt.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
        yield return null;

        //탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody CaseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 CaseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        CaseRigid.AddForce(CaseVec, ForceMode.Impulse);//배출 속도
        CaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);//회전

    }
}
