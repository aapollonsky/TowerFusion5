using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TowerFusion
{
    /// <summary>
    /// Component that manages traits applied to a tower
    /// </summary>
    public class TowerTraitManager : MonoBehaviour
    {
        [Header("Current Traits")]
        [SerializeField] private List<TowerTrait> appliedTraits = new List<TowerTrait>();
        
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer baseRenderer;
        [SerializeField] private SpriteRenderer overlayRenderer;
        [SerializeField] private Transform effectsParent;
        
        // Cached components
        private Tower tower;
        private List<ParticleSystem> activeEffects = new List<ParticleSystem>();
        
        // Events
        public System.Action<TowerTrait> OnTraitAdded;
        public System.Action<TowerTrait> OnTraitRemoved;
        public System.Action OnTraitsChanged;
        
        // Properties
        public List<TowerTrait> AppliedTraits => appliedTraits.ToList(); // Return copy to prevent external modification
        public int TraitCount => appliedTraits.Count;
        public bool HasTraits => appliedTraits.Count > 0;
        
        private void Awake()
        {
            tower = GetComponent<Tower>();
            
            if (baseRenderer == null)
                baseRenderer = GetComponent<SpriteRenderer>();
                
            SetupOverlayRenderer();
            SetupEffectsParent();
        }
        
        private void Start()
        {
            // Apply any pre-configured traits
            if (appliedTraits.Count > 0)
            {
                RefreshAllVisuals();
                NotifyTowerOfTraitChanges();
            }
        }
        
        /// <summary>
        /// Add a trait to this tower
        /// </summary>
        public bool AddTrait(TowerTrait trait)
        {
            if (trait == null)
            {
                Debug.LogWarning("Cannot add null trait to tower");
                return false;
            }
            
            if (appliedTraits.Contains(trait))
            {
                Debug.LogWarning($"Tower already has trait: {trait.traitName}");
                return false;
            }
            
            appliedTraits.Add(trait);
            ApplyTraitVisuals(trait);
            NotifyTowerOfTraitChanges();
            
            OnTraitAdded?.Invoke(trait);
            OnTraitsChanged?.Invoke();
            
            Debug.Log($"Added trait '{trait.traitName}' to tower at {transform.position}");
            return true;
        }
        
        /// <summary>
        /// Remove a trait from this tower
        /// </summary>
        public bool RemoveTrait(TowerTrait trait)
        {
            if (trait == null || !appliedTraits.Contains(trait))
                return false;
            
            appliedTraits.Remove(trait);
            RefreshAllVisuals();
            NotifyTowerOfTraitChanges();
            
            OnTraitRemoved?.Invoke(trait);
            OnTraitsChanged?.Invoke();
            
            Debug.Log($"Removed trait '{trait.traitName}' from tower at {transform.position}");
            return true;
        }
        
        /// <summary>
        /// Remove all traits from this tower
        /// </summary>
        public void ClearAllTraits()
        {
            if (appliedTraits.Count == 0) return;
            
            appliedTraits.Clear();
            RefreshAllVisuals();
            NotifyTowerOfTraitChanges();
            
            OnTraitsChanged?.Invoke();
            Debug.Log($"Cleared all traits from tower at {transform.position}");
        }
        
        /// <summary>
        /// Check if tower has a specific trait
        /// </summary>
        public bool HasTrait(TowerTrait trait)
        {
            return appliedTraits.Contains(trait);
        }
        
        /// <summary>
        /// Check if tower has any trait of a specific category
        /// </summary>
        public bool HasTraitOfCategory(TraitCategory category)
        {
            return appliedTraits.Any(trait => trait.category == category);
        }
        
        /// <summary>
        /// Get all traits of a specific category
        /// </summary>
        public List<TowerTrait> GetTraitsOfCategory(TraitCategory category)
        {
            return appliedTraits.Where(trait => trait.category == category).ToList();
        }
        
        /// <summary>
        /// Calculate combined stats from all traits
        /// </summary>
        public TowerStats CalculateModifiedStats(TowerStats baseStats)
        {
            TowerStats modifiedStats = baseStats;
            
            foreach (var trait in appliedTraits)
            {
                modifiedStats = trait.ApplyToStats(modifiedStats);
            }
            
            return modifiedStats;
        }
        
        /// <summary>
        /// Get combined visual effects from all traits
        /// </summary>
        public Color GetCombinedOverlayColor()
        {
            if (appliedTraits.Count == 0)
                return Color.clear;
            
            Color combined = Color.clear;
            float totalAlpha = 0f;
            
            foreach (var trait in appliedTraits)
            {
                Color traitColor = trait.overlayColor;
                traitColor.a = trait.overlayAlpha;
                
                if (combined == Color.clear)
                {
                    combined = traitColor;
                }
                else
                {
                    // Blend colors additively for better visibility
                    combined = Color.Lerp(combined, traitColor, 0.6f);
                }
                
                totalAlpha += trait.overlayAlpha;
            }
            
            // Ensure minimum visibility and cap maximum
            combined.a = Mathf.Clamp(totalAlpha, 0.3f, 0.9f);
            
            Debug.Log($"Combined overlay color for {name}: {combined} (from {appliedTraits.Count} traits)");
            return combined;
        }
        
        #region Visual Management
        
        private void SetupOverlayRenderer()
        {
            if (overlayRenderer == null)
            {
                GameObject overlayObj = new GameObject("TraitOverlay");
                overlayObj.transform.SetParent(transform);
                overlayObj.transform.localPosition = Vector3.zero;
                overlayObj.transform.localScale = Vector3.one;
                
                overlayRenderer = overlayObj.AddComponent<SpriteRenderer>();
                
                // Ensure we have a base renderer reference
                if (baseRenderer == null)
                    baseRenderer = GetComponent<SpriteRenderer>();
                
                if (baseRenderer != null)
                {
                    overlayRenderer.sortingOrder = baseRenderer.sortingOrder + 1;
                    // Copy the main sprite to the overlay initially
                    overlayRenderer.sprite = baseRenderer.sprite;
                    Debug.Log($"Setup trait overlay for {name}: base sorting order {baseRenderer.sortingOrder}, overlay {overlayRenderer.sortingOrder}");
                }
                else
                {
                    overlayRenderer.sortingOrder = 1; // Default sorting order
                    Debug.LogWarning($"No base SpriteRenderer found for {name} - overlay may not display correctly");
                }
                
                overlayRenderer.color = Color.clear;
            }
        }
        
        private void SetupEffectsParent()
        {
            if (effectsParent == null)
            {
                GameObject effectsObj = new GameObject("TraitEffects");
                effectsObj.transform.SetParent(transform);
                effectsObj.transform.localPosition = Vector3.zero;
                effectsParent = effectsObj.transform;
            }
        }
        
        /// <summary>
        /// Create a simple glow effect for trait visualization
        /// </summary>
        private void CreateTraitGlow(Color glowColor)
        {
            GameObject glowObj = new GameObject("TraitGlow");
            glowObj.transform.SetParent(effectsParent);
            glowObj.transform.localPosition = Vector3.zero;
            glowObj.transform.localScale = Vector3.one * 1.3f; // Slightly larger than base
            
            SpriteRenderer glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            
            // Use base sprite for glow shape
            if (baseRenderer != null && baseRenderer.sprite != null)
            {
                glowRenderer.sprite = baseRenderer.sprite;
            }
            
            // Set glow color and properties
            glowColor.a = 0.4f;
            glowRenderer.color = glowColor;
            glowRenderer.sortingOrder = baseRenderer.sortingOrder - 1; // Behind base sprite
            
            // Add pulsing animation
            StartCoroutine(PulseGlow(glowRenderer));
            
            Debug.Log($"Created trait glow effect: {glowColor}");
        }
        
        /// <summary>
        /// Animate the glow effect
        /// </summary>
        private System.Collections.IEnumerator PulseGlow(SpriteRenderer glowRenderer)
        {
            float baseAlpha = glowRenderer.color.a;
            Color baseColor = glowRenderer.color;
            
            while (glowRenderer != null && appliedTraits.Count > 0)
            {
                float pulse = Mathf.Sin(Time.time * 2f) * 0.2f + 1f; // Pulse between 0.8 and 1.2
                Color newColor = baseColor;
                newColor.a = baseAlpha * pulse;
                glowRenderer.color = newColor;
                
                Vector3 scale = Vector3.one * (1.2f + Mathf.Sin(Time.time * 1.5f) * 0.1f);
                glowRenderer.transform.localScale = scale;
                
                yield return null;
            }
        }
        
        private void ApplyTraitVisuals(TowerTrait trait)
        {
            // Update overlay color
            Color newOverlayColor = GetCombinedOverlayColor();
            overlayRenderer.color = newOverlayColor;
            
            Debug.Log($"Applied visual for trait '{trait.traitName}' on {name}:");
            Debug.Log($"  - Trait color: {trait.overlayColor} (alpha: {trait.overlayAlpha})");
            Debug.Log($"  - Final overlay color: {newOverlayColor}");
            Debug.Log($"  - Overlay renderer active: {overlayRenderer.gameObject.activeInHierarchy}");
            
            // Apply overlay sprite if specified and we don't have one yet
            if (trait.overlaySprite != null && overlayRenderer.sprite == null)
            {
                overlayRenderer.sprite = trait.overlaySprite;
                Debug.Log($"  - Applied overlay sprite: {trait.overlaySprite.name}");
            }
            else if (trait.overlaySprite == null && overlayRenderer.sprite == null)
            {
                // Use the base sprite for color overlay if no specific overlay sprite
                if (baseRenderer != null && baseRenderer.sprite != null)
                {
                    overlayRenderer.sprite = baseRenderer.sprite;
                    Debug.Log($"  - Using base sprite for color overlay: {baseRenderer.sprite.name}");
                }
            }
            
            // Add particle effects
            if (trait.effectPrefab != null)
            {
                GameObject effectInstance = Instantiate(trait.effectPrefab.gameObject, effectsParent);
                ParticleSystem particles = effectInstance.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    activeEffects.Add(particles);
                    particles.Play();
                    Debug.Log($"  - Added particle effect: {trait.effectPrefab.name}");
                }
            }
            
            // Create glow effect for first trait (to avoid multiple glows)
            if (appliedTraits.Count == 1)
            {
                CreateTraitGlow(trait.overlayColor);
            }
        }
        
        private void RefreshAllVisuals()
        {
            // Clear existing effects
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                    DestroyImmediate(effect.gameObject);
            }
            activeEffects.Clear();
            
            // Reset overlay
            if (overlayRenderer != null)
            {
                overlayRenderer.color = Color.clear;
                overlayRenderer.sprite = null;
            }
            
            // Reset base renderer to original color
            if (baseRenderer != null)
            {
                baseRenderer.color = Color.white;
            }
            
            // Reapply all trait visuals
            foreach (var trait in appliedTraits)
            {
                ApplyTraitVisuals(trait);
            }
            
            // Also apply trait color tint to base renderer for better visibility
            if (appliedTraits.Count > 0 && baseRenderer != null)
            {
                Color combinedColor = GetCombinedOverlayColor();
                Color baseColorTint = Color.Lerp(Color.white, combinedColor, 0.3f);
                baseRenderer.color = baseColorTint;
                Debug.Log($"Applied base color tint to {name}: {baseColorTint}");
            }
        }
        
        private void NotifyTowerOfTraitChanges()
        {
            if (tower != null)
            {
                // Tell the tower to recalculate its stats
                tower.OnTraitsChanged();
            }
        }
        
        #endregion
        
        #region Combat Integration
        
        /// <summary>
        /// Apply trait effects when tower attacks an enemy
        /// </summary>
        public void ApplyTraitEffectsOnAttack(Enemy target, float damage)
        {
            if (target == null) return;
            
            foreach (var trait in appliedTraits)
            {
                ApplyTraitEffect(trait, target, damage);
            }
        }
        
        /// <summary>
        /// Apply trait effects when tower kills an enemy
        /// </summary>
        public void ApplyTraitEffectsOnKill(Enemy target)
        {
            if (target == null) return;
            
            foreach (var trait in appliedTraits)
            {
                if (trait.hasGoldReward)
                {
                    GameManager.Instance?.AddGold(trait.goldPerKill);
                    Debug.Log($"Harvest trait: +{trait.goldPerKill} gold from kill");
                }
            }
        }
        
        private void ApplyTraitEffect(TowerTrait trait, Enemy target, float damage)
        {
            // Apply burn effect
            if (trait.hasBurnEffect)
            {
                target.ApplyBurnEffect(trait.burnDamagePerSecond, trait.burnDuration);
                Debug.Log($"Trait '{trait.traitName}': Applied burn effect to {target.name}");
            }
            
            // Apply slow effect
            if (trait.hasSlowEffect)
            {
                target.ApplySlowEffect(trait.slowMultiplier, trait.slowDuration);
                Debug.Log($"Trait '{trait.traitName}': Applied slow effect to {target.name} ({trait.slowMultiplier}x speed)");
            }
            
            // Apply brittle effect
            if (trait.hasBrittleEffect)
            {
                target.ApplyBrittleEffect(trait.brittleDamageMultiplier, trait.brittleDuration);
                Debug.Log($"Trait '{trait.traitName}': Applied brittle effect to {target.name} ({trait.brittleDamageMultiplier}x damage taken)");
            }
            
            // Apply chain effect
            if (trait.hasChainEffect)
            {
                ApplyChainEffect(trait, target, damage);
            }
        }
        
        private void ApplyChainEffect(TowerTrait trait, Enemy primaryTarget, float damage)
        {
            // Find nearby enemies for chaining
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
                primaryTarget.transform.position, 
                trait.chainRange, 
                LayerMask.GetMask("Enemy")
            );
            
            List<Enemy> chainTargets = new List<Enemy>();
            foreach (var collider in nearbyColliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && enemy != primaryTarget)
                {
                    chainTargets.Add(enemy);
                }
            }
            
            // Sort by distance and take closest targets
            chainTargets = chainTargets
                .OrderBy(e => Vector3.Distance(primaryTarget.transform.position, e.transform.position))
                .Take(trait.chainTargets)
                .ToList();
            
            // Apply chain damage
            float chainDamage = damage * trait.chainDamageMultiplier;
            foreach (var chainTarget in chainTargets)
            {
                chainTarget.TakeDamage(chainDamage, DamageType.Magic); // Chain lightning is magical damage
                
                // Visual effect for chain
                Debug.DrawLine(primaryTarget.transform.position, chainTarget.transform.position, Color.yellow, 0.5f);
                Debug.Log($"Chain lightning: {chainDamage} damage to {chainTarget.name}");
            }
        }
        
        #endregion
    }
}