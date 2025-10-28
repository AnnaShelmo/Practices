using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Отрисовка")]
    [SerializeField] private int _pointsCount = 30;
    [SerializeField] private float _timeStep = 0.1f;
    [SerializeField] private float _widthLine = 0.02f;
    [SerializeField] private bool _useAir = true;

    [Header("Физика воздуха")]
    [SerializeField] private float _mass = 1f;
    [SerializeField] private float _radius = 0.1f;
    [SerializeField] private float _dragCoefficient = 0.47f;
    [SerializeField] private float _airDensity = 1.225f;
    [SerializeField] private Vector3 _wind = Vector3.zero;

    private LineRenderer _line;
    private float _area => Mathf.PI * _radius * _radius;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true;
        _line.widthMultiplier = _widthLine;
        _line.material = new Material(Shader.Find("Sprites/Default"));
    }

    // внешний метод, вызывается контроллером пушки
    public void DrawTrajectory(Vector3 startPosition, Vector3 startVelocity)
    {
        if (_useAir)
            DrawWithAirEuler(startPosition, startVelocity);
        else
            DrawVacuum(startPosition, startVelocity);
    }

    public void DrawVacuum(Vector3 startPosition, Vector3 startVelocity)
    {
        int count = Mathf.Max(2, _pointsCount);
        _line.positionCount = count;

        for (int i = 0; i < count; i++)
        {
            float t = i * _timeStep;
            Vector3 p = startPosition + startVelocity * t + 0.5f * Physics.gravity * (t * t);
            _line.SetPosition(i, p);
        }
    }

    public void DrawWithAirEuler(Vector3 startPosition, Vector3 startVelocity)
    {
        int count = Mathf.Max(2, _pointsCount);
        _line.positionCount = count;

        Vector3 p = startPosition;
        Vector3 v = startVelocity;
        float area = _area;

        for (int i = 0; i < count; i++)
        {
            _line.SetPosition(i, p);

            // относительно ветра
            Vector3 vRel = v - _wind;
            float speed = vRel.magnitude;
            Vector3 drag = Vector3.zero;
            if (speed > 1e-6f)
            {
                drag = (-0.5f * _airDensity * _dragCoefficient * area * speed) * vRel;
            }

            Vector3 a = Physics.gravity + drag / Mathf.Max(0.0001f, _mass);

            v += a * _timeStep; // шаг по скорости
            p += v * _timeStep; // шаг по позиции
        }
    }

    // позволяет контроллеру пушек задавать параметры воздуха/снаряда для превью
    public void SetAirParams(float mass, float radius, float cd, float rho, Vector3 wind)
    {
        _mass = Mathf.Max(0.0001f, mass);
        _radius = Mathf.Max(0.0001f, radius);
        _dragCoefficient = Mathf.Max(0f, cd);
        _airDensity = Mathf.Max(0f, rho);
        _wind = wind;
    }

    public void SetUseAir(bool useAir)
    {
        _useAir = useAir;
    }
}
