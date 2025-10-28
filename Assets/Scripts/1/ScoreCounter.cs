using TMPro;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    private static int _hits = 0;
    public static ScoreCounter Instance;
    [SerializeField] private TextMeshProUGUI _scoreText;

    private void Start()
    {
        UpdateUI();
    }

    public static void AddHit()
    {
        _hits++;
        if (Instance != null)
            Instance.UpdateUI();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (_scoreText != null)
            _scoreText.text = "Hits: " + _hits;
    }
}
