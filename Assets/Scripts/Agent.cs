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

    //Подпись и отпись от ивентов.
    private void OnEnable()
    {
        Player.PlayerStartEvent += OnPlayerStartEvent;
    }

    private void OnDisable()
    {
        Player.PlayerStartEvent -= OnPlayerStartEvent;
    }

    //Реализация методов интерфейса IHighlightable.
    //Для подстветки агента используется готовый ассет из ассет стора с соответствующими шейдерами.
    public void Highlight()
    {
        //Подсвечиваем агента оранжевым, если он в прицеле игрока.
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = new Color(1, 0.65f, 0, 1);
    }

    public void HighlightReset()
    {
        //Подсвечиваем агента красным, если он за препятствием.
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
        //Перезарядка выстрела агента
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
        //Передвижение патрулирующих агентов. Если они подходят к краю платформы или к препятствию, то вектор движения разворачивается.
        //Так как агент всегда повернут в одну сторону, а на игрока наводится только оружие, то передвижение агента только по Vector3.right.
        //Если агент подходит к краю платформы или натыкается на препятствие, то по Vector3.right просто умножается на -1
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

        //Взгляд и прицеливание агента. Если рейкаст попадает в игрока и перезарядка завершена, то производится выстрел.
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

    //Получение пули из пула объектов и нацеливание ее на игрока.
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

    //Сохранение renderer игрока при запуске игры чтобы впоследствии целиться в его центр.
    private void OnPlayerStartEvent(Renderer playerRenderer)
    {
        this.playerRenderer = playerRenderer;
    }

    //Реализация метода интерфейса IDamagable
    public void TakeDamage(Vector3 position, Vector3 direction)
    {
        AgentHitEvent?.Invoke();
        gameObject.SetActive(false);
    }
}