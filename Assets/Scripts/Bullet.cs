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
        //Придание ускорения пули сразу после активации.
        bulletRigidBody.velocity = transform.rotation * Vector3.forward * initialVelocity;
        isFirstCollide = true;
        //Запуск корутины, отслеживающей время жизни пули.
        lifeTimeCoroutine = BulletLifeCoroutine();
        StartCoroutine(lifeTimeCoroutine);
    }

    //Корутина времени жизни пули.
    private IEnumerator BulletLifeCoroutine()
    {
        float time = 0;
        while (time < lifeTime)
        {
            time += Time.deltaTime;
            yield return null;
        }
        //Возвращение пули в пул объектов.
        ReturnPooledBulletEvent?.Invoke(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Даже в одном кадре может произойти несколько коллизий по этому берем только первую.
        if (isFirstCollide)
        {
            isFirstCollide = false;
            //Остановка корутины, так как пуля вернется в пул объектов.
            StopCoroutine(lifeTimeCoroutine);

            //Получение из пула объектов эффекта попадания пули а затем его активация.
            GameObject hitParticle = GetPooledHitParticleEvent();
            hitParticle.SetActive(true);
            hitParticle.transform.position = transform.position;
            Debug.DrawLine(collision.contacts[0].point, collision.contacts[0].point + collision.contacts[0].normal*2f, Color.yellow, 5f);

            //Поворот эффекта в зависимости от того в какую нормаль попала пуля.
            hitParticle.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);

            //возврат пули в пул объектов.
            ReturnPooledBulletEvent?.Invoke(gameObject);

            //Если у объекта, в который попала пуля есть интерфейс IDamagable, то вызываем его метод получения урона.
            IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(collision.contacts[0].point, -collision.contacts[0].normal * pushForce);
            }
        }
    }
}