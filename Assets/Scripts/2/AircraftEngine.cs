using UnityEngine;
using UnityEngine.InputSystem;

namespace GliderLesson
{
    [RequireComponent(typeof(Rigidbody))]
    public class AircraftEngine : MonoBehaviour
    {
        [Header("Точка приложения силы")]
        [SerializeField] private Transform _nozzle;
        [SerializeField] private float _thrustDrySL = 15000f;
        [SerializeField] private float _thrustABSL = 25000f;
        [SerializeField] private InputActionAsset _actionAsset;

        private Rigidbody _rigidbody;
        private float _throttle01 = 0f;
        private bool _afterBurner = false;
        private float _speedMS = 0f;
        private float _lastAppliedThrust = 0f;

        private InputAction _throttleUpHold;
        private InputAction _throttleDownHold;
        private InputAction _toggleAB;

        public float Throttle01 => _throttle01;
        public bool AfterBurner => _afterBurner;
        public float CurrentThrust => _lastAppliedThrust;
        public float SpeedMS => _speedMS;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            InitializeActions();
        }

        private void InitializeActions()
        {
            if (_actionAsset == null)
            {
                Debug.LogError("InputActionAsset not assigned in AircraftEngine!");
                return;
            }

            var map = _actionAsset.FindActionMap("Aircraft");
            if (map == null)
            {
                Debug.LogError("Aircraft Action Map not found!");
                return;
            }

            _throttleUpHold = map.FindAction("ThrottleUp");
            _throttleDownHold = map.FindAction("ThrottleDown");
            _toggleAB = map.FindAction("ToggleAB");

            if (_toggleAB != null)
            {
                _toggleAB.performed += context => { _afterBurner = !_afterBurner; };
            }
        }

        private void OnEnable()
        {
            _throttleUpHold?.Enable();
            _throttleDownHold?.Enable();
            _toggleAB?.Enable();
        }

        private void OnDisable()
        {
            _throttleUpHold?.Disable();
            _throttleDownHold?.Disable();
            _toggleAB?.Disable();
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null) return;

            _speedMS = _rigidbody.velocity.magnitude;
            float dt = Time.fixedDeltaTime;

            // Управление тягой
            if (_throttleUpHold?.IsPressed() == true)
                _throttle01 = Mathf.Clamp01(_throttle01 + 1f * dt);
            if (_throttleDownHold?.IsPressed() == true)
                _throttle01 = Mathf.Clamp01(_throttle01 - 1f * dt);

            // Расчет тяги
            float thrust = _throttle01 * (_afterBurner ? _thrustABSL : _thrustDrySL);
            _lastAppliedThrust = thrust;

            // Применение силы
            if (_nozzle != null)
            {
                Vector3 force = _nozzle.forward * thrust;
                _rigidbody.AddForceAtPosition(force, _nozzle.position, ForceMode.Force);
            }
            else
            {
                Vector3 force = transform.forward * thrust;
                _rigidbody.AddForce(force, ForceMode.Force);
            }
        }
    }
}