using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerationManager : MonoBehaviour
{
    //Rocket
    public List<RocketController> rocketController;
    public List<int> rocketInstances;
    public GameObject rocketPrefab;
    public List<RocketController> lastGeneration;

    //Training units
    public GameObject[] trainingUnits;

    //Evolvement variables
    public int generationNumber;
    public int deadCount;
    public int populationSize;

    void Start()
    {
        generationNumber = 1;
        StartGeneration();
        Debug.Log("Generation: " + generationNumber);
    }

    void Update()
    {
        //Sort by penalty (=inverse fitness)
        rocketController.Sort(SortByPenalty);

        if (deadCount == populationSize)
        {
            CleanRockets();
            StartGeneration();
        }
    }

    public void StartGeneration()
    {
        FindTrainingUnits();
        InstantiateRockets();
        InitializeNeuralNetwork();
    }

    public void Deactivate(GameObject rocket)
    {
        int id = rocket.GetInstanceID();

        //Check if rocket was not deactivated yet
        if (!rocketInstances.Contains(id))
        {
            rocketInstances.Add(id);
            deadCount++;
            DisplayStats();
        }     
    }

    public void FindTrainingUnits()
    {
        trainingUnits = GameObject.FindGameObjectsWithTag("TrainingUnit");
        populationSize = trainingUnits.Length;
        DisplayStats();
    }


    public void InstantiateRockets()
    {
        //Loop through all TrainingUnits
        for (int i = 0; i < trainingUnits.Length; i++)
        {
            RocketController newRocketController = Instantiate(rocketPrefab, trainingUnits[i].transform, false).GetComponent<RocketController>();
            rocketController.Add(newRocketController);
        }
    }

    public void InitializeNeuralNetwork()
    {
        //Loop through all RocketControllers
        for (int i = 0; i < rocketController.Count; i++)
        {
            rocketController[i].ActivateNeuralNetwork(GetMutatedNeuralNetwork(i));
        }
    }

    public NeuralNetwork GetMutatedNeuralNetwork(int i)
    {
        if (generationNumber == 1) //Initialize new random network
        {
            return new NeuralNetwork(new int[] { 4, 10, 10, 5 });
        }

        else //Use best network from last generation
        {
            if (i == 0)
            {
                //No mutation
                Debug.Log("Generation " + generationNumber + ": Penalty " + lastGeneration[0].penalty);
                return new NeuralNetwork(lastGeneration[0].controller, 0);
            }

            if (i < populationSize * 0.25)
            {
                //Little mutation chance
                return new NeuralNetwork(lastGeneration[0].controller, 0.01f);   
            }

            if (i < populationSize * 0.50)
            {   //Higher mutation chance
                return new NeuralNetwork(lastGeneration[0].controller, 0.02f);
            }

            //Complete new network to keep population diverse
            return new NeuralNetwork(new int[] { 4, 10, 10, 5 });
        }
    }

    public void CleanRockets()
    {
        //Backup last generation
        lastGeneration = rocketController;

        //Next generation
        generationNumber++;
        deadCount = 0;

        rocketInstances = new List<int>();

        for (int i = 0; i < rocketController.Count; i++)
        {
            Destroy(rocketController[i].gameObject);
        }

        rocketController = new List<RocketController>();
    }

    //Sorting RocketControllers, lower penalty is better
    public int SortByPenalty(RocketController a, RocketController b)
    {
        return (a.penalty.CompareTo(b.penalty));
    }

    public void DisplayStats()
    {
        GameObject.Find("GenerationNumber").GetComponent<Text>().text = "Generation: " + generationNumber.ToString();
        GameObject.Find("AliveRockets").GetComponent<Text>().text = "Alive rockets: " + (populationSize - deadCount).ToString();
    }

}
