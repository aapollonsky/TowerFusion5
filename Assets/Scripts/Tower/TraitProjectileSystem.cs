using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Manages independent projectile firing for a single trait
    /// </summary>
    public class TraitProjectileSystem : MonoBehaviour
    {
        private TowerTrait trait;
        private Tower tower;
        private Transform firePoint;
        private float lastFireTime;
        private bool isActive = true;
        
        /// <summary>
        /// Initialize the projectile system for a specific trait
        /// </summary>
        public void Initialize(TowerTrait traitData, Tower parentTower, Transform projectileFirePoint)
        {
            trait = traitData;
            tower = parentTower;
            firePoint = projectileFirePoint != null ? projectileFirePoint : tower.transform;
            lastFireTime = -trait.projectileCooldown; // Allow immediate first shot
            
            Debug.Log($"<color=cyan>TraitProjectileSystem initialized for '{trait.traitName}' on {tower.name}</color>");
            Debug.Log($"  Cooldown: {trait.projectileCooldown}s between shots, Damage: {trait.projectileDamage}");
        }
        
        private void Update()
        {
            if (!isActive || trait == null || tower == null) return;
            
            // Check if it's time to fire
            float timeSinceLastFire = Time.time - lastFireTime;
            
            if (timeSinceLastFire >= trait.projectileCooldown)
            {
                TryFire();
            }
        }
        
        private void TryFire()
        {
            // Get current target from tower
            Enemy target = tower.CurrentTarget;
            
            if (target == null || !target.IsAlive) return;
            
            // Check if target is in range
            float distance = Vector2.Distance(tower.Position, target.Position);
            if (distance > tower.ModifiedRange) return;
            
            // Fire projectile
            FireProjectile(target);
            lastFireTime = Time.time;
        }
        
        private void FireProjectile(Enemy target)
        {
            if (trait.projectilePrefab == null)
            {
                Debug.LogWarning($"Trait '{trait.traitName}' has no projectile prefab assigned!");
                return;
            }
            
            // Notify reactive defense system BEFORE creating projectile (same as Tower.Attack)
            tower.OnTowerFired?.Invoke(tower, target);
            
            // Instantiate projectile
            GameObject projectileObj = Instantiate(
                trait.projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );
            
            // Initialize projectile
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    target,
                    trait.projectileDamage,
                    trait.projectileDamageType,
                    trait.projectileSpeed,
                    tower
                );
                
                // Mark projectile as trait-specific
                projectile.SetTraitProjectile(trait);
                
                Debug.Log($"<color=yellow>[{Time.time:F2}s] {trait.traitName} fired projectile at {target.name} (cooldown={trait.projectileCooldown}s)</color>");
            }
            else
            {
                Debug.LogError($"Projectile prefab for trait '{trait.traitName}' is missing Projectile component!");
                Destroy(projectileObj);
            }
        }
        
        /// <summary>
        /// Enable or disable this projectile system
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
        }
        
        /// <summary>
        /// Get the trait associated with this system
        /// </summary>
        public TowerTrait GetTrait()
        {
            return trait;
        }
        
        private void OnDestroy()
        {
            Debug.Log($"<color=red>TraitProjectileSystem for '{trait?.traitName}' destroyed</color>");
        }
    }
}
