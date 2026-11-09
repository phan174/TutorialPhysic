using UnityEngine;

public class PendulumScript : MonoBehaviour
{
    public float speed = 1.5f;
    public float limit = 75f;
    public bool RandomStart = false;
    private float random = 0;
    
    void Awake()
    {
        if (RandomStart)
        {
            random = Random.Range(0f, 1f);
        }
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * speed + random) * limit;
        transform.localRotation = Quaternion.Euler(angle, 0, 0);
    }
}
