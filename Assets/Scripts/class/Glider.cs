using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GliderLesson
{
    [RequireComponent(typeof(Rigidbody))]
    public class Glider : MonoBehaviour
    {
        [Header("Atmosphere")]
        [SerializeField] private float _airDensity = 1.225f; // плотность атмосферы

        [Header("References")]
        [SerializeField] private Transform _wingCP; // контрольная точка крыла

        [Header("Wing Geometry & Aero")]
        [SerializeField] private float _wingAero = 1.5f; // площадь крыла, м²
        [SerializeField] private float _wingAspectRatio = 8f; // удлинение крыла (b²/S)
        [SerializeField] private float _oswaldEfficiency = 0.85f; // фактор Освальда

        [SerializeField] private float _wingCd0 = 0.02f; // паразитное сопротивление (трение, форма)
        [SerializeField] private float _wingCLalpha = 5.5f; // подъём на радиан (для тонкого профиля ~2π)

        [SerializeField] private float _alphaLimitDeg = 18f; // ограничение угла, чтобы не произошёл срыв

        // Телеметрия
        private Vector3 _vPoint;
        private float _speedMS;
        private float _alphaRad;
        private float _cl, _cd, _qDyn, _lMag, _dMagm, _glider;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (_wingCP == null)
                return;

            _vPoint = _rigidbody.GetPointVelocity(_wingCP.position);
            _speedMS = _vPoint.magnitude;

            if(_speedMS<0)
                return;

            // Направление набегающего потока
            Vector3 flowDir = (-_vPoint).normalized;
            Vector3 xChord = _wingCP.forward; 
            Vector3 zUp = _wingCP.up;        

            Vector3 ySpan = _wingCP.right;   

            // 2)
            float flowX = Vector3.Dot(flowDir, xChord);
            float flowZ = Vector3.Dot(flowDir, zUp);

            float alphaRaw = Mathf.Atan2(flowZ, flowX); 

            float aLim = Mathf.Deg2Rad * Mathf.Abs(_alphaLimitDeg);
            _alphaRad = Mathf.Clamp(alphaRaw, -aLim, aLim);

            _cl = _wingCLalpha * _alphaRad;

            // 3) 
            float kInduced = 1f / (Mathf.PI * Mathf.Max(_wingAspectRatio, 0.001f) * Mathf.Max(_oswaldEfficiency, 0.001f));
            
            _cd = _wingCd0 + kInduced * _cl * _cl;

            // 4) Силы

            _qDyn = 0.5f * _airDensity * _speedMS * _speedMS;
            _lMag = _qDyn * _wingAero * _cl;
            _dMagm = _qDyn * _wingAero * _cd;

            Vector3 Ddir = -flowDir; // направление сопротивления — против потока

            // Подъёмная сила перпендикулярна потоку, в плоскости, образованной потоком и размахом крыла
            Vector3 liftDir = Vector3.Cross(flowDir, ySpan);
            liftDir = liftDir.normalized;

            float q = 0.5f * _airDensity * _vPoint.sqrMagnitude; // динамическое давление
            float liftMag = q * _cl * _wingAero;
            float dragMag = q * _cd * _wingAero;

            // Векторные силы
            Vector3 L = liftDir * liftMag;
            Vector3 D = Ddir * dragMag;

            // 5) Приложение силы к точке крыла
            _rigidbody.AddForceAtPosition(L + D, _wingCP.position, ForceMode.Force);
           
        }

        private void OnGUI()
        {
            GUI.color = Color.black;
            GUILayout.BeginArea(new Rect(10, 10, 300, 180), GUI.skin.box);

            GUILayout.Label("Glider HUD");
            GUILayout.Label($"Скорость: {_speedMS:0.0} м/с");
            GUILayout.Label($"Угол атаки: {Mathf.Rad2Deg * _alphaRad:0.0}°");
            GUILayout.Label($"Динамическое давление: {_qDyn:0.0} Па");

            GUILayout.EndArea();
        }
    }
}