using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace TowerFusion
{
    /// <summary>
    /// Manages tower placement and tower-related functionality
    /// </summary>
    public class TowerManager : MonoBehaviour
    {
        [Header("Tower Configuration")]
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private Transform towerContainer;
        
        [Header("Placement Settings")]
        [SerializeField] private LayerMask groundLayerMask = 1;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        
        // Singleton instance
        public static TowerManager Instance { get; private set; }
        
        // Tower management
        private List<Tower> activeTowers = new List<Tower>();
        private Tower selectedTower;
        
        // Placement state
        private TowerData towerToBuild;
        private GameObject placementPreview;
        private Camera mainCamera;
        
        // Events
        public System.Action<Tower> OnTowerPlaced;
        public System.Action<Tower> OnTowerSelected;
        public System.Action<Tower> OnTowerUpgraded;
        public System.Action<Tower> OnTowerSold;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                mainCamera = Camera.main;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            HandleInput();
            UpdatePlacementPreview();
        }
        
        /// <summary>
        /// Handle input for tower placement and selection
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                // Don't process clicks on UI elements
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                
                if (towerToBuild != null)
                {
                    TryPlaceTower(mouseWorldPos);
                }
                else
                {
                    SelectTowerAt(mouseWorldPos);
                }
            }
            
            if (Input.GetMouseButtonDown(1)) // Right click
            {
                CancelPlacement();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
                DeselectTower();
            }
        }
        
        /// <summary>
        /// Update placement preview visual
        /// </summary>
        private void UpdatePlacementPreview()
        {
            if (towerToBuild == null || placementPreview == null)
                return;
            
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 snapPosition = GetValidPlacementPosition(mouseWorldPos);
            
            placementPreview.transform.position = snapPosition;
            
            // Update preview material based on validity
            bool canPlace = CanPlaceTowerAt(snapPosition);
            Renderer previewRenderer = placementPreview.GetComponent<Renderer>();
            
            if (previewRenderer != null)
            {
                previewRenderer.material = canPlace ? validPlacementMaterial : invalidPlacementMaterial;
            }
        }
        
        /// <summary>
        /// Get mouse position in world coordinates
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            if (mainCamera == null)
                return Vector3.zero;
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Distance from camera
            return mainCamera.ScreenToWorldPoint(mousePos);
        }
        
        /// <summary>
        /// Start tower placement mode
        /// </summary>
        public void StartPlacingTower(TowerData towerData)
        {
            if (towerData == null)
            {
                Debug.LogWarning("StartPlacingTower: towerData is null");
                return;
            }
            
            Debug.Log($"StartPlacingTower called for {towerData.towerName}. Current towers: {activeTowers.Count}");
            
            // Check if player has enough gold
            if (!GameManager.Instance || GameManager.Instance.CurrentGold < towerData.buildCost)
            {
                Debug.Log("Not enough gold to build this tower!");
                return;
            }
            
            towerToBuild = towerData;
            CreatePlacementPreview();
            
            Debug.Log($"Started placing tower: {towerData.towerName}");
        }
        
        /// <summary>
        /// Create visual preview for tower placement
        /// </summary>
        private void CreatePlacementPreview()
        {
            if (towerPrefab == null || towerToBuild == null)
                return;
            
            // Destroy existing preview
            if (placementPreview != null)
            {
                Destroy(placementPreview);
            }
            
            // Create new preview
            placementPreview = Instantiate(towerPrefab);
            
            // Setup preview appearance
            Tower previewTower = placementPreview.GetComponent<Tower>();
            if (previewTower != null)
            {
                previewTower.Initialize(towerToBuild);
                previewTower.SetRangeIndicatorVisible(true);
                previewTower.enabled = false; // Disable tower behavior
            }
            
            // Make it semi-transparent
            SpriteRenderer spriteRenderer = placementPreview.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.7f;
                spriteRenderer.color = color;
            }
            
            // Disable colliders
            Collider2D[] colliders = placementPreview.GetComponents<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }
        }
        
        /// <summary>
        /// Try to place tower at position
        /// </summary>
        private void TryPlaceTower(Vector3 position)
        {
            Vector3 placementPosition = GetValidPlacementPosition(position);
            
            Debug.Log($"TryPlaceTower: Attempting to place tower at {placementPosition}. Current tower count: {activeTowers.Count}");
            
            if (!CanPlaceTowerAt(placementPosition))
            {
                Debug.Log("Cannot place tower at this location!");
                return;
            }
            
            if (!GameManager.Instance.SpendGold(towerToBuild.buildCost))
            {
                Debug.Log("Not enough gold to build tower!");
                return;
            }
            
            // Create the actual tower
            GameObject towerObj = Instantiate(towerPrefab, placementPosition, Quaternion.identity, towerContainer);
            Tower tower = towerObj.GetComponent<Tower>();
            
            if (tower != null)
            {
                tower.Initialize(towerToBuild);
                RegisterTower(tower);
                OnTowerPlaced?.Invoke(tower);
                
                Debug.Log($"Tower placed: {towerToBuild.towerName} at {placementPosition}. New tower count: {activeTowers.Count}");
            }
            
            // End placement mode
            CancelPlacement();
        }
        
        /// <summary>
        /// Cancel tower placement
        /// </summary>
        public void CancelPlacement()
        {
            towerToBuild = null;
            
            if (placementPreview != null)
            {
                Destroy(placementPreview);
                placementPreview = null;
            }
        }
        
        /// <summary>
        /// Check if tower can be placed at position
        /// </summary>
        private bool CanPlaceTowerAt(Vector3 position)
        {
            // Check if MapManager exists and whether the map allows placement at this position
            if (MapManager.Instance == null)
            {
                Debug.LogWarning("CanPlaceTowerAt: MapManager.Instance is null");
                return false;
            }

            // Check if we've reached the maximum number of towers
            var map = MapManager.Instance.CurrentMap;
            if (map != null && activeTowers.Count >= map.maxTowers)
            {
                Debug.Log($"Cannot place tower: Maximum towers reached ({activeTowers.Count}/{map.maxTowers})");
                return false;
            }

            bool mapAllows = MapManager.Instance.CanPlaceTowerAt(position);
            if (!mapAllows)
            {
                // Helpful debug: show nearest allowed position and count of configured positions
                if (map != null)
                {
                    int configured = map.towerPositions != null ? map.towerPositions.Count : 0;
                    Vector3 nearest = map.GetClosestTowerPosition(position);
                    Debug.Log($"Cannot place tower: map disallows at {position}. Nearest allowed position: {nearest}. Configured positions: {configured}");
                }
                else
                {
                    Debug.Log("Cannot place tower: CurrentMap is null on MapManager");
                }

                return false;
            }
            
            // Check if there's already a tower at this position
            foreach (Tower tower in activeTowers)
            {
                if (Vector3.Distance(tower.Position, position) < 0.5f)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get valid placement position (snapped to grid)
        /// </summary>
        private Vector3 GetValidPlacementPosition(Vector3 rawPosition)
        {
            // First, snap to grid if GridManager exists
            if (GridManager.Instance != null && GridManager.Instance.IsInitialized)
            {
                return GridManager.Instance.SnapToGrid(rawPosition);
            }
            
            // Fallback to MapManager's tower positions if available
            if (MapManager.Instance != null)
            {
                return MapManager.Instance.GetClosestTowerPosition(rawPosition);
            }
            
            // Last resort: snap to simple grid
            float gridSize = 0.5f;
            float snappedX = Mathf.Round(rawPosition.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(rawPosition.y / gridSize) * gridSize;
            
            return new Vector3(snappedX, snappedY, 0f);
        }
        
        /// <summary>
        /// Register a new tower
        /// </summary>
        private void RegisterTower(Tower tower)
        {
            if (tower == null || activeTowers.Contains(tower))
                return;
            
            activeTowers.Add(tower);
            
            // Subscribe to tower events
            tower.OnTowerUpgraded += OnTowerUpgradedHandler;
            tower.OnTowerDestroyed += OnTowerDestroyedHandler;
        }
        
        /// <summary>
        /// Unregister a tower
        /// </summary>
        private void UnregisterTower(Tower tower)
        {
            if (tower == null)
                return;
            
            activeTowers.Remove(tower);
            
            // Unsubscribe from events
            tower.OnTowerUpgraded -= OnTowerUpgradedHandler;
            tower.OnTowerDestroyed -= OnTowerDestroyedHandler;
            
            if (selectedTower == tower)
            {
                selectedTower = null;
            }
        }
        
        /// <summary>
        /// Handle tower destroyed event
        /// </summary>
        private void OnTowerDestroyedHandler(Tower tower)
        {
            Debug.Log($"Tower {tower.TowerData.towerName} was destroyed by enemies!");
            UnregisterTower(tower);
            OnTowerSold?.Invoke(tower); // Reuse the sold event for UI updates
        }
        
        /// <summary>
        /// Select tower at position
        /// </summary>
        private void SelectTowerAt(Vector3 position)
        {
            Tower clickedTower = null;
            float closestDistance = 0.5f; // Selection radius
            
            foreach (Tower tower in activeTowers)
            {
                float distance = Vector3.Distance(tower.Position, position);
                if (distance <= closestDistance)
                {
                    clickedTower = tower;
                    closestDistance = distance;
                }
            }
            
            SelectTower(clickedTower);
        }
        
        /// <summary>
        /// Select a specific tower
        /// </summary>
        public void SelectTower(Tower tower)
        {
            // Deselect previous tower
            if (selectedTower != null)
            {
                selectedTower.SetRangeIndicatorVisible(false);
            }
            
            selectedTower = tower;
            
            if (selectedTower != null)
            {
                selectedTower.SetRangeIndicatorVisible(true);
            }
            
            // Always invoke the event, whether selecting or deselecting
            OnTowerSelected?.Invoke(selectedTower);
        }
        
        /// <summary>
        /// Deselect current tower
        /// </summary>
        public void DeselectTower()
        {
            SelectTower(null);
        }
        
        /// <summary>
        /// Upgrade selected tower
        /// </summary>
        public bool UpgradeSelectedTower()
        {
            if (selectedTower == null)
                return false;
            
            return selectedTower.TryUpgrade();
        }
        
        /// <summary>
        /// Sell selected tower
        /// </summary>
        public bool SellSelectedTower()
        {
            if (selectedTower == null)
                return false;
            
            // Calculate sell price (usually 75% of build cost + upgrades)
            int sellPrice = Mathf.RoundToInt(selectedTower.TowerData.buildCost * 0.75f);
            
            GameManager.Instance?.AddGold(sellPrice);
            
            Tower towerToRemove = selectedTower;
            DeselectTower();
            
            UnregisterTower(towerToRemove);
            OnTowerSold?.Invoke(towerToRemove);
            
            Destroy(towerToRemove.gameObject);
            
            Debug.Log($"Tower sold for {sellPrice} gold");
            return true;
        }
        
        /// <summary>
        /// Handle tower upgraded event
        /// </summary>
        private void OnTowerUpgradedHandler(Tower tower)
        {
            OnTowerUpgraded?.Invoke(tower);
        }
        
        /// <summary>
        /// Get all active towers
        /// </summary>
        public List<Tower> GetActiveTowers()
        {
            return new List<Tower>(activeTowers);
        }
        
        /// <summary>
        /// Get selected tower
        /// </summary>
        public Tower GetSelectedTower()
        {
            return selectedTower;
        }
        
        /// <summary>
        /// Get tower count
        /// </summary>
        public int GetTowerCount()
        {
            return activeTowers.Count;
        }
    }
}