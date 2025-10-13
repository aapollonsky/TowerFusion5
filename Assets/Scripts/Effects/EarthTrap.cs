using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// Ground hole created by Earth trait that swallows enemies on contact
    /// </summary>
    public class EarthTrap : MonoBehaviour
    {
        [Header("Hole Configuration")]
        [SerializeField] private float duration = 3f;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float swallowDuration = 0.5f; // Time for enemy to fall in
        
        [Header("Visual Effects")]
        [SerializeField] private SpriteRenderer holeSprite;
        [SerializeField] private ParticleSystem holeParticles;
        [SerializeField] private CircleCollider2D holeCollider;
        
        private float timeRemaining;
        private HashSet<Enemy> swallowedEnemies = new HashSet<Enemy>(); // Track enemies being swallowed
        private Dictionary<Enemy, Coroutine> swallowCoroutines = new Dictionary<Enemy, Coroutine>();
        
        private void Awake()
        {
            if (holeCollider == null)
            {
                holeCollider = gameObject.AddComponent<CircleCollider2D>();
                holeCollider.isTrigger = true;
            }
            
            holeCollider.radius = radius;
        }
        
        /// <summary>
        /// Initialize hole with custom parameters
        /// </summary>
        public void Initialize(float holeDuration, float holeRadius)
        {
            duration = holeDuration;
            radius = holeRadius;
            timeRemaining = duration;
            
            if (holeCollider != null)
            {
                holeCollider.radius = holeRadius;
            }
            
            // Find sprite renderer if not assigned
            if (holeSprite == null)
            {
                holeSprite = GetComponent<SpriteRenderer>();
            }
            
            // Scale visual to match radius (disk size) - reduced to half size
            if (holeSprite != null)
            {
                transform.localScale = Vector3.one * (holeRadius * 1f); // Changed from 2f to 1f
            }
            
            // Start particle effects if available
            if (holeParticles != null && !holeParticles.isPlaying)
            {
                holeParticles.Play();
            }
            
            Debug.Log($"Black Disk (Earth Hole) initialized at {transform.position} (Duration: {duration}s, Radius: {radius})");
        }
        
        private void Update()
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0)
            {
                DestroyHole();
                return;
            }
            
            // Update visual fade as hole is about to close
            UpdateVisuals();
        }
        
        /// <summary>
        /// Update hole visuals based on remaining time
        /// </summary>
        private void UpdateVisuals()
        {
            if (holeSprite != null)
            {
                // Fade out in last 0.5 seconds
                float alpha = timeRemaining < 0.5f ? (timeRemaining / 0.5f) : 1f;
                Color color = holeSprite.color;
                color.a = alpha;
                holeSprite.color = color;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive && !swallowedEnemies.Contains(enemy))
            {
                // Check if enemy is near the center of the disk (not just the edge)
                float distanceFromCenter = Vector2.Distance(enemy.transform.position, transform.position);
                float centerRadius = radius * 0.3f; // Only trigger when within 30% of disk radius (center area)
                
                if (distanceFromCenter <= centerRadius)
                {
                    // Start swallowing the enemy
                    swallowedEnemies.Add(enemy);
                    Coroutine swallowCoroutine = StartCoroutine(SwallowEnemy(enemy));
                    swallowCoroutines[enemy] = swallowCoroutine;
                    Debug.Log($"Enemy {enemy.name} fell into Earth Hole center!");
                }
            }
        }
        
        /// <summary>
        /// Swallow enemy into the hole with animation
        /// </summary>
        private System.Collections.IEnumerator SwallowEnemy(Enemy enemy)
        {
            if (enemy == null) yield break;
            
            Vector3 startPosition = enemy.transform.position;
            Vector3 holeCenter = transform.position;
            Vector3 startScale = enemy.transform.localScale;
            
            float elapsed = 0f;
            
            // Immediately kill the enemy (no health remaining)
            if (enemy.IsAlive)
            {
                enemy.TakeDamage(enemy.MaxHealth * 100f, DamageType.Magic); // Overkill
                Debug.Log($"Enemy {enemy.name} killed by Earth Hole");
            }
            
            // Animate enemy falling into hole
            while (elapsed < swallowDuration && enemy != null && enemy.gameObject != null)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / swallowDuration;
                
                // Move toward hole center
                enemy.transform.position = Vector3.Lerp(startPosition, holeCenter, progress);
                
                // Scale down (disappear into hole)
                float scale = Mathf.Lerp(1f, 0f, progress);
                enemy.transform.localScale = startScale * scale;
                
                // Rotate for spiral effect
                enemy.transform.Rotate(Vector3.forward, 720f * Time.deltaTime);
                
                yield return null;
            }
            
            // Destroy the enemy GameObject (it fell into the hole)
            if (enemy != null && enemy.gameObject != null)
            {
                Debug.Log($"Enemy {enemy.name} swallowed by Earth Hole - destroying GameObject");
                Destroy(enemy.gameObject);
            }
            
            // Cleanup
            swallowCoroutines.Remove(enemy);
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && swallowedEnemies.Contains(enemy))
            {
                // If enemy somehow exits before being fully swallowed, stop the coroutine
                if (swallowCoroutines.ContainsKey(enemy))
                {
                    StopCoroutine(swallowCoroutines[enemy]);
                    swallowCoroutines.Remove(enemy);
                }
                swallowedEnemies.Remove(enemy);
                
                // Restore enemy scale if it escaped
                if (enemy != null)
                {
                    enemy.transform.localScale = Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// Destroy hole and cleanup
        /// </summary>
        private void DestroyHole()
        {
            Debug.Log($"Earth Hole closed at {transform.position}");
            
            // Stop all swallow coroutines
            foreach (var coroutine in swallowCoroutines.Values)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            swallowCoroutines.Clear();
            swallowedEnemies.Clear();
            
            // Stop particles
            if (holeParticles != null)
            {
                holeParticles.Stop();
            }
            
            // Destroy after particles finish
            Destroy(gameObject, holeParticles != null ? holeParticles.main.duration : 0f);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw hole radius
            Gizmos.color = new Color(0.2f, 0.1f, 0.05f, 0.5f); // Darker for hole
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawSphere(transform.position, radius * 0.1f); // Center dot
        }
    }
}
