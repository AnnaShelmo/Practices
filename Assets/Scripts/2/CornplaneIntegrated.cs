using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CornplaneIntegrated : MonoBehaviour
{
    [Header("Aerodynamics")]
    [SerializeField] private GliderLesson.GliderLesson _mainWing;
    [SerializeField] private GliderLesson.GliderLesson _tailWing;
    [Header("Propulsion")]
    [SerializeField] private GliderLesson.AircraftEngine _engine;
    [Header("Flight Control")]
    [SerializeField] private FlightController _flightController;
    [Header("Control Surfaces")]
    [SerializeField] private Transform _elevator;
    [SerializeField] private Transform _rudder;
    [SerializeField] private float _controlSurfaceMaxAngle = 25f;

    private Rigidbody _rb;
    private AircraftHUD _hud;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _hud = FindObjectOfType<AircraftHUD>();
        _rb.centerOfMass = Vector3.down * 0.3f;
    }

    private void Update()
    {
        UpdateControlSurfaces();
        UpdateHUD();
    }

    private void UpdateControlSurfaces()
    {
        // Используем входные данные из FlightController для управления поверхностями
        if (_elevator && _flightController)
        {
            float pitchInput = _flightController.GetPitchInput();
            _elevator.localRotation = Quaternion.Euler(pitchInput * _controlSurfaceMaxAngle, 0, 0);
        }

        if (_rudder && _flightController)
        {
            float yawInput = _flightController.GetYawInput();
            _rudder.localRotation = Quaternion.Euler(0, yawInput * _controlSurfaceMaxAngle, 0);
        }
    }

    private void UpdateHUD()
    {
        if (_hud)
        {
            _hud.UpdateTelemetry(
                airSpeed: _rb.velocity.magnitude,
                altitude: transform.position.y,
                pitch: NormalizeAngle(transform.eulerAngles.x),
                roll: NormalizeAngle(transform.eulerAngles.z),
                heading: NormalizeAngle(transform.eulerAngles.y),
                throttle: _engine ? _engine.Throttle01 : 0f
            );
        }
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
}