using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject hitParticlePrefab;
    [SerializeField]
    private int amountToPool;

    private Stack<GameObject> pooledBullets;
    private Stack<GameObject> pooledHitParticles;

    [Header("References")]
    [SerializeField]
    private Transform bullets;
    [SerializeField]
    private Transform hits;

    //Подпись и отпись от ивентов.
    private void OnEnable()
    {
        Player.GetPooledBulletEvent += OnGetPooledBulletEvent;
        Agent.GetPooledBulletEvent += OnGetPooledBulletEvent;
        Bullet.ReturnPooledBulletEvent += OnReturnPooledBulletEvent;
        Bullet.GetPooledHitParticleEvent += OnGetPooledHitParticleEvent;
        HitParticle.ReturnPooledHitParticleEvent += OnReturnPooledHitParticleEvent;
    }

    private void OnDisable()
    {
        Player.GetPooledBulletEvent -= OnGetPooledBulletEvent;
        Agent.GetPooledBulletEvent -= OnGetPooledBulletEvent;
        Bullet.ReturnPooledBulletEvent -= OnReturnPooledBulletEvent;
        Bullet.GetPooledHitParticleEvent -= OnGetPooledHitParticleEvent;
        HitParticle.ReturnPooledHitParticleEvent -= OnReturnPooledHitParticleEvent;
    }

    private void Start()
    {
        //Создание stack'ов, в которых будут храниться объекты. Инстанциирование объектов.
        pooledBullets = new Stack<GameObject>();
        GameObject bullet;
        for (int i = 0; i < amountToPool; i++)
        {
            bullet = Instantiate(bulletPrefab, bullets);
            bullet.SetActive(false);
            pooledBullets.Push(bullet);
        }
        pooledHitParticles = new Stack<GameObject>();
        GameObject hitParticle;
        for (int i = 0; i < amountToPool; i++)
        {
            hitParticle = Instantiate(hitParticlePrefab, hits);
            hitParticle.SetActive(false);
            pooledHitParticles.Push(hitParticle);
        }
    }

    //Обработка полученных ивентов.
    //Вытаскиваем пулю сверху stack и возвращаем тому, кому она требуется.
    private GameObject OnGetPooledBulletEvent()
    {
        if (pooledBullets.Count > 0)
        {
            GameObject pooledObject = pooledBullets.Peek();
            pooledBullets.Pop();
            return pooledObject;
        }
        return null;
    }

    //Аналогично с эффектом.
    private GameObject OnGetPooledHitParticleEvent()
    {
        if (pooledHitParticles.Count > 0)
        {
            GameObject pooledObject = pooledHitParticles.Peek();
            pooledHitParticles.Pop();
            return pooledObject;
        }
        return null;
    }

    //Получаем использованную пулю, сбрасываем ряд ее параметров и возвращаем ее на вершину stack.
    private void OnReturnPooledBulletEvent(GameObject gameObject)
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        pooledBullets.Push(gameObject);
        gameObject.SetActive(false);
    }

    //Аналогично с эффектом.
    private void OnReturnPooledHitParticleEvent(GameObject gameObject)
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        pooledHitParticles.Push(gameObject);
        gameObject.SetActive(false);
    }
}