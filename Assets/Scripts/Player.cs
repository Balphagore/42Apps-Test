using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IDamagable
{
    [Range(0, 10)]
    [SerializeField]
    private float movingSpeed;
    [Range(0, 2)]
    [SerializeField]
    private float sensitivity;

    private float cameraRotation;
    private Vector3 movementVector;
    private bool isGround;
    private Vector3 bulletDirection;
    private IHighlightable highlightable;

    [Header("References")]
    [SerializeField]
    private Rigidbody playerRigidbody;
    [SerializeField]
    private GameObject bulletSpawnPoint;

    public delegate void EndGameHandler(string resultText);
    public static event EndGameHandler EndGameEvent;
    public delegate GameObject GetPooledBulletHandler();
    public static event GetPooledBulletHandler GetPooledBulletEvent;
    public delegate void PlayerStartHandler(Renderer playerRenderer);
    public static event PlayerStartHandler PlayerStartEvent;
    public delegate void PlayerShootHandler();
    public static event PlayerShootHandler PlayerShootEvent;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerStartEvent?.Invoke(transform.GetComponentInChildren<Renderer>());
    }

    private void FixedUpdate()
    {
        //Проверка находится ли игрок на земле
        isGround = false;
        foreach (Collider collider in Physics.OverlapSphere(transform.position, 0.4f))
        {
            if (collider.gameObject.tag == "Floor")
            {
                isGround = true;
                break;
            }
        }

        //Передвижение если игрок на земле
        if (isGround)
        {
            if (movementVector != Vector3.zero)
            {
                playerRigidbody.velocity = transform.TransformDirection(movementVector * movingSpeed);
            }
            else
            {
                playerRigidbody.velocity = Vector3.zero;
            }
        }
        playerRigidbody.velocity = Vector3.ClampMagnitude(playerRigidbody.velocity, 10);

        //Логика прицеливания
        //Так как глаза и оружие игрока направлены в разные места, то сначала пускаем рейкаст из камеры игрока, а затем нацеливаем пулю в точку попадания рейкаста.
        RaycastHit hit;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask.GetMask("Aimable")))
        {
            Debug.DrawLine(Camera.main.transform.position, hit.point);
            OnAim(hit.collider.gameObject.GetComponentInParent<IHighlightable>());
            bulletDirection = hit.point;
        }
        else
        {
            bulletDirection = ray.GetPoint(100);
            if (highlightable != null)
            {
                highlightable.HighlightReset();
                highlightable = null;
            }
        }
        Debug.DrawLine(bulletSpawnPoint.transform.position, bulletDirection, Color.green);
    }

    //Подсветка агента
    private void OnAim(IHighlightable highlightable)
    {
        if (highlightable != null)
        {
            if (highlightable != this.highlightable)
            {
                if (this.highlightable != null)
                {
                    this.highlightable.HighlightReset();
                }
                this.highlightable = highlightable;
                highlightable.Highlight();
            }
        }
        else
        {
            if (this.highlightable != null)
            {
                this.highlightable.HighlightReset();
                this.highlightable = null;
            }
        }
    }

    //Взаимодействие с триггерами вызывающими победу или поражение
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "LoseTrigger")
        {
            StartCoroutine("RestartCoroutine");
            EndGameEvent?.Invoke("Lose");
        }
        else
        {
            if (other.gameObject.tag == "WinTrigger")
            {
                StartCoroutine("RestartCoroutine");
                EndGameEvent?.Invoke("Win");
            }
        }
    }

    //Рестарт сцены через 5 секунд после вызова корутины
    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(0);
    }

    //Обработка ивентов InputSystem
    public void OnMovement(InputAction.CallbackContext value)
    {
        movementVector = new Vector3(value.ReadValue<Vector2>().x, 0, value.ReadValue<Vector2>().y);
    }

    public void OnLook(InputAction.CallbackContext value)
    {
        transform.Rotate(0, value.ReadValue<Vector2>().x * sensitivity, 0);
        cameraRotation -= value.ReadValue<Vector2>().y * sensitivity;
        cameraRotation = Mathf.Clamp(cameraRotation, -90, 80);
        Camera.main.transform.localRotation = Quaternion.Euler(cameraRotation, 0, 0);
    }

    public void OnShoot(InputAction.CallbackContext value)
    {
        if (value.phase == InputActionPhase.Performed)
        {
            GameObject bullet = GetPooledBulletEvent();
            if (bullet != null)
            {
                bullet.transform.position = bulletSpawnPoint.transform.position;
                bullet.transform.LookAt(bulletDirection);
                bullet.SetActive(true);
                PlayerShootEvent?.Invoke();
            }
        }
    }

    //Реализация метода интерфейся IDamagable
    public void TakeDamage(Vector3 position, Vector3 push)
    {
        playerRigidbody.AddForceAtPosition(push, position, ForceMode.Impulse);
    }
}