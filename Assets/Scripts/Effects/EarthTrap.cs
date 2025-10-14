using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// Ground disk created by Earth trait that damages enemies walking over it
    /// </summary>
    public class EarthTrap : MonoBehaviour
    {
        [Header("Disk Configuration")]
        [SerializeField] private float duration = 3f;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float damagePercent = 0.3f; // 30% of max health
        
        [Header("Visual Effects")]
        [SerializeField] private SpriteRenderer diskSprite;
        [SerializeField] private ParticleSystem diskParticles;
        [SerializeField] private CircleCollider2D diskCollider;
        
        private float timeRemaining;
        private HashSet<Enemy> damagedEnemies = new HashSet<Enemy>(); // Track enemies already damaged
        
        private void Awake()
        {
            if (diskCollider == null)
            {
                diskCollider = gameObject.AddComponent<CircleCollider2D>();
                diskCollider.isTrigger = true;
            }
            
            diskCollider.radius = radius;
        }
        
        /// <summary>
        /// Initialize disk with custom parameters
        /// </summary>
        public void Initialize(float diskDuration, float diskRadius)
        {
            duration = diskDuration;
            radius = diskRadius;
            timeRemaining = duration;
            
            if (diskCollider != null)
            {
                diskCollider.radius = diskRadius;
            }
            
            // Find sprite renderer if not assigned
            if (diskSprite == null)
            {
                diskSprite = GetComponent<SpriteRenderer>();
            }
            
            // Make disk brown semi-transparent
            if (diskSprite != null)
            {
                Color brownColor = new Color(0.55f, 0.35f, 0.2f, 0.6f); // Brown semi-transparent
                diskSprite.color = brownColor;
                
                // Scale visual to match radius
                transform.localScale = Vector3.one * (diskRadius * 1f);
            }
            
            // Start particle effects if available
            if (diskParticles != null && !diskParticles.isPlaying)
            {
                diskParticles.Play();
            }
            
            Debug.Log($"<color=brown>Brown Earth Disk created at {transform.position} (Duration: {duration}s, Damage: {damagePercent * 100}%)</color>");
        }
        
        private void Update()
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0)
            {
                DestroyDisk();
                return;
            }
            
            // Update visual fade as disk is about to disappear
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update disk visuals based on remaining time
        /// </summary>
        private void UpdateVisuals()
        {
            if (diskSprite != null)
            {
                // Fade out in last 0.5 seconds
                float alpha = timeRemaining < 0.5f ? (timeRemaining / 0.5f * 0.6f) : 0.6f; // Keep semi-transparent
                Color color = diskSprite.color;
                color.a = alpha;
                diskSprite.color = color;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive && !damagedEnemies.Contains(enemy))
            {
                // Damage enemy for 50% of their max health
                float damageAmount = enemy.MaxHealth * damagePercent;
                enemy.TakeDamage(damageAmount, DamageType.Magic);
                
                // Track that we damaged this enemy (don't damage again)
                damagedEnemies.Add(enemy);
                
                Debug.Log($"<color=brown>Enemy {enemy.name} walked over Earth Disk - took {damageAmount} damage ({damagePercent * 100}% of max health)</color>");
            }
        }
        
        /// <summary>
        /// Destroy disk and cleanup
        /// </summary>
        private void DestroyDisk()
        {
            Debug.Log($"<color=brown>Earth Disk expired at {transform.position}</color>");
            
            // Clear tracked enemies
            damagedEnemies.Clear();
            
            // Stop particles
            if (diskParticles != null)
            {
                diskParticles.Stop();
            }
            
            // Destroy after particles finish
            Destroy(gameObject, diskParticles != null ? diskParticles.main.duration : 0f);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw disk radius
            Gizmos.color = new Color(0.55f, 0.35f, 0.2f, 0.5f); // Brown for disk
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawSphere(transform.position, radius * 0.1f); // Center dot
        }
    }
}
