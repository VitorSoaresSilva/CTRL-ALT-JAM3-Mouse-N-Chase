using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]

//By FJB
public class EngineSound : MonoBehaviour
{
    public _Developers.Vitor.CarFollowPath carFollowPath; 
    public float minPitch = 0.5f;
    public float maxPitch = 2.0f;
    public float maxSpeed = 20.0f;

    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float speed = carFollowPath.speed;
        float pitch = Mathf.Lerp(minPitch, maxPitch, speed / maxSpeed);
        audioSource.pitch = pitch;
    }
}




//BASE
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(AudioSource))]
//public class EngineSound : MonoBehaviour
//{
//    public float minPitch = 0.5f;
//    public float maxPitch = 2.0f;
//    public float maxSpeed = 20.0f;

//    private Rigidbody rb;
//    private AudioSource audioSource;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        audioSource = GetComponent<AudioSource>();
//    }

//    void Update()
//    {
//        float speed = rb.velocity.magnitude;
//        float pitch = Mathf.Lerp(minPitch, maxPitch, speed / maxSpeed);
//        audioSource.pitch = pitch;
//    }
//}
