using UnityEngine;
using TMPro;

public class Interface : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TextMeshProUGUI resultText;
    [SerializeField]
    private GameObject crosshair;

    //Подпись и отпись от ивентов.
    private void OnEnable()
    {
        Player.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        Player.EndGameEvent -= OnEndGameEvent;
    }

    private void Start()
    {
        resultText.gameObject.SetActive(false);
    }

    //Вывод итоговой надписи на экран в ответ на ивент игрока.
    private void OnEndGameEvent(string resultText)
    {
        this.resultText.text = "You " + resultText + "!";
        this.resultText.gameObject.SetActive(true);
        crosshair.SetActive(false);
    }
}