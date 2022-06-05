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

    //������� � ������ �� �������.
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
        //�������� stack'��, � ������� ����� ��������� �������. ���������������� ��������.
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

    //��������� ���������� �������.
    //����������� ���� ������ stack � ���������� ����, ���� ��� ���������.
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

    //���������� � ��������.
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

    //�������� �������������� ����, ���������� ��� �� ���������� � ���������� �� �� ������� stack.
    private void OnReturnPooledBulletEvent(GameObject gameObject)
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        pooledBullets.Push(gameObject);
        gameObject.SetActive(false);
    }

    //���������� � ��������.
    private void OnReturnPooledHitParticleEvent(GameObject gameObject)
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        pooledHitParticles.Push(gameObject);
        gameObject.SetActive(false);
    }
}