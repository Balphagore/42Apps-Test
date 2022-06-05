using UnityEngine;
using TMPro;

public class Interface : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TextMeshProUGUI resultText;
    [SerializeField]
    private GameObject crosshair;

    //������� � ������ �� �������.
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

    //����� �������� ������� �� ����� � ����� �� ����� ������.
    private void OnEndGameEvent(string resultText)
    {
        this.resultText.text = "You " + resultText + "!";
        this.resultText.gameObject.SetActive(true);
        crosshair.SetActive(false);
    }
}