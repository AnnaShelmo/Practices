using UnityEngine;

public class CarTelemetry : MonoBehaviour
{
    public Rigidbody rb;
    public KartAero aero;
    public CarSuspension suspension;

    private void OnGUI()
    {
        if (rb == null || aero == null || suspension == null)
            return;

        GUI.Label(new Rect(10, 10, 300, 20), $"Speed: {aero.speedMS:F1} m/s ({aero.speedKMH:F1} km/h)");
        GUI.Label(new Rect(10, 30, 300, 20), $"Drag: {aero.dragForceValue:F0} N");
        GUI.Label(new Rect(10, 50, 300, 20), $"Downforce: {aero.downforceValue:F0} N");
        GUI.Label(new Rect(10, 70, 300, 20), $"Ground Effect: {aero.groundEffectForce:F0} N");

        GUI.Label(new Rect(10, 90, 300, 20), $"DRS: {(aero.drsActive ? "ON" : "OFF")}");
        GUI.Label(new Rect(10, 110, 300, 20), $"Wing Angle: {(aero.drsActive ? aero.drsWingAngle : aero.normalWingAngle)}°");

        Vector3 comWorld = rb.worldCenterOfMass;
        GUI.Label(new Rect(10, 130, 300, 20), $"COM Height: {comWorld.y:F2} m");
    }
}
