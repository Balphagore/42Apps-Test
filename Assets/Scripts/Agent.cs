using System;
using UnityEngine;

public class Agent : MonoBehaviour, IHighlightable, IDamagable
{
    [Range(0, 60)]
    [SerializeField]
    private float fireInterval;
    [SerializeField]
    private bool isPatroling;
    [Range(0, 10)]
    [SerializeField]
    private float movingSpeed;

    private float reloadTimer;
    private bool isReadyToFire;
    private bool isGround;
    private Vector3 movementVector;

    [Header("References")]
    [SerializeField]
    private Renderer playerRenderer;
    [SerializeField]
    private Outline outline;
    [SerializeField]
    private Transform gunHolder;
    [SerializeField]
    private GameObject bulletSpawnPoint;
    [SerializeField]
    private Rigidbody enemyRigidbody;

    public delegate GameObject GetPooledBulletHandler();
    public static GetPooledBulletHandler GetPooledBulletEvent;
    public delegate void AgentHitHandler();
    public static AgentHitHandler AgentHitEvent;

    //������� � ������ �� �������.
    private void OnEnable()
    {
        Player.PlayerStartEvent += OnPlayerStartEvent;
    }

    private void OnDisable()
    {
        Player.PlayerStartEvent -= OnPlayerStartEvent;
    }

    //���������� ������� ���������� IHighlightable.
    //��� ���������� ������ ������������ ������� ����� �� ����� ����� � ���������������� ���������.
    public void Highlight()
    {
        //������������ ������ ���������, ���� �� � ������� ������.
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = new Color(1, 0.65f, 0, 1);
    }

    public void HighlightReset()
    {
        //������������ ������ �������, ���� �� �� ������������.
        outline.OutlineMode = Outline.Mode.OutlineHidden;
        outline.OutlineColor = Color.red;
    }

    private void Start()
    {
        reloadTimer = fireInterval;
        movementVector = Vector3.right;
    }

    private void Update()
    {
        //����������� �������� ������
        if (reloadTimer > 0)
        {
            reloadTimer -= Time.deltaTime;
        }
        else
        {
            isReadyToFire = true;
        }
    }

    private void FixedUpdate()
    {
        //������������ ������������� �������. ���� ��� �������� � ���� ��������� ��� � �����������, �� ������ �������� ���������������.
        //��� ��� ����� ������ �������� � ���� �������, � �� ������ ��������� ������ ������, �� ������������ ������ ������ �� Vector3.right.
        //���� ����� �������� � ���� ��������� ��� ���������� �� �����������, �� �� Vector3.right ������ ���������� �� -1
        if (isPatroling)
        {
            isGround = false;
            foreach (Collider collider in Physics.OverlapSphere(transform.position, 0.4f))
            {
                if (collider.gameObject.tag == "Floor")
                {
                    isGround = true;
                    break;
                }
            }
            foreach (Collider collider in Physics.OverlapSphere(transform.position, 1f))
            {
                if (collider.gameObject.tag == "Obstacle")
                {
                    movementVector *= -1;
                    break;
                }
            }
            if (!isGround)
            {
                movementVector *= -1;
            }
            enemyRigidbody.velocity = transform.TransformDirection(movementVector * movingSpeed);
        }

        //������ � ������������ ������. ���� ������� �������� � ������ � ����������� ���������, �� ������������ �������.
        gunHolder.LookAt(playerRenderer.bounds.center);
        RaycastHit hit;
        Ray ray = new Ray(gunHolder.position, playerRenderer.bounds.center - gunHolder.position);
        Debug.DrawLine(gunHolder.position, playerRenderer.bounds.center, new Color(0.3f, 0, 0));
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
        {
            if (hit.collider.tag == "Player")
            {
                Debug.DrawLine(gunHolder.position, hit.point, Color.red);
                if (isReadyToFire)
                {
                    Fire();
                }
            }
        }
    }

    //��������� ���� �� ���� �������� � ����������� �� �� ������.
    private void Fire()
    {
        reloadTimer = fireInterval;
        isReadyToFire = false;
        GameObject bullet = GetPooledBulletEvent();
        if (bullet != null)
        {
            bullet.transform.position = bulletSpawnPoint.transform.position;
            bullet.transform.rotation = gunHolder.transform.rotation;
            bullet.SetActive(true);
        }
    }

    //���������� renderer ������ ��� ������� ���� ����� ������������ �������� � ��� �����.
    private void OnPlayerStartEvent(Renderer playerRenderer)
    {
        this.playerRenderer = playerRenderer;
    }

    //���������� ������ ���������� IDamagable
    public void TakeDamage(Vector3 position, Vector3 direction)
    {
        AgentHitEvent?.Invoke();
        gameObject.SetActive(false);
    }
}