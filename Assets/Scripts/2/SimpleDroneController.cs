using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleDroneController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _maxThrust = 30f;            // Максимальная тяга (Н)
    [SerializeField] private float _thrustResponse = 0.6f;      // Чувствительность тяги
    [SerializeField] private float _torquePowerRoll = 2.5f;     // Момент по крену (ось X)
    [SerializeField] private float _torquePowerPitch = 2.5f;    // Момент по тангажу (ось Z)
    [SerializeField] private float _torquePowerYaw = 1.8f;      // Момент по рысканью (ось Y)

    [Header("Damping")]
    [SerializeField] private float _angularDamping = 0.56f;     // Угловое демпфирование
    [SerializeField] private float _linearDamping = 0.2f;       // Линейное демпфирование скорости

    [Header("Input System")]
    [SerializeField] private InputActionAsset _actionAsset;    // Схема действий (Input System)

    private Rigidbody _rigidbody;

    // Input Actions
    private InputAction _throttleUp;
    private InputAction _throttleDown;
    private InputAction _yawRight;
    private InputAction _yawLeft;
    private InputAction _pitchUp;
    private InputAction _pitchDown;
    private InputAction _rollRight;
    private InputAction _rollLeft;

    // Inputs
    private float _throttleInput;
    private float _yawInput;
    private float _pitchInput;
    private float _rollInput;

    private float _hoverShare; // доля тяги для висения (вес / макс. тяга)

    private void Awake()
    {
        _rigidbody  = GetComponent<Rigidbody>();

        _hoverShare = (_rigidbody.mass * Physics.gravity.magnitude) / Mathf.Max(0.01f, _maxThrust);
        InitializeActions();
    }

    private void InitializeActions()
    {
        var map = _actionAsset.FindActionMap("Drone");

        _throttleUp = map.FindAction("ThrottleUp");
        _throttleDown = map.FindAction("ThrottleDown");

        _yawRight = map.FindAction("YawRight");
        _yawLeft = map.FindAction("YawLeft");

        _pitchUp = map.FindAction("PitchUp");
        _pitchDown = map.FindAction("PitchDown");

        _rollRight = map.FindAction("RollRight");
        _rollLeft = map.FindAction("RollLeft");
    }

    private void OnEnable()
    {
        _throttleUp.Enable();
        _throttleDown.Enable();
        _yawRight.Enable();
        _yawLeft.Enable();
        _pitchUp.Enable();
        _pitchDown.Enable();
        _rollRight.Enable();
        _rollLeft.Enable();
    }

    private void OnDisable()
    {
        _throttleUp.Disable();
        _throttleDown.Disable();
        _yawRight.Disable();
        _yawLeft.Disable();
        _pitchUp.Disable();
        _pitchDown.Disable();
        _rollRight.Disable();
        _rollLeft.Disable();
    }
    private void Update() => ReadInput();

    // Чтение значений из Input Actions
    private void ReadInput()
    {
        _throttleInput = Bool01(_throttleUp) - Bool01(_throttleDown);
        _yawInput = Bool01(_yawRight) - Bool01(_yawLeft);
        _pitchInput = Bool01(_pitchUp) - Bool01(_pitchDown);
        _rollInput = Bool01(_rollRight) - Bool01(_rollLeft);
    }
    private float Bool01(InputAction action)
    {
        return action.IsPressed() ? 1f : 0f;
    }

    private void FixedUpdate()
    {
        ApplyForces();
        ApplyTorque();
        ApplyDamping();
    }

    private void ApplyForces()
    {
        // Тяга вдоль локальной "вверх"
        float thrustShare = Mathf.Clamp01(_hoverShare + _thrustResponse * _throttleInput);
        float thrust = thrustShare * _maxThrust;

        _rigidbody.AddRelativeForce(Vector3.up * thrust, ForceMode.Force);
    }

    private void ApplyTorque()
    {
        Vector3 localTorque = new Vector3(
            _pitchInput * _torquePowerPitch, 
            _yawInput * _torquePowerYaw,      
            _rollInput * _torquePowerRoll     
        );

        _rigidbody.AddRelativeTorque(localTorque, ForceMode.Force);

        _rigidbody.AddTorque(-_rigidbody.angularVelocity * _angularDamping, ForceMode.Force);
    }

    private void ApplyDamping()
    {
        _rigidbody.AddForce(-_rigidbody.velocity * _linearDamping, ForceMode.Force);
    }

    //добавить вращение винтов. вращение должно зависеть от тяги.
}
