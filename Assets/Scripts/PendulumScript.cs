using UnityEngine;

public class PendulumScript : MonoBehaviour
{
    [Header("Set Angle and Speed")]
    public float speed = 2.5f;          
    public float initialAngle = 75f;    
    
    [Header("Damping / Friction")]
    //Damping factor (the higher, the faster it stops). Example: 0.1 to 0.5
    public float damping = 0.2f;        
    
    public bool randomStart = false;

    private float currentAmplitude;
    private float timer = 0f;

    void Start()
    {
        currentAmplitude = initialAngle;
        if (randomStart)
        {
            timer = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        //Calculate the current amplitude based on damping
        currentAmplitude = initialAngle * Mathf.Exp(-damping * timer);

        //Calculate the angle based on sine wave and current amplitude
        float angle = Mathf.Sin(timer * speed) * currentAmplitude;

        //Apply the rotation to the pendulum
        transform.localRotation = Quaternion.Euler(angle, 0, 0);
    }

    // Function called when wanting to activate/push the pendulum from outside
    public void Push(float newAngle)
    {
        initialAngle = newAngle;
        currentAmplitude = newAngle;
        timer = 0f;
        enabled = true;
    }
}