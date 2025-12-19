using UnityEngine;

namespace GliderLesson
{
    public class GliderLesson : MonoBehaviour
    {
        [Header("Atmosphere")]
        [SerializeField] private float _airDensity = 1.225f;
        [Header("References")]
        [SerializeField] private Transform _wingCp;
        [Header("Wing Geometry & Aero")]
        [SerializeField] private float _wingAero = 0.5f;
        [SerializeField] private float _wingAspectRatio = 8f;
        [SerializeField] private float _oswaldEfficiency = 0.85f;
        [SerializeField] private float _wingCDO = 0.02f;
        [SerializeField] private float _wingClapla = 2.0f;
        [SerializeField] private float _alphaLimitDeg = 18f;

        //телеметрия
        private Vector3 _vPoint;
        private float _speedMS;
        private float _alphaRad;
        private float _cl, _cd, _qDyn, _lMag, _dMage, _glider;
        private Rigidbody _rigidbody;

        public float AlphaDeg => Mathf.Rad2Deg * _alphaRad;
        public float LiftForce => _lMag;
        public float DragForce => _dMage;
        public float DynamicPressure => _qDyn;

        private void Awake()
        {
            // Вместо GetComponent - находим Rigidbody от корневого объекта
            _rigidbody = GetComponentInParent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("GliderLesson: No Rigidbody found in parent!");
            }
        }

        private void FixedUpdate()
        {
            if (_wingCp == null)
            {
                return;
            }

            // скорость крыла в точке
            _vPoint = _rigidbody.GetPointVelocity(_wingCp.position);
            _speedMS = _vPoint.magnitude;

            // ДОБАВЛЕНЫ ПРОВЕРКИ ОТ NaN
            if (_speedMS < 0.001f) return;
            if (float.IsNaN(_speedMS)) return;

            // угол атаки
            Vector3 flowDir = (-_vPoint).normalized;
            Vector3 xChard = _wingCp.forward;
            Vector3 zUp = _wingCp.up;
            Vector3 ySpan = _wingCp.right;

            float flowX = Vector3.Dot(flowDir, xChard);
            float flowZ = Vector3.Dot(flowDir, zUp);
            float alphaRaw = Mathf.Atan2(flowZ, flowX);

            //мягкое ограничение, чтобы модель не уходила в неустойчивую область
            float aLin = Mathf.Deg2Rad * Mathf.Abs(_alphaLimitDeg);
            _alphaRad = Mathf.Clamp(alphaRaw, -aLin, aLin);

            // аэродинамические коэффициенты
            _cl = _wingClapla * _alphaRad;
            var kInduced = 1f / (Mathf.PI * Mathf.Max(_wingAspectRatio, 0.1f) * Mathf.Max(_oswaldEfficiency, 0.1f));
            _cd = _wingCDO + kInduced * _cl * _cl;

            // Силы
            //динамическое давление
            _qDyn = 0.5f * _airDensity * _speedMS * _speedMS;
            _lMag = _qDyn * _wingAero * _cl;
            _dMage = _qDyn * _wingAero * _cd;

            Vector3 Ddir = -flowDir;

            //подъемная сила перпендикулярная потоку в плоскости
            Vector3 liftDir = Vector3.Cross(flowDir, ySpan);
            liftDir.Normalize();

            Vector3 l = _lMag * liftDir;
            Vector3 D = _dMage * Ddir;

            // ДОБАВЛЕНЫ ПРОВЕРКИ ПЕРЕД ПРИМЕНЕНИЕМ СИЛЫ
            if (float.IsNaN(_lMag) || float.IsNaN(_dMage)) return;
            if (float.IsNaN(liftDir.x) || float.IsNaN(Ddir.x)) return;
            if (!float.IsNaN(l.x) && !float.IsNaN(D.x))
            {
                _rigidbody.AddForceAtPosition(D + l, _wingCp.position, ForceMode.Force);
            }
        }
    }
}