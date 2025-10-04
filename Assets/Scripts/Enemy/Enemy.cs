using UnityEngine;
using System.Collections;

namespace TowerFusion
{
    /// <summary>
    /// Individual enemy behavior controller
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb2D;
        
        [Header("Health Bar")]
        [SerializeField] private Transform healthBarContainer;
        [SerializeField] private Transform healthBarFill;
        
        // Current state
        private float currentHealth;
        private int currentPathIndex = 0;
        private bool isAlive = true;
        private bool hasReachedEnd = false;
        
        // Status effects
        private float speedMultiplier = 1f;
        private float damageMultiplier = 1f;
        private bool isSlowed = false;
        private bool isBrittle = false;
        private Color originalColor = Color.white;
        private bool originalColorStored = false;
        
        // Movement
        private Vector3 targetPosition;
        private Vector3[] pathPoints;
        
        // Events
        public System.Action<Enemy> OnEnemyKilled;
        public System.Action<Enemy> OnEnemyReachedEnd;
        
        // Properties
        public EnemyData EnemyData => enemyData;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => enemyData?.maxHealth ?? 100f;
        public bool IsAlive => isAlive;
        public Vector3 Position => transform.position;
        public float CurrentSpeed => enemyData.moveSpeed * speedMultiplier;
        public bool IsSlowed => isSlowed;
        public bool IsBrittle => isBrittle;
        
