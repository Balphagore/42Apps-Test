using UnityEngine;

public class HitParticle : MonoBehaviour
{
    public delegate void ReturnPooledHitParticleHandler(GameObject gameObject);
    public static event ReturnPooledHitParticleHandler ReturnPooledHitParticleEvent;

    //����������� ������� � ��� ��������, ����� �� ��������� ����������.
    private void OnDisable()
    {
        ReturnPooledHitParticleEvent?.Invoke(gameObject);
    }
}