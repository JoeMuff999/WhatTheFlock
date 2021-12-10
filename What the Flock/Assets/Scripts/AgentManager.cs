using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Jobs;
using Unity.Collections;



/*
    High-level manager of the game's AI
    Maintains a list of all agents in the game
    Responsible for telling the agents when to update their values. 
    Since Agent updates only change internal values (do not directly modify the gameObject), the AgentManager is also responsible for updating the Agent's GameObject in the world.
    Includes both a sequential update method and a parallel update method which can be changed with "ParallelizationEnabled" in the editor
*/
public class AgentManager : MonoBehaviour
{
    private static List<Agent> agents;
    private static NativeArray<Agent.BoidDefinition> currentAgentStates;

    public int NumBoids;


    //used to give a unique id to each agent in the sim
    private static int agentCount = 0;

    public float SpawnHeightMin = 0;
    public float SpawnHeightMax = 0;

    public GameObject TopLeftBoundary;
    public GameObject BottomRightBoundary;

    public static Bounds GameBounds;

    public bool ParallelizationEnabled;


    /*
        For debugging
        Draws spheres in the editor where the boundaries for the agents are
    */
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(TopLeftBoundary.transform.position, 5);
        Gizmos.DrawSphere(BottomRightBoundary.transform.position, 5);
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position + new Vector3(0, SpawnHeightMin, 0), 5);
        Gizmos.DrawSphere(transform.position + new Vector3(0, SpawnHeightMax, 0), 5);
    }

    public GameObject BoidPrefab;

    /*
        Parallel update job for our agents
        Each agent will have its update method added to a job queue
    */
    public struct AgentUpdateJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Agent.BoidDefinition> currentAgentStates; //needs to be readonly since we are accessing concurrently 
        public NativeArray<Agent.BoidDefinition> updatedAgentStates;
        public float deltaTime;

        public void Execute(int idx)
        {
            var data = currentAgentStates[idx]; //gets the value, not the reference
            data.UpdatePosition(currentAgentStates, deltaTime);
            updatedAgentStates[idx] = data;
        }
    }


    private void Awake()
    {
        agents = new List<Agent>();
        currentAgentStates = new NativeArray<Agent.BoidDefinition>(NumBoids, Allocator.Persistent);
        GameBounds = new Bounds();
        GameBounds.SetMinMax(new Vector3(BottomRightBoundary.transform.position.x, SpawnHeightMin, BottomRightBoundary.transform.position.z), new Vector3(TopLeftBoundary.transform.position.x, SpawnHeightMax, TopLeftBoundary.transform.position.z));
    }

    private void Start()
    {
        SpawnBoids();
    }

    /*
        Responsible for spawning boids on startup
        Spawns boids within the boundaries defined in the editor ("TopLeftBoundary" and "BottomRightBoundary"; "SpawnHeightMin" and "SpawnHeightMax")
        Does so "safely" by making sure each Boid is initalized at least some minimum distance away from neighboring boids
    */
    private void SpawnBoids()
    {
        float min_separation = BoidPrefab.GetComponent<Agent>().boidDefinition.separationRadius * 2;
        int max_attempts = 40;
        int boids_spawned = 0;

        List<Vector3> placedSoFar = new List<Vector3>();
        for (int i = 0; i < NumBoids; i++)
        {
            int current_attempts = 0;

            bool foundSpot = false;
            float initial_x = 0;
            float initial_y = 0;
            float initial_z = 0;
            while (!foundSpot && current_attempts < max_attempts)
            {
                initial_x = Random.Range(BottomRightBoundary.transform.position.x, TopLeftBoundary.transform.position.x);
                initial_y = Random.Range(SpawnHeightMin, SpawnHeightMax);
                initial_z = Random.Range(BottomRightBoundary.transform.position.z, TopLeftBoundary.transform.position.z);
                Vector3 current = new Vector3(initial_x, initial_y, initial_z);
                foundSpot = true;
                foreach (Vector3 toCheck in placedSoFar)
                {
                    if (Vector3.Distance(toCheck, current) < min_separation)
                    {
                        foundSpot = false;
                        break;
                    }
                }
                current_attempts++;
            }
            if (foundSpot)
            {
                Quaternion intial_rotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f));
                GameObject go = Instantiate(BoidPrefab, new Vector3(initial_x, initial_y, initial_z), intial_rotation);
                placedSoFar.Add(new Vector3(initial_x, initial_y, initial_z));
                boids_spawned++;
            }
        }
        Debug.Log("Boids spawned " + boids_spawned);
    }

    public static Vector3 getRandomGoal()
    {
        Bounds bounds = AgentManager.GameBounds;
        Vector3 randomGoal = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        return randomGoal;
    }

    /*
        Each agent registers itself with the manager
        Upon doing so, it is given a unique id. It is also added to the AgentManager's list of agents
    */
    public static void RegisterAgent(Agent agent)
    {
        agent.boidDefinition.id = agentCount;//assign id here
        agent.boidDefinition.randomRef = new Unity.Mathematics.Random((uint)(agentCount + 1));

        agents.Add(agent);
        currentAgentStates[agentCount] = agent.boidDefinition; //NOTE: this is a copy b/c structs are value types, https://jonskeet.uk/csharp/references.html
        // Debug.Log(currentStates[currentStates.Count-1].id);
        agentCount++;
    }

    /*
        On death, agents will unregister themselves from the manager
        They are given an id of -1 and they will no longer be updated and/or be considered in future updates
    */
    public static void UnregisterAgent(Agent agent)
    {
        // agents.Remove(agent);
        var a = currentAgentStates[agent.boidDefinition.id]; //NOTE: this is a potential concurrency error. you may need to move this such that a queue is created for unregistering which will then occur only after all of the updates have finished (i.e.in the main loop)
        a.id = -1; //invalidate their id 
        currentAgentStates[agent.boidDefinition.id] = a;
        //NOTE: agents that are dead will have an id of -1. places of concern: main loop (do not update states of dead agents), neighborhood checks (do not check against neighbors that are dead)
        //cant remove agent from agent list because then size of agents will be smaller than size of currentAgentStates. TODO: if you have time, come up with a better solution to this.
    }
    /*
        Core update loop for our agents
        Schedules a job for each agent's update
        Parallelized using Unity's Job system
    */
    private void parallelUpdate()
    {
        NativeArray<Agent.BoidDefinition> updatedAgentStates = new NativeArray<Agent.BoidDefinition>(agents.Count, Allocator.TempJob);

        // Set up the job data
        AgentUpdateJob jobData = new AgentUpdateJob();
        jobData.currentAgentStates = currentAgentStates;
        jobData.updatedAgentStates = updatedAgentStates;
        jobData.deltaTime = Time.deltaTime;

        // Schedule the job
        JobHandle handle = jobData.Schedule(agents.Count, 1);

        // Wait for the job to complete
        handle.Complete();
        //safe copy, idk how c# works so just trying to be sure
        NativeArray<Agent.BoidDefinition>.Copy(updatedAgentStates, currentAgentStates);
        //copy the output of the jobs back into the agent states 
        for (int i = 0; i < currentAgentStates.Length; ++i)
        {
            agents[i].boidDefinition = currentAgentStates[i];
        }
        updatedAgentStates.Dispose();
    }

    /*
        *Non-parallel* (depracated, only here for demonstration purposes)
        Core update loop for our agents
    */
    private void defaultUpdate()
    {
        foreach (Agent agent in agents)
        {
            //skip dead agents
            if (agent.boidDefinition.id == -1)
                continue;
            agent.boidDefinition.UpdatePosition(currentAgentStates, Time.deltaTime);
        }
        //copy over updated states
        for (int i = 0; i < agents.Count; ++i)
        {
            Agent agent = agents[i];
            //dont skip dead agents. so this is weird. see unregister agent for more information
            currentAgentStates[i] = agent.boidDefinition;
        }
    }

    private void Update()
    {
        if (ParallelizationEnabled)
        {
            parallelUpdate();
        }
        else
        {
            defaultUpdate();
        }
        //move each agent, cant parallelize unfortunately
        foreach (Agent a in agents)
        {
            if (a.boidDefinition.id == -1) //skip dead agents
                continue;

            //move visual representation
            Transform t = a.transform;
            t.position = a.boidDefinition.position;
            if (Vector3.Distance(t.position, a.boidDefinition.PlayerPosition) < .5f)
            {
                //kill player.
                UIManager.Death();
            }
            Vector3 velocity = a.boidDefinition.velocity;

            // t.rotation = new Vector3(0, -1 * Mathf.Atan2(velocity.z, velocity.x) * Mathf.Rad2Deg, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            t.rotation = Quaternion.LookRotation(velocity.normalized);


        }

    }

    private void OnDestroy()
    {
        currentAgentStates.Dispose();
    }
}