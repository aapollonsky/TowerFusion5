using UnityEngine;
using System;

namespace TowerFusion
{
    /// <summary>
    /// Manages corn theft mechanics and tracks game state related to corn
    /// </summary>
    public class CornManager : MonoBehaviour
    {
        public static CornManager Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private CornStorage cornStorage;
        
        private int totalCornStolen = 0; // Successfully returned to spawn
        
        // Properties
        public CornStorage Storage => cornStorage;
        public int TotalCornStolen => totalCornStolen;
        public int RemainingCorn => cornStorage != null ? cornStorage.CornCount : 0;
        public int InitialCornCount => cornStorage != null ? cornStorage.InitialCornCount : 0;
        
        // Events
        public event Action<Enemy> OnCornGrabbed;           // Enemy grabbed corn
        public event Action<Enemy> OnCornSuccessfullyStolen; // Enemy reached spawn with corn
        public event Action OnGameLostToCorn;                // All corn stolen
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Find corn storage if not assigned
            if (cornStorage == null)
            {
                cornStorage = FindObjectOfType<CornStorage>();
                if (cornStorage == null)
                {
                    Debug.LogError("CornManager: No CornStorage found in scene!");
                }
            }
        }
        
        private void Start()
        {
            if (cornStorage != null)
            {
                // Subscribe to storage events
                cornStorage.OnCornTaken += HandleCornTaken;
                cornStorage.OnCornReturned += HandleCornReturned;
                cornStorage.OnAllCornStolen += HandleAllCornTaken;
            }
        }
        
        private void OnDestroy()
        {
            if (cornStorage != null)
            {
                cornStorage.OnCornTaken -= HandleCornTaken;
                cornStorage.OnCornReturned -= HandleCornReturned;
                cornStorage.OnAllCornStolen -= HandleAllCornTaken;
            }
        }
        
        /// <summary>
        /// Register that an enemy grabbed corn from storage
        /// </summary>
        public void RegisterCornGrab(Enemy thief)
        {
            if (cornStorage == null)
                return;
            
            bool success = cornStorage.TakeCorn(thief);
            if (success)
            {
                OnCornGrabbed?.Invoke(thief);
                Debug.Log($"[CornManager] {thief.name} grabbed corn!");
            }
        }
        
        /// <summary>
        /// Register that an enemy successfully returned to spawn with corn
        /// </summary>
        public void RegisterCornSteal(Enemy thief)
        {
            totalCornStolen++;
            Debug.Log($"[CornManager] Corn successfully stolen! Total stolen: {totalCornStolen}");
            
            OnCornSuccessfullyStolen?.Invoke(thief);
            
            CheckGameLossCondition();
        }
        
        /// <summary>
        /// Return corn to storage (when carrier dies)
        /// </summary>
        public void ReturnCornToStorage()
        {
            if (cornStorage != null)
            {
                cornStorage.ReturnCorn();
            }
        }
        
        /// <summary>
        /// Check if all corn has been stolen (game loss condition)
        /// </summary>
        public bool IsGameLost()
        {
            // Game is lost when all corn successfully returned to enemy spawn
            return cornStorage != null && totalCornStolen >= cornStorage.InitialCornCount;
        }
        
        /// <summary>
        /// Get corn storage position for pathfinding
        /// </summary>
        public Vector3 GetCornStoragePosition()
        {
            return cornStorage != null ? cornStorage.Position : Vector3.zero;
        }
        
        /// <summary>
        /// Check if enemy is in range to grab corn
        /// </summary>
        public bool IsInGrabRange(Vector3 enemyPosition)
        {
            return cornStorage != null && cornStorage.IsInGrabRange(enemyPosition);
        }
        
        private void CheckGameLossCondition()
        {
            if (IsGameLost())
            {
                Debug.LogWarning("[CornManager] GAME LOST - All corn has been stolen!");
                OnGameLostToCorn?.Invoke();
            }
        }
        
        private void HandleCornTaken(int remainingCount)
        {
            Debug.Log($"[CornManager] Corn taken. Remaining: {remainingCount}");
            
            // Could trigger warning UI when corn gets low
            if (remainingCount <= 5 && remainingCount > 0)
            {
                Debug.LogWarning($"[CornManager] WARNING: Only {remainingCount} corn remaining!");
            }
        }
        
        private void HandleCornReturned(int remainingCount)
        {
            Debug.Log($"[CornManager] Corn returned. Remaining: {remainingCount}");
        }
        
        private void HandleAllCornTaken()
        {
            Debug.LogWarning("[CornManager] All corn has been taken from storage!");
            // Note: Game isn't lost yet - enemies still need to return it to spawn
        }
        
        /// <summary>
        /// Reset for new game/wave
        /// </summary>
        public void ResetCornState()
        {
            totalCornStolen = 0;
            if (cornStorage != null)
            {
                cornStorage.ResetStorage();
            }
            Debug.Log("[CornManager] Corn state reset");
        }
        
        /// <summary>
        /// Get debug info string
        /// </summary>
        public string GetDebugInfo()
        {
            if (cornStorage == null)
                return "No corn storage";
            
            return $"Corn Status:\n" +
                   $"  In Storage: {RemainingCorn}/{InitialCornCount}\n" +
                   $"  Successfully Stolen: {totalCornStolen}\n" +
                   $"  In Transit: {InitialCornCount - RemainingCorn - totalCornStolen}";
        }
    }
}
