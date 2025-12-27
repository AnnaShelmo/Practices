using UnityEngine;
using UnityEngine.InputSystem;

namespace GliderLesson
{
    [RequireComponent(typeof(Rigidbody))]
    public class EngineAirplane : MonoBehaviour
    {
        [Header("Точка приложения силы")]
        [SerializeField] private Transform _nozzle; 

        [Header("Тяга двигателя (Н)")]
        [SerializeField] private float _thrustDrySL = 79609.4f;   
        [SerializeField] private float _thrustABSL = 129050.4f;  

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset _actionAsset;

        private Rigidbody _rigidbody;

        // текущее состояние 
        private float _throttle01;     
        private bool _afterBurner;     
        private float _speedMS;
        private float _lastAppliedThrust;

        // Input Actions
        private InputAction _throttleUpHold;
        private InputAction _throttleDownHold;
        private InputAction _toggleAB;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _rigidbody.mass = 2000;
            _throttle01 = 0f;
            _afterBurner = false;
            InitializeActions();
        }

        private void InitializeActions()
        {
            if (_actionAsset == null) return;

            var map = _actionAsset.FindActionMap("JetEngine");
            if (map == null)
            {
                Debug.LogError("ActionMap 'JetEngine' not found!");
                return;
            }

            _throttleUpHold = map.FindAction("ThrottleUp");
            _throttleDownHold = map.FindAction("ThrottleDown");
            _toggleAB = map.FindAction("ToggleAfterburner");

            if (_toggleAB != null)
                _toggleAB.performed += ctx => _afterBurner = !_afterBurner;
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
            _speedMS = _rigidbody.velocity.magnitude;
            float dt = Time.fixedDeltaTime;

            if (_throttleUpHold != null && _throttleUpHold.IsPressed())
                _throttle01 = Mathf.Clamp01(_throttle01 + 1f * dt);

            if (_throttleDownHold != null && _throttleDownHold.IsPressed())
                _throttle01 = Mathf.Clamp01(_throttle01 - 0.05f * dt);

            float thrust = _throttle01 * (_afterBurner ? _thrustABSL : _thrustDrySL);

            if (_nozzle != null && _rigidbody != null)
                _rigidbody.AddForceAtPosition(_nozzle.forward * thrust, _nozzle.position, ForceMode.Force);

            _lastAppliedThrust = thrust;
        }

        private void OnGUI()
        {
            GUI.color = Color.black;
            GUILayout.BeginArea(new Rect(10, 10, 300, 100), GUI.skin.box);
            GUILayout.Label("Engine HUD");
            GUILayout.Label($"Throttle: {_throttle01 * 100:0}%");
            GUILayout.Label($"Afterburner: {_afterBurner}");
            GUILayout.Label($"Applied Thrust: {_lastAppliedThrust:0} N");
            GUILayout.EndArea();
        }
    }
}