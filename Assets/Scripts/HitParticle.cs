using UnityEngine;

public class HitParticle : MonoBehaviour
{
    public delegate void ReturnPooledHitParticleHandler(GameObject gameObject);
    public static event ReturnPooledHitParticleHandler ReturnPooledHitParticleEvent;

    //Возвращение эффекта в пул объектов, когда он полностью завершился.
    private void OnDisable()
    {
        ReturnPooledHitParticleEvent?.Invoke(gameObject);
    }
}