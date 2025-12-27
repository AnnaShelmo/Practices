using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class KartAero : MonoBehaviour
{
    // =========================
    // AERO DRAG
    // =========================
    [Header("Aero Drag")]
    [SerializeField] private float airDensity = 1.225f;
    [SerializeField] private float dragCoefficient = 0.9f;
    [SerializeField] private float frontalArea = 0.6f;

    // =========================
    // REAR WING
    // =========================
    [Header("Rear Wing")]
    [SerializeField] private Transform rearWing;
    [SerializeField] private float wingArea = 0.4f;
    [SerializeField] private float liftCoefficientSlope = 0.05f;

    [Header("Wing Angles")]
    public float normalWingAngle = 20f;
    public float drsWingAngle = -3f;

    // =========================
    // GROUND EFFECT
    // =========================
    [Header("Ground Effect")]
    [SerializeField] private float groundEffectStrength = 3000f;
    [SerializeField] private float groundRayLength = 1.0f;

    // =========================
    // INPUT
    // =========================
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    // =========================
    // TELEMETRY (READ ONLY)
    // =========================
    [Header("Telemetry")]
    public float speedMS;
    public float speedKMH;
    public float dragForceValue;
    public float downforceValue;
    public float groundEffectForce;
    public bool drsActive;


    private Rigidbody rb;
    private float currentWingAngle;
    private InputAction drsAction;

    // =========================
    // UNITY
    // =========================
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentWingAngle = normalWingAngle;

        // Kart = Action Map, DRS = Action
        drsAction = inputActions.FindAction("Kart/DRS", true);
    }

    private void OnEnable()
    {
        drsAction.Enable();
    }

    private void OnDisable()
    {
        drsAction.Disable();
    }

    private void FixedUpdate()
    {
        HandleDRS();

        ApplyDrag();
        ApplyWingDownforce();
        ApplyGroundEffect();

        speedMS = rb.velocity.magnitude;
        speedKMH = speedMS * 3.6f;
    }

    // =========================
    // DRS
    // =========================
    private void HandleDRS()
    {
        drsActive = drsAction.IsPressed();
        currentWingAngle = drsActive ? drsWingAngle : normalWingAngle;
    }

    // =========================
    // DRAG
    // =========================
    private void ApplyDrag()
    {
        Vector3 v = rb.velocity;
        float speed = v.magnitude;

        if (speed < 0.1f)
        {
            dragForceValue = 0f;
            return;
        }

        dragForceValue =
            0.5f *
            airDensity *
            dragCoefficient *
            frontalArea *
            speed *
            speed;

        Vector3 drag = -v.normalized * dragForceValue;
        rb.AddForce(drag, ForceMode.Force);
    }

    // =========================
    // DOWNFORCE
    // =========================
    private void ApplyWingDownforce()
    {
        if (rearWing == null)
        {
            downforceValue = 0f;
            return;
        }

        float speed = rb.velocity.magnitude;
        if (speed < 0.1f)
        {
            downforceValue = 0f;
            return;
        }

        float alphaRad = currentWingAngle * Mathf.Deg2Rad;
        float Cl = liftCoefficientSlope * alphaRad;

        downforceValue =
            0.5f *
            airDensity *
            Cl *
            wingArea *
            speed *
            speed;

        Vector3 force = -transform.up * downforceValue;
        rb.AddForceAtPosition(force, rearWing.position, ForceMode.Force);
    }

    // =========================
    // GROUND EFFECT
    // =========================
    private void ApplyGroundEffect()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, groundRayLength))
        {
            float h = Mathf.Max(hit.distance, 0.05f);
            groundEffectForce = groundEffectStrength / h;

            Vector3 force = -transform.up * groundEffectForce;
            rb.AddForce(force, ForceMode.Force);
        }
        else
        {
            groundEffectForce = 0f;
        }
    }
}
