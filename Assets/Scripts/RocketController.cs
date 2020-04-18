using UnityEngine;

public class RocketController : MonoBehaviour
{
    //Rocket thrusters
    Rigidbody mainEngine;
    Rigidbody[] sideThruster;

    //Particle animation
    ParticleSystem.MainModule engineFire;

    //Neural Network
    public NeuralNetwork controller;
    public bool initilized;
    public float penalty; //similar to fitness but lower is better
    
    //Link to other script
    GenerationManager generationManager;

    void Start()
    {
        //Find Engine 
        mainEngine = transform.Find("Engine").GetComponent<Rigidbody>();
        engineFire = mainEngine.GetComponentInChildren<ParticleSystem>().main;
        engineFire.startLifetime = 0;

        //Find thruster
        sideThruster = new Rigidbody[4];
        sideThruster[0] = transform.Find("Top/T1").GetComponent<Rigidbody>();
        sideThruster[1] = transform.Find("Top/T2").GetComponent<Rigidbody>();
        sideThruster[2] = transform.Find("Top/T3").GetComponent<Rigidbody>();
        sideThruster[3] = transform.Find("Top/T4").GetComponent<Rigidbody>();

        //Find GenerationManager
        generationManager = GameObject.Find("Main Camera").GetComponent<GenerationManager>();
    }

    public void Deactivate()
    {
        initilized = false;
        engineFire.startLifetime = 0;
    }

    void Update()
    {
        //Rocket out of bounds
        if (transform.position.magnitude > 80)
        {
            generationManager.Deactivate(transform.root.GetChild(1).gameObject);
            AddPenalty(100f);
        }

        if (initilized)
        {
            FlyRocket();
        }
    }

    public void ActivateNeuralNetwork(NeuralNetwork n)
    {
        controller = n;
        initilized = true;
        penalty = 0f;
    }

    //Use neutral network
    public void FlyRocket()
    {
        float[] input = new float[4];
        input[0] = transform.rotation.w;
        input[1] = transform.rotation.x;
        input[2] = transform.rotation.y;
        input[3] = transform.rotation.z; 

        //Fire neurons
        float[] output = controller.FireNeurons(input);
        ThrustSide(0, output[0]);
        ThrustSide(1, output[1]);
        ThrustSide(2, output[2]);
        ThrustSide(3, output[3]);
        ThrustMainEngine(output[4]);
    }

    public void AddPenalty(float x)
    {
        penalty += x;
    }

    public void ThrustMainEngine(float power)
    {
        mainEngine.AddRelativeForce(Vector3.up * Mathf.Clamp(1000 * power, 0, 500));
        engineFire.startLifetime = Mathf.Clamp(100 * power, 0.2f, 1);
    }


    public void ThrustSide(int thruster, float power)
    {
        sideThruster[thruster].AddRelativeForce(Vector3.up * Mathf.Clamp(50 * power, 0, 50));
        ParticleSystem.MainModule thrusterFire = sideThruster[thruster].GetComponentInChildren<ParticleSystem>().main;
        thrusterFire.startLifetime = Mathf.Clamp(10 * power, 0.2f, 1);
    }
}
