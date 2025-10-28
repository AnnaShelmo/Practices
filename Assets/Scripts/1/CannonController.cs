using UnityEngine;

public class CannonController : MonoBehaviour
{
    [SerializeField] private Transform _muzzle;
    [SerializeField] private TrajectoryRenderer _trajectoryRenderer;
    [SerializeField] private GameObject _projectilePrefab;

    [Header("”правление пушкой")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotateSpeed = 60f;   // вращение по Y
    [SerializeField] private float _pitchSpeed = 30f;    // наклон дула (R/F)
    [SerializeField] private float _launchSpeed = 25f;

    [Header("ƒиапазоны параметров снар€да")]
    [SerializeField] private float mMin = 0.5f;
    [SerializeField] private float mMax = 5f;
    [SerializeField] private float rMin = 0.05f;
    [SerializeField] private float rMax = 0.4f;

    [Header("јэродинамика")]
    [SerializeField] private float _dragCoefficient = 0.47f;
    [SerializeField] private float _airDensity = 1.225f;
    [SerializeField] private Vector3 _wind = Vector3.zero;

    private float _currentMass;
    private float _currentRadius;

    private void Start()
    {
        RandomizeProjectileParams();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleMuzzlePitch();

        Vector3 initialVelocity = _muzzle.forward * _launchSpeed;

        // обновление превью траектории
        if (_trajectoryRenderer != null)
        {
            _trajectoryRenderer.SetAirParams(_currentMass, _currentRadius, _dragCoefficient, _airDensity, _wind);
            _trajectoryRenderer.DrawTrajectory(_muzzle.position, initialVelocity);
        }

        // выстрел
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire(initialVelocity);
        }
    }

    private void HandleMovementInput()
    {
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;

        Vector3 move = (transform.right * h + transform.forward * v) * _moveSpeed * Time.deltaTime;
        transform.position += new Vector3(move.x, 0f, move.z);

        if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.up, -_rotateSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime, Space.World);
    }

    private void HandleMuzzlePitch()
    {
        if (_muzzle == null) return;

        if (Input.GetKey(KeyCode.R)) // вверх
            _muzzle.Rotate(Vector3.right, -_pitchSpeed * Time.deltaTime, Space.Self);

        if (Input.GetKey(KeyCode.F)) // вниз
            _muzzle.Rotate(Vector3.right, _pitchSpeed * Time.deltaTime, Space.Self);
    }

    private void RandomizeProjectileParams()
    {
        _currentMass = Random.Range(mMin, mMax);
        _currentRadius = Random.Range(rMin, rMax);
    }

    private void Fire(Vector3 initialVelocity)
    {
        if (_projectilePrefab == null || _muzzle == null) return;

        GameObject newCore = Instantiate(_projectilePrefab, _muzzle.position, Quaternion.identity);
        var qd = newCore.GetComponent<QuadraticDrag>();
        if (qd != null)
        {
            qd.SetPhysicalParams(_currentMass, _currentRadius, _dragCoefficient, _airDensity, _wind, initialVelocity);
        }
        else
        {
            var rb = newCore.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = _currentMass;
                rb.velocity = initialVelocity;
            }
            newCore.transform.localScale = Vector3.one * (_currentRadius * 2f);
        }

        RandomizeProjectileParams();
        if (_trajectoryRenderer != null)
            _trajectoryRenderer.SetAirParams(_currentMass, _currentRadius, _dragCoefficient, _airDensity, _wind);
    }

    public void SetRanges(float mMinNew, float mMaxNew, float rMinNew, float rMaxNew)
    {
        mMin = mMinNew;
        mMax = mMaxNew;
        rMin = rMinNew;
        rMax = rMaxNew;
    }
}
