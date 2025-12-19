using UnityEngine;
using UnityEngine.InputSystem;

public class FlightController : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [Header("Rate Control (PD)")]
    [SerializeField] private Vector3 _maxRateDeg = new Vector3(90, 90, 120);
    [SerializeField] private Vector3 _kp = new Vector3(3, 2, 3);
    [SerializeField] private Vector3 _kd = new Vector3(0.8f, 0.6f, 0.9f);
    [SerializeField] private Vector3 _maxTorque = new Vector3(15, 10, 20);
    [SerializeField] private float _deadZone = 0.05f;
    [SerializeField] private Vector2 _attHoldKp = new Vector2(2, 2);
    [SerializeField] private float _attHoldMaxRate = 45f;

    private Rigidbody _rigidbody;
    private InputAction _yaw;
    private InputAction _pitch;
    private InputAction _roll;
    private InputAction _hold;
    private float _targetPitchDeg;
    private float _targetRollDeg;
    private bool _isHolding;

    public Vector3 CurrentRateCommand { get; private set; }
    public bool IsHoldingAttitude => _isHolding;

    private void Awake() => Initialize();

    private void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
        var map = _playerInput.actions.FindActionMap("Aircraft");
        _pitch = map.FindAction("PitchUp");
        _roll = map.FindAction("RollRight");
        _yaw = map.FindAction("YawRight");
        _hold = map.FindAction("HoldAttribute");
    }

    private void OnEnable()
    {
        _pitch?.Enable();
        _roll?.Enable();
        _yaw?.Enable();
        _hold?.Enable();
    }

    private void OnDisable()
    {
        _pitch?.Disable();
        _roll?.Disable();
        _yaw?.Disable();
        _hold?.Disable();
    }

    private void FixedUpdate()
    {
        Vector3 omega = _rigidbody.angularVelocity;
        Vector3 omegaBody = transform.InverseTransformDirection(omega);
        CurrentRateCommand = ReadRateCommandDeg();

        Vector3 rateCmdDeg;
        if (_isHolding)
        {
            rateCmdDeg = GenerateHoldRateDeg();
        }
        else
        {
            rateCmdDeg = CurrentRateCommand;
        }

        ApplyPDControl(rateCmdDeg, omegaBody);
    }

    private void ApplyPDControl(Vector3 rateCmdDeg, Vector3 omegaBody)
    {
        Vector3 rateCmdRad = rateCmdDeg * Mathf.Deg2Rad;
        Vector3 omegaCmdRad = omegaBody;

        Vector3 error = rateCmdRad - omegaCmdRad;
        Vector3 torque = new Vector3(
            error.x * _kp.x - omegaCmdRad.x * _kd.x,
            error.y * _kp.y - omegaCmdRad.y * _kd.y,
            error.z * _kp.z - omegaCmdRad.z * _kd.z
        );

        torque = Vector3.Min(torque, _maxTorque);
        torque = Vector3.Max(torque, -_maxTorque);
        _rigidbody.AddRelativeTorque(torque, ForceMode.Force);
    }

    private Vector3 GenerateHoldRateDeg()
    {
        var (pitch, roll) = GetLocalPitchRollDeg();
        float pitchRate = Mathf.Clamp(-pitch * _attHoldKp.x, -_attHoldMaxRate, _attHoldMaxRate);
        float rollRate = Mathf.Clamp(-roll * _attHoldKp.y, -_attHoldMaxRate, _attHoldMaxRate);
        return new Vector3(pitchRate, 0, rollRate);
    }

    private (float pitch, float roll) GetLocalPitchRollDeg()
    {
        Vector3 e = transform.localEulerAngles;
        float pitch = NormalizeAngle(e.x);
        float roll = NormalizeAngle(e.z);
        return (pitch, roll);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;
        return angle;
    }

    private Vector3 ReadRateCommandDeg()
    {
        float uPitch = _pitch.ReadValue<float>();
        float uRoll = _roll.ReadValue<float>();
        float uYaw = _yaw.ReadValue<float>();

        if (Mathf.Abs(uPitch) < _deadZone) uPitch = 0;
        if (Mathf.Abs(uRoll) < _deadZone) uRoll = 0;
        if (Mathf.Abs(uYaw) < _deadZone) uYaw = 0;

        Vector3 max = _maxRateDeg;
        return new Vector3(uPitch * max.x, uYaw * max.y, uRoll * max.z);
    }

    public void SetHoldMode(bool hold)
    {
        _isHolding = hold;
    }

    public float GetPitchInput()
    {
        return _pitch.ReadValue<float>();
    }

    public float GetRollInput()
    {
        return _roll.ReadValue<float>();
    }

    public float GetYawInput()
    {
        return _yaw.ReadValue<float>();
    }
}