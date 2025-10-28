using UnityEngine;
using System.Collections;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _targetPrefab;
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private float _spawnRadius = 15f;
    [SerializeField] private float _spawnHeight = 2f;

    [Header("Диапазоны параметров мишеней")]
    [SerializeField] private float mMin = 0.5f;
    [SerializeField] private float mMax = 3f;
    [SerializeField] private float rMin = 0.1f;
    [SerializeField] private float rMax = 0.6f;

    [Header("Стартовая скорость мишеней (горизонтально)")]
    [SerializeField] private float minSpeed = 0f;
    [SerializeField] private float maxSpeed = 4f;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnOne()
    {
        if (_targetPrefab == null) return;

        Vector2 circle = Random.insideUnitCircle * _spawnRadius;
        Vector3 pos = transform.position + new Vector3(circle.x, _spawnHeight, circle.y);

        GameObject t = Instantiate(_targetPrefab, pos, Quaternion.identity);

        float mass = Random.Range(mMin, mMax);
        float radius = Random.Range(rMin, rMax);

        // масштабирование по радиусу (диаметр)
        t.transform.localScale = Vector3.one * (radius * 2f);

        var rb = t.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = mass;
            rb.velocity = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * Random.Range(minSpeed, maxSpeed);
        }

        // если есть компонент Target
        var targetScript = t.GetComponent<Target>();
        if (targetScript != null)
        {
            targetScript.SetParams(mass, radius);
        }
    }
}