        private void Awake()
        {
            if (rb2D == null)
                rb2D = GetComponent<Rigidbody2D>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        /// <summary>
        /// Initialize enemy with data
        /// </summary>
        public void Initialize(EnemyData data)
        {
            enemyData = data;
            currentHealth = enemyData.maxHealth;
            isAlive = true;
            hasReachedEnd = false;
            currentPathIndex = 0;
            
            // Store original color for status effect restoration
            if (spriteRenderer != null && !originalColorStored)
            {
                originalColor = spriteRenderer.color;
                originalColorStored = true;
                Debug.Log($"Stored original color for {name}: {originalColor}");
            }
            
            SetupVisuals();
            SetupMovement();
            
            if (enemyData.isRegenerating && enemyData.regenerationRate > 0)
            {
                StartCoroutine(RegenerateHealth());
            }
        }
        
        /// <summary>
        /// Setup visual appearance
        /// </summary>
        private void SetupVisuals()
        {
            if (spriteRenderer != null && enemyData.enemySprite != null)
            {
                spriteRenderer.sprite = enemyData.enemySprite;
                spriteRenderer.color = enemyData.enemyColor;
            }
            
            transform.localScale = enemyData.scale;
            
            // Setup health bar
            UpdateHealthBar();
        }
        
        /// <summary>
        /// Setup movement path
        /// </summary>
        private void SetupMovement()
        {
            pathPoints = MapManager.Instance?.GetPathPoints();
            
            if (pathPoints != null && pathPoints.Length > 0)
            {
                transform.position = MapManager.Instance.GetEnemySpawnPoint();
                targetPosition = pathPoints[0];
            }
        }
        
        private void Update()
        {
            if (!isAlive || hasReachedEnd)
                return;
            
            MoveAlongPath();
        }
        
        /// <summary>
        /// Move enemy along the path
        /// </summary>
        private void MoveAlongPath()
        {
            if (pathPoints == null || pathPoints.Length == 0)
                return;
            
            // Move towards target
            Vector3 direction = (targetPosition - transform.position).normalized;
            float currentSpeed = enemyData.moveSpeed * speedMultiplier;
            float moveDistance = currentSpeed * Time.deltaTime;
            
            transform.position += direction * moveDistance;
            
            // Check if reached current target
            if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
            {
                currentPathIndex++;
                
                if (currentPathIndex >= pathPoints.Length)
                {
                    // Reached the end
                    ReachEnd();
                }
                else
                {
                    targetPosition = pathPoints[currentPathIndex];
                }
            }
        }
        
        /// <summary>
        /// Take damage from towers
        /// </summary>
        public void TakeDamage(float damage, DamageType damageType)
        {
            if (!isAlive)
                return;
            
            float actualDamage = enemyData.CalculateDamage(damage, damageType);
            
            // Apply brittle effect (increased damage taken)
            if (isBrittle)
            {
                actualDamage *= damageMultiplier;
            }
            
            currentHealth -= actualDamage;
            
            // Update health bar
            UpdateHealthBar();
            
            // Create damage number effect
            CreateDamageNumber(actualDamage);
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Enemy dies and gives rewards
        /// </summary>
        private void Die()
        {
            if (!isAlive)
                return;
            
            isAlive = false;
            
            // Give gold reward
            GameManager.Instance?.AddGold(enemyData.goldReward);
            
            // Notify systems
            OnEnemyKilled?.Invoke(this);
            EnemyManager.Instance?.OnEnemyKilled(this);
            
            // Create death effect
            CreateDeathEffect();
            
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Enemy reaches the end and damages player
        /// </summary>
        private void ReachEnd()
        {
            if (hasReachedEnd)
                return;
            
            hasReachedEnd = true;
            
            // Damage player
            GameManager.Instance?.ModifyHealth(-enemyData.damageToPlayer);
            
            // Notify systems
            OnEnemyReachedEnd?.Invoke(this);
            EnemyManager.Instance?.OnEnemyReachedEnd(this);
            
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Update health bar visual
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBarFill != null)
            {
                float healthPercent = currentHealth / MaxHealth;
                healthBarFill.localScale = new Vector3(healthPercent, 1f, 1f);
            }
            
            if (healthBarContainer != null)
            {
                healthBarContainer.gameObject.SetActive(currentHealth < MaxHealth);
            }
        }
        
        /// <summary>
        /// Create floating damage number
        /// </summary>
        private void CreateDamageNumber(float damage)
        {
            // TODO: Implement damage number effect
            Debug.Log($"{enemyData.enemyName} took {damage:F1} damage");
        }
        
        /// <summary>
        /// Create death effect
        /// </summary>
        private void CreateDeathEffect()
        {
            // TODO: Implement death particle effect
            Debug.Log($"{enemyData.enemyName} was killed!");
        }
        
        /// <summary>
        /// Regenerate health over time
        /// </summary>
        private IEnumerator RegenerateHealth()
        {
            while (isAlive)
            {
                yield return new WaitForSeconds(1f);
                
                if (currentHealth < MaxHealth)
                {
                    currentHealth += enemyData.regenerationRate;
                    currentHealth = Mathf.Min(currentHealth, MaxHealth);
                    UpdateHealthBar();
                }
            }
        }
        
        /// <summary>
        /// Get predicted position after time
        /// </summary>
        public Vector3 GetPredictedPosition(float timeAhead)
        {
            if (!isAlive || pathPoints == null)
                return transform.position;
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            float currentSpeed = enemyData.moveSpeed * speedMultiplier;
            float distance = currentSpeed * timeAhead;
            
            return transform.position + direction * distance;
        }
        
        /// <summary>
        /// Update sprite color based on current status effects
        /// </summary>
        private void UpdateStatusEffectColor()
        {
            if (spriteRenderer == null) return;
            
            Color targetColor = originalColor;
            
            // Apply status effect colors
            if (isSlowed && isBrittle)
            {
                // Mix cyan (slow) and white (brittle) - results in light cyan-white
                targetColor = Color.Lerp(originalColor, Color.cyan, 0.3f);
                targetColor = Color.Lerp(targetColor, Color.white, 0.2f);
            }
            else if (isSlowed)
            {
                // Cyan tint for slow
                targetColor = Color.Lerp(originalColor, Color.cyan, 0.4f);
            }
            else if (isBrittle)
            {
                // White tint for brittle
                targetColor = Color.Lerp(originalColor, Color.white, 0.5f);
            }
            
            spriteRenderer.color = targetColor;
            Debug.Log($"{name} color updated: Slowed={isSlowed}, Brittle={isBrittle}, Color={targetColor}");
        }
        
        #region Trait Effects
        
        /// <summary>
        /// Apply burn effect to this enemy
        /// </summary>
        public void ApplyBurnEffect(float damagePerSecond, float duration)
        {
            if (!isAlive) return;
            
            StartCoroutine(BurnCoroutine(damagePerSecond, duration));
            Debug.Log($"Applied burn effect: {damagePerSecond} DPS for {duration}s");
        }
        
        /// <summary>
        /// Apply slow effect to this enemy
        /// </summary>
        public void ApplySlowEffect(float speedMultiplier, float duration)
        {
            if (!isAlive) return;
            
            StartCoroutine(SlowCoroutine(speedMultiplier, duration));
            Debug.Log($"Applied slow effect: {speedMultiplier}x speed for {duration}s");
        }
        
        /// <summary>
        /// Apply brittle effect to this enemy (increases incoming damage)
        /// </summary>
        public void ApplyBrittleEffect(float damageMultiplier, float duration)
        {
            if (!isAlive) return;
            
            StartCoroutine(BrittleCoroutine(damageMultiplier, duration));
            Debug.Log($"Applied brittle effect: {damageMultiplier}x damage taken for {duration}s");
        }
        
        private System.Collections.IEnumerator BurnCoroutine(float damagePerSecond, float duration)
        {
            float elapsed = 0f;
            float damageInterval = 0.5f; // Apply damage every 0.5 seconds
            float nextDamageTime = 0f;
            
            while (elapsed < duration && isAlive)
            {
                elapsed += Time.deltaTime;
                
                if (elapsed >= nextDamageTime)
                {
                    float damage = damagePerSecond * damageInterval;
                    currentHealth -= damage;
                    
                    // Create damage number
                    CreateDamageNumber(damage);
                    
                    if (currentHealth <= 0)
                    {
                        Die();
                        yield break;
                    }
                    
                    UpdateHealthBar();
                    nextDamageTime += damageInterval;
                }
                
                yield return null;
            }
        }
        
        private System.Collections.IEnumerator SlowCoroutine(float newSpeedMultiplier, float duration)
        {
            if (isSlowed)
            {
                // If already slowed, just update the multiplier if it's more severe
                if (newSpeedMultiplier < speedMultiplier)
                {
                    speedMultiplier = newSpeedMultiplier;
                }
                Debug.Log($"Enemy already slowed, existing effect continues");
                yield break;
            }
            
            // Apply slow effect
            isSlowed = true;
            float originalSpeedMultiplier = speedMultiplier;
            speedMultiplier = newSpeedMultiplier;
            
            // Update visual indicator
            UpdateStatusEffectColor();
            
            Debug.Log($"Enemy slowed: {speedMultiplier}x speed for {duration}s");
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // Remove slow effect
            speedMultiplier = originalSpeedMultiplier;
            isSlowed = false;
            
            // Update color based on remaining status effects
            UpdateStatusEffectColor();
            
            Debug.Log("Enemy slow effect ended");
        }
        
        private System.Collections.IEnumerator BrittleCoroutine(float newDamageMultiplier, float duration)
        {
            if (isBrittle)
            {
                // If already brittle, use the higher multiplier
                if (newDamageMultiplier > damageMultiplier)
                {
                    damageMultiplier = newDamageMultiplier;
                }
                Debug.Log($"Enemy already brittle, existing effect continues");
                yield break;
            }
            
            // Apply brittle effect
            isBrittle = true;
            float originalDamageMultiplier = damageMultiplier;
            damageMultiplier = newDamageMultiplier;
            
            // Update visual indicator
            UpdateStatusEffectColor();
            
            Debug.Log($"Enemy brittle: {damageMultiplier}x damage taken for {duration}s");
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // Remove brittle effect
            damageMultiplier = originalDamageMultiplier;
            isBrittle = false;
            
            // Update color based on remaining status effects
            UpdateStatusEffectColor();
            
            Debug.Log("Enemy brittle effect ended");
        }
        
        #endregion
    }
}