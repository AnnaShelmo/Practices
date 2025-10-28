using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class QuadraticDrag : MonoBehaviour
{
    [SerializeField] private float mass = 1f;
    [SerializeField] private float radius = 0.1f;
    [SerializeField] private float dragCoefficient = 0.47f;
    [SerializeField] private float airDensity = 1.225f;
    [SerializeField] private Vector3 wind = Vector3.zero;

    private Rigidbody rb;
    private float area;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.angularDrag = 0f;
    }

    public void SetPhysicalParams(float m, float r, float Cd, float rho, Vector3 w, Vector3 initialVelocity)
    {
        mass = Mathf.Max(0.0001f, m);
        radius = Mathf.Max(0.0001f, r);
        dragCoefficient = Mathf.Max(0f, Cd);
        airDensity = Mathf.Max(0f, rho);
        wind = w;

        area = Mathf.PI * radius * radius;

        rb.mass = mass;
        rb.velocity = initialVelocity;
        rb.useGravity = true;
        rb.drag = 0f; // отключение встроенного линейного drag
        rb.angularVelocity = Vector3.zero;

        // масштабирование объекта по радиусу 
        float scale = radius * 2f; // диаметр
        transform.localScale = Vector3.one * scale;
    }

    private void FixedUpdate()
    {
        Vector3 vRel = rb.velocity - wind;
        float speed = vRel.magnitude;
        if (speed < 1e-6f) return;

        Vector3 drag = -0.5f * airDensity * dragCoefficient * area * speed * vRel;
        rb.AddForce(drag, ForceMode.Force);
    }
}
