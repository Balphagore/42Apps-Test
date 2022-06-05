using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Range(0, 10)]
    [SerializeField]
    private float lifeTime;
    [Range(0, 1000)]
    [SerializeField]
    private float initialVelocity;
    [Range(0, 1000000)]
    [SerializeField]
    private float pushForce;

    [Header("References")]
    [SerializeField]
    private Rigidbody bulletRigidBody;
    private IEnumerator lifeTimeCoroutine;
    private bool isFirstCollide;

    public delegate void ReturnPooledBulletHandler(GameObject gameObject);
    public static event ReturnPooledBulletHandler ReturnPooledBulletEvent;
    public delegate GameObject GetPooledHitParticleHandler();
    public static event GetPooledHitParticleHandler GetPooledHitParticleEvent;

    private void OnEnable()
    {
        //�������� ��������� ���� ����� ����� ���������.
        bulletRigidBody.velocity = transform.rotation * Vector3.forward * initialVelocity;
        isFirstCollide = true;
        //������ ��������, ������������� ����� ����� ����.
        lifeTimeCoroutine = BulletLifeCoroutine();
        StartCoroutine(lifeTimeCoroutine);
    }

    //�������� ������� ����� ����.
    private IEnumerator BulletLifeCoroutine()
    {
        float time = 0;
        while (time < lifeTime)
        {
            time += Time.deltaTime;
            yield return null;
        }
        //����������� ���� � ��� ��������.
        ReturnPooledBulletEvent?.Invoke(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //���� � ����� ����� ����� ��������� ��������� �������� �� ����� ����� ������ ������.
        if (isFirstCollide)
        {
            isFirstCollide = false;
            //��������� ��������, ��� ��� ���� �������� � ��� ��������.
            StopCoroutine(lifeTimeCoroutine);

            //��������� �� ���� �������� ������� ��������� ���� � ����� ��� ���������.
            GameObject hitParticle = GetPooledHitParticleEvent();
            hitParticle.SetActive(true);
            hitParticle.transform.position = transform.position;
            Debug.DrawLine(collision.contacts[0].point, collision.contacts[0].point + collision.contacts[0].normal*2f, Color.yellow, 5f);

            //������� ������� � ����������� �� ���� � ����� ������� ������ ����.
            hitParticle.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);

            //������� ���� � ��� ��������.
            ReturnPooledBulletEvent?.Invoke(gameObject);

            //���� � �������, � ������� ������ ���� ���� ��������� IDamagable, �� �������� ��� ����� ��������� �����.
            IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(collision.contacts[0].point, -collision.contacts[0].normal * pushForce);
            }
        }
    }
}