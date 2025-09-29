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
        public int startingGold = 100;
        
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
        
        private void InitializeGame()
        {
            currentHealth = startingHealth;
            currentGold = startingGold;
            currentWave = 0;
            gameState = GameState.Preparing;
            
            Debug.Log("Game initialized - Tower Fusion 4");
        }
        
        public void ModifyHealth(int amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Max(0, currentHealth);
            OnHealthChanged?.Invoke(currentHealth);
            
            if (currentHealth <= 0)
            {
                GameOver();
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
            gameState = GameState.Victory;
            OnGameStateChanged?.Invoke(gameState);
            OnVictory?.Invoke();
            Debug.Log("Victory!");
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