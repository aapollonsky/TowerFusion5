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
            float moveDistance = enemyData.moveSpeed * Time.deltaTime;
            
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
            float distance = enemyData.moveSpeed * timeAhead;
            
            return transform.position + direction * distance;
        }
    }
}