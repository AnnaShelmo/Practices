using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class KartController : MonoBehaviour
{
    [SerializeField] private float _gravity = 9.81f;

    [SerializeField] private Transform _frontLeftWheel;
    [SerializeField] private Transform _frontRightWheel;
    [SerializeField] private Transform _rearLeftWheel;
    [SerializeField] private Transform _rearRightWheel;

    [Range(0f, 1f)]
    [SerializeField] private float _frontAxleShare = 0.5f;

    [SerializeField] private float _maxSteerAngle = 30f;

    [SerializeField] private InputActionReference _moveActionRef;
    [SerializeField] private InputActionReference _handbrakeActionRef;

    [SerializeField] private KartEngine _engine;
    [SerializeField] private float _gearRatio = 8f;
    [SerializeField] private float _drivetrainEfficiency = 0.9f;
    [SerializeField] private float _wheelRadius = 0.3f;

    [SerializeField] private float _frontCAlpha = 80f;
    [SerializeField] private float _rearCAlpha = 80f;
    [SerializeField] private float _rollingResistance = 0.5f;
    [SerializeField] private float _frictionCoefficient = 1f;

    private Rigidbody _rb;
    private float _frontLeftNormalForce;
    private float _frontRightNormalForce;
    private float _rearLeftNormalForce;
    private float _rearRightNormalForce;

    private Quaternion _flInitRot;
    private Quaternion _frInitRot;

    private float _throttle;
    private float _steer;
    private bool _handbrake;

    private float _rearFxSum;
    private float _frontFySum;
    private float _rearVLatL;
    private float _rearVLatR;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _flInitRot = _frontLeftWheel.localRotation;
        _frInitRot = _frontRightWheel.localRotation;
        ComputeWheelLoads();
    }

    private void OnEnable()
    {
        _moveActionRef.action.Enable();
        _handbrakeActionRef.action.Enable();
    }

    private void OnDisable()
    {
        _moveActionRef.action.Disable();
        _handbrakeActionRef.action.Disable();
    }

    private void Update()
    {
        Vector2 move = _moveActionRef.action.ReadValue<Vector2>();
        _steer = Mathf.Clamp(move.x, -1f, 1f);
        _throttle = Mathf.Clamp(move.y, -1f, 1f);
        _handbrake = _handbrakeActionRef.action.IsPressed();

        Quaternion steerRot = Quaternion.Euler(0f, _maxSteerAngle * _steer, 0f);
        _frontLeftWheel.localRotation = _flInitRot * steerRot;
        _frontRightWheel.localRotation = _frInitRot * steerRot;
    }

    private void FixedUpdate()
    {
        _rearFxSum = 0f;
        _frontFySum = 0f;

        ApplyWheel(_frontLeftWheel, _frontLeftNormalForce, false, true);
        ApplyWheel(_frontRightWheel, _frontRightNormalForce, false, true);
        ApplyWheel(_rearLeftWheel, _rearLeftNormalForce, true, false);
        ApplyWheel(_rearRightWheel, _rearRightNormalForce, true, false);
    }

    private void ComputeWheelLoads()
    {
        float weight = _rb.mass * _gravity;
        float front = weight * _frontAxleShare;
        float rear = weight * (1f - _frontAxleShare);

        _frontLeftNormalForce = front * 0.5f;
        _frontRightNormalForce = front * 0.5f;
        _rearLeftNormalForce = rear * 0.5f;
        _rearRightNormalForce = rear * 0.5f;
    }

    private void ApplyWheel(Transform wheel, float normalForce, bool driven, bool front)
    {
        Vector3 pos = wheel.position;
        Vector3 fwd = wheel.forward;
        Vector3 right = wheel.right;

        Vector3 v = _rb.GetPointVelocity(pos);
        float vLong = Vector3.Dot(v, fwd);
        float vLat = Vector3.Dot(v, right);

        float Fx = 0f;
        float Fy = 0f;

        float cAlpha = front ? _frontCAlpha : _rearCAlpha;

        if (!front && _handbrake)
        {
            cAlpha = 0f;
            Fx += -5f * vLong;
        }

        Fy = -cAlpha * vLat;

        if (driven)
        {
            float speedFwd = Vector3.Dot(_rb.velocity, transform.forward);
            float engineTorque = _engine.Simulate(_throttle, speedFwd, Time.fixedDeltaTime);
            float wheelTorque = engineTorque * _gearRatio * _drivetrainEfficiency * 0.5f;
            Fx += wheelTorque / _wheelRadius;
            _rearFxSum += Fx;
        }

        Fx += -_rollingResistance * vLong;

        float limit = _frictionCoefficient * normalForce;
        float mag = Mathf.Sqrt(Fx * Fx + Fy * Fy);
        if (mag > limit)
        {
            float s = limit / mag;
            Fx *= s;
            Fy *= s;
        }

        if (front) _frontFySum += Fy;
        else
        {
            if (wheel == _rearLeftWheel) _rearVLatL = vLat;
            if (wheel == _rearRightWheel) _rearVLatR = vLat;
        }

        _rb.AddForceAtPosition(fwd * Fx + right * Fy, pos);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 250));
        GUILayout.Label($"Speed: {_rb.velocity.magnitude:F1} m/s ({_rb.velocity.magnitude * 3.6f:F1} km/h)");
        GUILayout.Label($"RPM: {_engine.CurrentRpm:F0}");
        GUILayout.Label($"Engine Torque: {_engine.CurrentTorque:F1}");
        GUILayout.Label($"Rear Fx Sum: {_rearFxSum:F1}");
        GUILayout.Label($"Front Fy Sum: {_frontFySum:F1}");
        GUILayout.Label($"Rear vLat L/R: {_rearVLatL:F2} / {_rearVLatR:F2}");
        GUILayout.EndArea();
    }
}
