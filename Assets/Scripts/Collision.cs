using UnityEngine;

public class Collision : MonoBehaviour
{
    GenerationManager generationManager;

    void Start()
    {
        generationManager = GameObject.Find("Main Camera").GetComponent<GenerationManager>();
    }

    public void OnCollisionEnter(UnityEngine.Collision collision)
    {
        //Collision parent and collision object name
        GameObject parent = collision.gameObject.transform.root.GetChild(1).gameObject;
        string partName = collision.gameObject.name;

        //Impact force
        float impact = collision.relativeVelocity.magnitude;

        //If not landing on legs, kill rocket
        if (collision.gameObject.tag != "Leg")
        {
            //Visual explosionforce
            collision.gameObject.GetComponent<Rigidbody>().AddExplosionForce(30f * impact, collision.gameObject.transform.position, 30f);

            //Add fitness penalty on collion
            parent.GetComponent<RocketController>().AddPenalty(impact);

            //Deactivate rocket
            generationManager.Deactivate(parent);
            parent.GetComponent<RocketController>().Deactivate();
        }

        //If hard impact, break rocket
        if (partName == "RocketPrefab(Clone)" && impact > 20)
        {
            Destroy(collision.gameObject.GetComponent<FixedJoint>());
            Destroy(collision.gameObject.GetComponent<MeshRenderer>());
        }

    }

}

