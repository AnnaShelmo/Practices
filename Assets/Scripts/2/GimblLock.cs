using UnityEngine;

public class GimbalLock : MonoBehaviour
{
    public enum RightMode { EulerMode, QuaternionMode }

    [Header("Ёйлеровы углы")]
    [Range(-180, 180), SerializeField] private float _yawDeg;
    [Range(-180, 180), SerializeField] private float _pitchDeg;
    [Range(-180, 180), SerializeField] private float _rollDeg;
    [SerializeField] private RightMode _rightMode = RightMode.QuaternionMode;

    private Transform _yawTransform;
    private Transform _pitchTransform;
    private Transform _rollTransform;
    private Transform _leftArrow;

    private Transform _rightRoot, _rightArrow;
    private Quaternion _qRght = Quaternion.identity;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        var leftroot = new GameObject("EulerGimbals").transform;

        _yawTransform = new GameObject("yaw").transform;
        _yawTransform.SetParent(leftroot, false);

        _pitchTransform = new GameObject("pitch").transform;
        _pitchTransform.SetParent(_yawTransform, false);

        _rollTransform = new GameObject("roll").transform;
        _rollTransform.SetParent(_pitchTransform, false);

        _leftArrow = CreateArrow("leftArrow", _rollTransform);

        _rightRoot = new GameObject("RightRoot").transform;
    }

    private void Update()
    {
        if (_rightMode == RightMode.EulerMode)
        {
            _yawTransform.localRotation = Quaternion.Euler(0f, _yawDeg, 0f);
            _pitchTransform.localRotation = Quaternion.Euler(_pitchDeg, 0f, 0f);
            _rollTransform.localRotation = Quaternion.Euler(0f, 0f, _rollDeg);
        }
        else if (_rightMode == RightMode.QuaternionMode)
        {
            float dt = Time.deltaTime;
            float dYaw = _yawDeg * dt;
            float dPitch = _pitchDeg * dt;
            float dRoll = _rollDeg * dt;

            Quaternion q = Quaternion.Euler(dPitch, dYaw, dRoll);
            Quaternion dQ = Quaternion.AngleAxis(dRoll, Vector3.forward) *
                            Quaternion.AngleAxis(dPitch, Vector3.right) *
                            Quaternion.AngleAxis(dYaw, Vector3.up);

            _qRght = Normalized(_qRght * dQ);
            _rightRoot.rotation = _qRght;
        }
    }

    private Quaternion Normalized(Quaternion q)
    {
        float m = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

        return (m > 1e-6f)
            ? new Quaternion(q.x / m, q.y / m, q.z / m, q.w / m)
            : Quaternion.identity;
    }

    private Transform CreateArrow(string name, Transform parent)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);

        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = "shaft";
        shaft.transform.SetParent(root, false);
        shaft.transform.localScale = new Vector3(0.05f, 0.05f, 1.5f);
        shaft.GetComponent<Renderer>().material.color = Color.yellow;

        return root;
    }
}