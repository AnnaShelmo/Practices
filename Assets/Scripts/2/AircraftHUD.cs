using GliderLesson;
using UnityEngine;

public class AircraftHUD : MonoBehaviour
{
    private CornplaneIntegrated _aircraft;
    private AircraftEngine _engine;
    private GliderLesson.GliderLesson _wing;
    private FlightController _flightController;

    private void Awake()
    {
        _aircraft = FindObjectOfType<CornplaneIntegrated>();
        _engine = FindObjectOfType<AircraftEngine>();
        _wing = FindObjectOfType<GliderLesson.GliderLesson>();
        _flightController = FindObjectOfType<FlightController>();
    }

    public void UpdateTelemetry(float airSpeed, float altitude, float pitch, float roll, float heading, float throttle)
    {
        // This method is called from CornplaneIntegrated to update HUD data
    }

    private void OnGUI()
    {
        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 350, 500), GUI.skin.box);
        GUILayout.Label(" ” ”–”«Õ»  - “≈À≈Ã≈“–»ﬂ", GUI.skin.label);
        GUILayout.Space(10);

        if (_aircraft && _aircraft.GetComponent<Rigidbody>())
        {
            var rb = _aircraft.GetComponent<Rigidbody>();
            Vector3 velocity = rb.velocity;
            float speedKmh = velocity.magnitude * 3.6f;

            GUILayout.Label("Œ—ÕŒ¬Õ€≈ œ¿–¿Ã≈“–€", GUI.skin.label);
            GUILayout.Label($"—ÍÓÓÒÚ¸: {speedKmh:0.0} ÍÏ/˜");
            GUILayout.Label($"¬˚ÒÓÚ‡: {_aircraft.transform.position.y:0.0} Ï");
            GUILayout.Label($" ÛÒ: {NormalizeAngle(_aircraft.transform.eulerAngles.y):0}∞");
            GUILayout.Label($" ÂÌ: {NormalizeAngle(_aircraft.transform.eulerAngles.z):0}∞");
            GUILayout.Label($"“‡Ì„‡Ê: {NormalizeAngle(_aircraft.transform.eulerAngles.x):0}∞");
            GUILayout.Label($"—ÍÓÓÒÚ¸ (X): {velocity.x:0.1} Ï/Ò");
            GUILayout.Label($"—ÍÓÓÒÚ¸ (Y): {velocity.y:0.1} Ï/Ò");
            GUILayout.Label($"—ÍÓÓÒÚ¸ (Z): {velocity.z:0.1} Ï/Ò");
            GUILayout.Space(10);
        }

        if (_engine)
        {
            GUILayout.Label("ƒ¬»√¿“≈À‹", GUI.skin.label);
            GUILayout.Label($"–ÂÊËÏ: {(_engine.AfterBurner ? "‘Œ–—¿∆" : "ÕŒ–Ã¿À‹Õ€…")}");
            GUILayout.Label($"“ˇ„‡: {_engine.Throttle01 * 100:0}%");
            GUILayout.Label($"—ÍÓÓÒÚ¸: {_engine.SpeedMS * 3.6f:0.0} ÍÏ/˜");
            GUILayout.Label($"—ËÎ‡ Úˇ„Ë: {_engine.CurrentThrust:0} Õ");
            GUILayout.Space(10);
        }

        if (_wing)
        {
            GUILayout.Label("¿›–Œƒ»Õ¿Ã» ¿", GUI.skin.label);
            GUILayout.Label($"”„ÓÎ ‡Ú‡ÍË: {_wing.AlphaDeg:0.0}∞");
            GUILayout.Label($"œÓ‰˙ÂÏÌ‡ˇ ÒËÎ‡: {_wing.LiftForce:0.0} Õ");
            GUILayout.Label($"—ÓÔÓÚË‚ÎÂÌËÂ: {_wing.DragForce:0.0} Õ");
            GUILayout.Label($"ƒËÌ‡ÏË˜ÂÒÍÓÂ ‰‡‚ÎÂÌËÂ: {_wing.DynamicPressure:0.0} œ‡");
            GUILayout.Space(10);
        }

        if (_flightController)
        {
            GUILayout.Label("–≈∆»Ã€ ”œ–¿¬À≈Õ»ﬂ", GUI.skin.label);
            GUILayout.Label($"—Ú‡·ËÎËÁ‡ˆËˇ: {(_flightController.IsHoldingAttitude ? "¬ À" : "¬€ À")}");
            GUILayout.Label($"“ÂÍÛ˘‡ˇ ÍÓÏ‡Ì‰‡: {_flightController.CurrentRateCommand}");
            GUILayout.Space(10);
        }

        GUILayout.Label("”œ–¿¬À≈Õ»≈", GUI.skin.label);
        GUILayout.Label("Throttle Up: Shift");
        GUILayout.Label("Throttle Down: Ctrl");
        GUILayout.Label("Afterburner: Alt");
        GUILayout.Label("Pitch Up: W");
        GUILayout.Label("Pitch Down: S");
        GUILayout.Label("Roll Left: A");
        GUILayout.Label("Roll Right: D");
        GUILayout.Label("Yaw Left: Q");
        GUILayout.Label("Yaw Right: E");
        GUILayout.Label("Hold Attitude: H");
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width - 360, 10, 350, 200), GUI.skin.box);
        GUILayout.Label("—“¿“”— —»—“≈Ã", GUI.skin.label);
        GUILayout.Label($"ƒ‚Ë„‡ÚÂÎ¸: {(_engine ? "Œ " : "Õ≈“")}");
        GUILayout.Label($"¿˝Ó‰ËÌ‡ÏËÍ‡: {(_wing ? "Œ " : "Õ≈“")}");
        GUILayout.Label($"”Ô‡‚ÎÂÌËÂ: {(_flightController ? "Œ " : "Õ≈“")}");
        GUILayout.Label($"‘ËÁËÍ‡: {(_aircraft ? "Œ " : "Õ≈“")}");
        GUILayout.EndArea();
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;
        return angle;
    }
}