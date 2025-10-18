using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Main game manager that coordinates all game systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public int startingHealth = 20;
        public int startingGold = 1000;
        
        [Header("Corn Theft Settings")]
        [SerializeField] private bool enableCornTheftMode = true;
        [Tooltip("If true, game loss is based on corn theft. If false, uses traditional health system.")]
        [SerializeField] private bool useCornForLossCondition = true;
        
        [Header("Current Game State")]
        [SerializeField] private int currentHealth;
        [SerializeField] private int currentGold;
        [SerializeField] private int currentWave;
        [SerializeField] private GameState gameState;
        
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        // Game state properties
        public int CurrentHealth => currentHealth;
        public int CurrentGold => currentGold;
        public int CurrentWave => currentWave;
        public GameState GameState => gameState;
        
        // Events
        public System.Action<int> OnHealthChanged;
        public System.Action<int> OnGoldChanged;
        public System.Action<int> OnWaveChanged;
        public System.Action<GameState> OnGameStateChanged;
        public System.Action OnGameOver;
        public System.Action OnVictory;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Subscribe to corn theft events if enabled
            if (enableCornTheftMode && CornManager.Instance != null)
            {
                CornManager.Instance.OnGameLostToCorn += HandleCornGameLost;
                Debug.Log("GameManager: Subscribed to corn theft events");
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (CornManager.Instance != null)
            {
                CornManager.Instance.OnGameLostToCorn -= HandleCornGameLost;
            }
        }
        
        private void InitializeGame()
        {
            currentHealth = startingHealth;
            currentGold = startingGold;
            currentWave = 0;
            gameState = GameState.Preparing;
            
            Debug.Log("Game initialized - Tower Fusion 5");
        }
        
        public void ModifyHealth(int amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Max(0, currentHealth);
            OnHealthChanged?.Invoke(currentHealth);
            
            // Check loss condition based on mode
            if (useCornForLossCondition)
            {
                // In corn theft mode, health damage doesn't cause game over
                // Only corn theft causes loss (handled by CornManager)
                if (currentHealth <= 0)
                {
                    Debug.LogWarning("Health depleted, but corn theft mode is active. Game continues until corn stolen.");
                }
            }
            else
            {
                // Traditional health-based game over
                if (currentHealth <= 0)
                {
                    GameOver();
                }
            }
        }
        
        public bool SpendGold(int amount)
        {
            if (currentGold >= amount)
            {
                currentGold -= amount;
                OnGoldChanged?.Invoke(currentGold);
                return true;
            }
            return false;
        }
        
        public void AddGold(int amount)
        {
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }
        
        public void StartWave()
        {
            if (gameState == GameState.Preparing)
            {
                currentWave++;
                gameState = GameState.WaveInProgress;
                OnWaveChanged?.Invoke(currentWave);
                OnGameStateChanged?.Invoke(gameState);
                
                // Start wave through Wave Manager
                WaveManager.Instance?.StartWave(currentWave);
            }
        }
        
        public void EndWave()
        {
            if (gameState == GameState.WaveInProgress)
            {
                gameState = GameState.Preparing;
                OnGameStateChanged?.Invoke(gameState);
                
                // Add wave completion bonus
                AddGold(50 + (currentWave * 10));
            }
        }
        
        public void GameOver()
        {
            gameState = GameState.GameOver;
            OnGameStateChanged?.Invoke(gameState);
            OnGameOver?.Invoke();
            Debug.Log("Game Over!");
        }
        
        public void Victory()
        {
            // Check if corn theft mode requires corn validation
            if (enableCornTheftMode && CornManager.Instance != null)
            {
                if (CornManager.Instance.RemainingCorn <= 0)
                {
                    Debug.LogWarning("Cannot win - all corn has been taken!");
                    return;
                }
                
                Debug.Log($"Victory with {CornManager.Instance.RemainingCorn} corn remaining!");
            }
            
            gameState = GameState.Victory;
            OnGameStateChanged?.Invoke(gameState);
            OnVictory?.Invoke();
            Debug.Log("Victory!");
        }
        
        /// <summary>
        /// Handle game loss due to corn theft
        /// </summary>
        private void HandleCornGameLost()
        {
            Debug.LogError("Game Lost - All corn has been stolen!");
            GameOver();
        }
        
        public void RestartGame()
        {
            currentHealth = startingHealth;
            currentGold = startingGold;
            currentWave = 0;
            gameState = GameState.Preparing;
            
            OnHealthChanged?.Invoke(currentHealth);
            OnGoldChanged?.Invoke(currentGold);
            OnWaveChanged?.Invoke(currentWave);
            OnGameStateChanged?.Invoke(gameState);
        }
    }
    
    public enum GameState
    {
        Preparing,
        WaveInProgress,
        GameOver,
        Victory,
        Paused
    }
}