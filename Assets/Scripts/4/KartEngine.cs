using UnityEngine;

public class KartEngine : MonoBehaviour
{
    [SerializeField] private float _idleRpm = 1000f;
    [SerializeField] private float _maxRpm = 8000f;
    [SerializeField] private float _revLimiterRpm = 7500f;

    [SerializeField] private AnimationCurve _torqueCurve;

    [SerializeField] private float _flywheelInertia = 0.2f;
    [SerializeField] private float _throttleResponse = 5f;

    [SerializeField] private float _engineFrictionCoeff = 0.02f;
    [SerializeField] private float _loadTorqueCoeff = 5f;

    public float CurrentRpm { get; private set; }
    public float CurrentTorque { get; private set; }
    public float SmoothedThrottle { get; private set; }
    public float RevLimiterFactor { get; private set; } = 1f;

    private float _invInertiaFactor;

    private void Awake()
    {
        CurrentRpm = _idleRpm;
        _invInertiaFactor = 60f / (2f * Mathf.PI * Mathf.Max(_flywheelInertia, 0.0001f));
    }

    public float Simulate(float throttleInput, float forwardSpeed, float deltaTime)
    {
        float targetThrottle = Mathf.Clamp01(throttleInput);
        SmoothedThrottle = Mathf.MoveTowards(SmoothedThrottle, targetThrottle, _throttleResponse * deltaTime);

        UpdateRevLimiterFactor();

        float maxTorqueAtRpm = _torqueCurve.Evaluate(CurrentRpm);
        float driveTorque = maxTorqueAtRpm * SmoothedThrottle * RevLimiterFactor;

        float frictionTorque = _engineFrictionCoeff * CurrentRpm;
        float loadTorque = _loadTorqueCoeff * Mathf.Abs(forwardSpeed);

        float netTorque = driveTorque - frictionTorque - loadTorque;

        float rpmDot = netTorque * _invInertiaFactor;
        CurrentRpm += rpmDot * deltaTime;

        if (CurrentRpm < _idleRpm) CurrentRpm = _idleRpm;
        if (CurrentRpm > _maxRpm) CurrentRpm = _maxRpm;

        CurrentTorque = driveTorque;
        return CurrentTorque;
    }

    private void UpdateRevLimiterFactor()
    {
        if (CurrentRpm <= _revLimiterRpm)
        {
            RevLimiterFactor = 1f;
            return;
        }

        if (CurrentRpm >= _maxRpm)
        {
            RevLimiterFactor = 0f;
            return;
        }

        float t = (CurrentRpm - _revLimiterRpm) / (_maxRpm - _revLimiterRpm);
        RevLimiterFactor = 1f - t;
    }
}
