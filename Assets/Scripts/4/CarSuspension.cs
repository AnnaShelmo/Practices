using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarSuspension : MonoBehaviour
{
    // =========================
    // SUSPENSION POINTS
    // =========================
    [Header("Suspension Points")]
    [SerializeField] private Transform fl;
    [SerializeField] private Transform fr;
    [SerializeField] private Transform rl;
    [SerializeField] private Transform rr;

    // =========================
    // SUSPENSION SETTINGS
    // =========================
    [Header("Suspension Settings")]
    [SerializeField] private float restLength = 0.4f;
    [SerializeField] private float springTravel = 0.2f;
    [SerializeField] private float springStiffness = 20000f;
    [SerializeField] private float damperStiffness = 3500f;
    [SerializeField] private float wheelRadius = 0.35f;

    // =========================
    // ANTI-ROLL BAR
    // =========================
    [Header("Anti-Roll Bar")]
    [SerializeField] private float frontAntiRollStiffness = 8000f;
    [SerializeField] private float rearAntiRollStiffness = 6000f;

    private Rigidbody rb;

    // Последние сжатия подвески
    private float lastFLcompression;
    private float lastFRcompression;
    private float lastRLcompression;
    private float lastRRcompression;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // 1. Симуляция каждой подвески
        SimulateWheel(fl, ref lastFLcompression);
        SimulateWheel(fr, ref lastFRcompression);
        SimulateWheel(rl, ref lastRLcompression);
        SimulateWheel(rr, ref lastRRcompression);

        // 2. Анти-ролл стабилизатор
        ApplyAntiRollBars();
    }

    // =========================
    // SUSPENSION CORE
    // =========================
    private void SimulateWheel(Transform pivot, ref float lastCompression)
    {
        Vector3 origin = pivot.position;
        Vector3 direction = -pivot.up;

        float maxDistance = restLength + springTravel + wheelRadius;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            float currentLength = hit.distance - wheelRadius;

            currentLength = Mathf.Clamp(
                currentLength,
                restLength - springTravel,
                restLength + springTravel
            );

            float compression = restLength - currentLength;

            float springForce = compression * springStiffness;

            float compressionVelocity =
                (compression - lastCompression) / Time.fixedDeltaTime;

            float damperForce = compressionVelocity * damperStiffness;

            lastCompression = compression;

            float totalForce = springForce + damperForce;

            Vector3 force = pivot.up * totalForce;

            rb.AddForceAtPosition(force, pivot.position, ForceMode.Force);
        }
        else
        {
            // Колесо в воздухе
            lastCompression = 0f;
        }
    }

    // =========================
    // ANTI-ROLL BAR
    // =========================
    private void ApplyAntiRollBars()
    {
        // ----- FRONT AXLE -----
        float frontDiff = lastFLcompression - lastFRcompression;
        float frontForce = frontDiff * frontAntiRollStiffness;

        if (lastFLcompression > 0f)
            rb.AddForceAtPosition(-transform.up * frontForce, fl.position, ForceMode.Force);

        if (lastFRcompression > 0f)
            rb.AddForceAtPosition(transform.up * frontForce, fr.position, ForceMode.Force);

        // ----- REAR AXLE -----
        float rearDiff = lastRLcompression - lastRRcompression;
        float rearForce = rearDiff * rearAntiRollStiffness;

        if (lastRLcompression > 0f)
            rb.AddForceAtPosition(-transform.up * rearForce, rl.position, ForceMode.Force);

        if (lastRRcompression > 0f)
            rb.AddForceAtPosition(transform.up * rearForce, rr.position, ForceMode.Force);
    }
}
