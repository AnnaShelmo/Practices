using UnityEngine;

public class Target : MonoBehaviour
{
    private float _mass;
    private float _radius;

    public void SetParams(float mass, float radius)
    {
        _mass = mass;
        _radius = radius;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            ScoreCounter.AddHit();
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
    }
}
