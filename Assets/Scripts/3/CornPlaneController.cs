using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CornPlaneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform nozzle;
    [SerializeField] private Transform wingCP;
    [SerializeField] private Transform propeller;
    [SerializeField] private InputActionAsset inputAsset;

    [Header("Engine")]
    [SerializeField] private float maxThrust = 8000f;
    [SerializeField] private float throttleSpeed = 0.5f;
    [SerializeField] private float propellerRPM = 1500f;

    [Header("Aerodynamics")]
    [SerializeField] private float liftPower = 0.05f;
    [SerializeField] private float dragCoeff = 0.015f;

    [Header("Controls")]
    [SerializeField] private float pitchPower = 2000f;
    [SerializeField] private float rollPower = 3000f;
    [SerializeField] private float yawPower = 800f;

    private Rigidbody rb;
    private InputAction throttleAction, pitchAction, rollAction, yawAction, afterburnerAction;

    private float throttle01;
    private bool afterburner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 700f;
        rb.drag = 0.02f;
        rb.angularDrag = 2f;

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        if (inputAsset == null)
        {
            Debug.LogError("Input Action Asset не назначен!");
            return;
        }

        var map = inputAsset.FindActionMap("Plane");
        if (map == null)
        {
            Debug.LogError("Action Map 'Plane' не найден!");
            return;
        }

        throttleAction = map.FindAction("Throttle");
        pitchAction = map.FindAction("Pitch");
        rollAction = map.FindAction("Roll");
        yawAction = map.FindAction("Yaw");
        afterburnerAction = map.FindAction("Afterburner");

        if (throttleAction == null) Debug.LogError("Throttle action не найден!");
        if (pitchAction == null) Debug.LogError("Pitch action не найден!");
        if (rollAction == null) Debug.LogError("Roll action не найден!");
        if (yawAction == null) Debug.LogError("Yaw action не найден!");
        if (afterburnerAction == null) Debug.LogError("Afterburner action не найден!");

        afterburnerAction.performed += _ =>
        {
            afterburner = !afterburner;
            Debug.Log($"Afterburner: {afterburner}");
        };
    }

    private void OnEnable()
    {
        throttleAction?.Enable();
        pitchAction?.Enable();
        rollAction?.Enable();
        yawAction?.Enable();
        afterburnerAction?.Enable();
    }

    private void OnDisable()
    {
        throttleAction?.Disable();
        pitchAction?.Disable();
        rollAction?.Disable();
        yawAction?.Disable();
        afterburnerAction?.Disable();
    }

    private void FixedUpdate()
    {
        ApplyThrottle();
        ApplyAerodynamics();
        ApplyControls();
        RotatePropeller();
    }

    private void ApplyThrottle()
    {
        if (throttleAction == null) return;

        float throttleInput = throttleAction.ReadValue<float>();
        throttle01 = Mathf.Clamp01(throttle01 + throttleInput * throttleSpeed * Time.fixedDeltaTime);

        if (nozzle != null)
        {
            float thrust = throttle01 * maxThrust * (afterburner ? 1.5f : 1f);
            rb.AddForceAtPosition(nozzle.forward * thrust, nozzle.position, ForceMode.Force);
        }
    }

    private void ApplyAerodynamics()
    {
        if (wingCP == null) return;

        Vector3 velocity = rb.GetPointVelocity(wingCP.position);
        if (velocity.sqrMagnitude < 1f) return;

        Vector3 liftDir = transform.up;
        float liftForce = velocity.sqrMagnitude * liftPower;
        float dragForce = velocity.sqrMagnitude * dragCoeff;

        rb.AddForceAtPosition(liftDir * liftForce, wingCP.position, ForceMode.Force);
        rb.AddForce(-velocity.normalized * dragForce, ForceMode.Force);
    }

    private void ApplyControls()
    {
        if (pitchAction == null || rollAction == null || yawAction == null) return;

        float pitch = pitchAction.ReadValue<float>();
        float roll = rollAction.ReadValue<float>();
        float yaw = yawAction.ReadValue<float>();

        // Детальная отладка
        if (Mathf.Abs(pitch) > 0.1f || Mathf.Abs(roll) > 0.1f || Mathf.Abs(yaw) > 0.1f)
        {
            Debug.Log($"Controls - Pitch: {pitch:F2}, Roll: {roll:F2}, Yaw: {yaw:F2}");
        }

        Vector3 torque = new Vector3(
            pitch * pitchPower * Time.fixedDeltaTime,
            yaw * yawPower * Time.fixedDeltaTime,
            -roll * rollPower * Time.fixedDeltaTime
        );

        rb.AddRelativeTorque(torque, ForceMode.Force);
    }

    private void RotatePropeller()
    {
        if (propeller == null) return;
        propeller.Rotate(Vector3.forward, throttle01 * propellerRPM * Time.fixedDeltaTime, Space.Self);
    }

    private void Update()
    {
        // Отладка в Update для более частого вывода
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"RB Angular Velocity: {rb.angularVelocity}");
            Debug.Log($"RB Velocity: {rb.velocity.magnitude:F1} m/s");
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 250, 200), GUI.skin.box);
        GUILayout.Label("=== CORN PLANE DEBUG ===");
        GUILayout.Label($"Throttle: {throttle01 * 100:F0}%");
        GUILayout.Label($"Afterburner: {afterburner}");
        GUILayout.Label($"Speed: {rb.velocity.magnitude:F1} m/s");
        GUILayout.Label($"Alt: {transform.position.y:F1} m");

        if (pitchAction != null && rollAction != null && yawAction != null)
        {
            float pitch = pitchAction.ReadValue<float>();
            float roll = rollAction.ReadValue<float>();
            float yaw = yawAction.ReadValue<float>();
            GUILayout.Label($"Pitch: {pitch:F2}");
            GUILayout.Label($"Roll: {roll:F2}");
            GUILayout.Label($"Yaw: {yaw:F2}");
        }

        GUILayout.Label($"Angular Vel: {rb.angularVelocity.magnitude:F2}");
        GUILayout.EndArea();
    }
}