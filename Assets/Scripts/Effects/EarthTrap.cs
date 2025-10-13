using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// Ground trap created by Earth trait that swallows enemies
    /// </summary>
    public class EarthTrap : MonoBehaviour
    {
        [Header("Trap Configuration")]
        [SerializeField] private float duration = 4f;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float pullStrength = 3f;
        [SerializeField] private float damagePerSecond = 5f;
        
        [Header("Visual Effects")]
        [SerializeField] private SpriteRenderer trapSprite;
        [SerializeField] private ParticleSystem trapParticles;
        [SerializeField] private CircleCollider2D trapCollider;
        
        private float timeRemaining;
        private HashSet<Enemy> trappedEnemies = new HashSet<Enemy>();
        
        private void Awake()
        {
            if (trapCollider == null)
            {
                trapCollider = gameObject.AddComponent<CircleCollider2D>();
                trapCollider.isTrigger = true;
            }
            
            trapCollider.radius = radius;
        }
        
        /// <summary>
        /// Initialize trap with custom parameters
        /// </summary>
        public void Initialize(float trapDuration, float trapRadius)
        {
            duration = trapDuration;
            radius = trapRadius;
            timeRemaining = duration;
            
            if (trapCollider != null)
            {
                trapCollider.radius = trapRadius;
            }
            
            // Scale visual to match radius
            if (trapSprite != null)
            {
                transform.localScale = Vector3.one * (trapRadius * 2f);
            }
            
            // Start particle effects
            if (trapParticles != null && !trapParticles.isPlaying)
            {
                trapParticles.Play();
            }
            
            Debug.Log($"Earth Trap initialized at {transform.position} (Duration: {duration}s, Radius: {radius})");
        }
        
        private void Update()
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0)
            {
                DestroyTrap();
                return;
            }
            
            // Pull and damage trapped enemies
            ProcessTrappedEnemies();
            
            // Update visual fade
            UpdateVisuals();
        }
        
        /// <summary>
        /// Process all enemies in trap range
        /// </summary>
        private void ProcessTrappedEnemies()
        {
            // Remove dead or null enemies
            trappedEnemies.RemoveWhere(e => e == null || !e.IsAlive);
            
            foreach (Enemy enemy in trappedEnemies)
            {
                // Pull enemy toward trap center
                Vector2 pullDirection = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized;
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                
                if (distance > 0.1f)
                {
                    // Apply pull force (stronger when closer)
                    float pullForce = pullStrength * (1f - distance / radius);
                    enemy.transform.position += (Vector3)pullDirection * pullForce * Time.deltaTime;
                }
                
                // Deal damage over time
                enemy.TakeDamage(damagePerSecond * Time.deltaTime, DamageType.Magic);
            }
        }
        
        /// <summary>
        /// Update trap visuals based on remaining time
        /// </summary>
        private void UpdateVisuals()
        {
            if (trapSprite != null)
            {
                // Fade out in last second
                float alpha = timeRemaining < 1f ? timeRemaining : 1f;
                Color color = trapSprite.color;
                color.a = alpha;
                trapSprite.color = color;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                trappedEnemies.Add(enemy);
                Debug.Log($"Enemy {enemy.name} entered Earth Trap");
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                trappedEnemies.Remove(enemy);
                Debug.Log($"Enemy {enemy.name} escaped Earth Trap");
            }
        }
        
        /// <summary>
        /// Destroy trap and cleanup
        /// </summary>
        private void DestroyTrap()
        {
            Debug.Log($"Earth Trap expired at {transform.position}");
            
            // Stop particles
            if (trapParticles != null)
            {
                trapParticles.Stop();
            }
            
            // Destroy after particles finish
            Destroy(gameObject, trapParticles != null ? trapParticles.main.duration : 0f);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw trap radius
            Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
