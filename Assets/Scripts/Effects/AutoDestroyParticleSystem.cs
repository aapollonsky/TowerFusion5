using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Component that automatically destroys the GameObject after the particle system finishes playing.
    /// Attach this to particle effect prefabs that should clean themselves up.
    /// </summary>
    public class AutoDestroyParticleSystem : MonoBehaviour
    {
        private ParticleSystem ps;
        private float destroyTime;
        
        void Start()
        {
            ps = GetComponent<ParticleSystem>();
            
            if (ps != null)
            {
                // Calculate when to destroy: duration + max particle lifetime
                destroyTime = ps.main.duration + ps.main.startLifetime.constantMax;
                
                // Schedule destruction
                Destroy(gameObject, destroyTime);
                
                Debug.Log($"[AutoDestroy] {gameObject.name} will auto-destroy in {destroyTime:F2}s");
            }
            else
            {
                Debug.LogWarning($"[AutoDestroy] No ParticleSystem found on {gameObject.name}, destroying in 2s");
                Destroy(gameObject, 2f);
            }
        }
    }
}
