using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Mathematics;
/*

    TODO: have agents try to kill player?
    intelligently limit their y movement (really just dont let them crash into the ground)

*/
public class Agent : MonoBehaviour
{
    [System.Serializable]
    public struct BoidDefinition
    {
        [HideInInspector]
        public Vector3 position;
        [HideInInspector]
        public Vector3 velocity;
        [HideInInspector]
        public int id;
        [HideInInspector]
        public Vector3 currentGoal;

        public Vector3 PlayerPosition;

        [Header("Agent Parameters")]
        public float maxSpeed;
        public float maxAcceleration;
        public float TargetPlayerChance;
        public float TargetPlayerTimer;

        private float currentPlayerTargetTimer;

        [Header("Boid Parameters")]
        public float neighborhoodRadius;
        public float separationRadius;
        public float separationWeight;
        public float alignmentWeight;
        public float cohesionWeight;
        public float goalWeight;
        public float goalThreshold;



        private bool wentOOB;

        private bool attackingPlayer;


        public Unity.Mathematics.Random randomRef;

        private Vector3 getRandomGoal()
        {
            Bounds b = AgentManager.GameBounds;
            float x = randomRef.NextFloat(b.min.x, b.max.x);
            float y = randomRef.NextFloat(b.min.y, b.max.y);
            float z = randomRef.NextFloat(b.min.z, b.max.z);
            return new Vector3(x, y, z);
        }

        private bool shouldAttackPlayer()
        {
            if(currentPlayerTargetTimer < 0)
            {
                currentPlayerTargetTimer = TargetPlayerTimer;
                float x = randomRef.NextFloat(0, 1);
                if(x <= TargetPlayerChance)
                {
                    currentGoal = PlayerPosition;
                    return true;
                }
            }
            return false;
        }

        public void UpdatePosition(NativeArray<Agent.BoidDefinition> bds, float deltaTime)
        {
            currentPlayerTargetTimer -= deltaTime;

            Vector3 deltaAccel;
            //if OOB, go towards goal vector until reached
            if(attackingPlayer)
            {
                deltaAccel = goalVector() * goalWeight;

            }
            else if (wentOOB)
            {
                deltaAccel = goalVector() * goalWeight;
                if (Vector3.Distance(position, currentGoal) < goalThreshold)
                {
                    wentOOB = false;
                    currentGoal = getRandomGoal();
                }
            }
            else
            {
                List<BoidDefinition> neighborhood = getNeighborhood(bds);
                deltaAccel = separationWeight * accelSeparation(neighborhood) + alignmentWeight * accelAlignment(neighborhood);
            }


            deltaAccel = Vector3.ClampMagnitude(deltaAccel, maxAcceleration);
            velocity += deltaAccel;

            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            Vector3 total_movement = velocity * deltaTime;

            // Debug.Log(deltaTime);

            position = new Vector3(position.x + total_movement.x, position.y + total_movement.y, position.z + total_movement.z);
            velocity = new Vector3(velocity.x + deltaAccel.x, velocity.y + deltaAccel.y, velocity.z + deltaAccel.z);

            if (isOOB(position))
            {
                wentOOB = true;
            }
            if (shouldAttackPlayer())
            {
                attackingPlayer = true;
            }

        }

        private List<BoidDefinition> getNeighborhood(NativeArray<BoidDefinition> boidDefinitions)
        {
            List<BoidDefinition> neighborhood = new List<BoidDefinition>();
            for (int i = 0; i < boidDefinitions.Length; ++i)
            {
                BoidDefinition boidDef = boidDefinitions[i];
                //skip yourself
                if (id == boidDef.id || boidDef.id == -1)
                    continue;
                if (Vector3.Distance(boidDef.position, position) < neighborhoodRadius)
                {
                    neighborhood.Add(boidDef); //its a copy! maybe we can find a cheaper way to do this :)
                }
            }
            return neighborhood;
        }

        private Vector3 accelAlignment(List<BoidDefinition> neighborhood)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (BoidDefinition boid in neighborhood)
            {
                float d = Vector3.Distance(position, boid.position);
                sum += boid.velocity;
                count++;
            }
            if (count > 0)
            {
                sum /= ((float)count);
                sum.Normalize();
                sum *= maxSpeed; //TODO: figure this out lol
                Vector3 steer = sum - velocity;
                return steer;
            }
            else
            {
                return Vector3.zero;
            }
        }

        private Vector3 accelSeparation(List<BoidDefinition> neighborhood)
        {
            Vector3 steer = Vector3.zero;
            int count = 0;
            // For every boid in the neighborhood, check if it's too close
            foreach (BoidDefinition boidDef in neighborhood)
            {
                float d = Vector3.Distance(position, boidDef.position);
                // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
                if (d > 0 && d < separationRadius) //why are checking if distance is greater than 0?
                {
                    // Calculate vector pointing away from neighbor
                    Vector3 diff = position - boidDef.position;
                    diff.Normalize();
                    diff /= d;
                    steer += diff;
                    count++;
                }
            }
            // Take the Average
            if (count > 0)
            {
                steer /= ((float)count);
            }

            // As long as the vector is greater than 0
            if (steer.magnitude > 0)
            {
                steer.Normalize();
                steer *= maxSpeed;
                steer -= velocity;
            }
            return steer;
        }
        private Vector3 goalVector()
        {
            return (currentGoal - position).normalized;
        }

        private float OOB_start_time;

        private bool isOOB(Vector3 new_pos)
        {
            return OOBX(new_pos) || OOBY(new_pos) || OOBZ(new_pos);
        }

        private bool OOBX(Vector3 new_pos)
        {
            Bounds bounds = AgentManager.GameBounds;

            return new_pos.x < bounds.min.x || new_pos.x > bounds.max.x;
        }

        private bool OOBY(Vector3 new_pos)
        {
            Bounds bounds = AgentManager.GameBounds;
            return new_pos.y < bounds.min.y || new_pos.y > bounds.max.y;
        }

        private bool OOBZ(Vector3 new_pos)
        {
            Bounds bounds = AgentManager.GameBounds;
            // print(bounds);
            return new_pos.z < bounds.min.z || new_pos.z > bounds.max.z;
        }
    }


    public BoidDefinition boidDefinition;

    // [SerializeField]
    // private float offScreenPadding;

    // [SerializeField]
    // private float OOBCollisionProtection = .5f;

    // private bool OOB = false;
    private void Start()
    {
        boidDefinition.position = transform.position;
        boidDefinition.velocity = Vector3.forward; //TODO: more interesting initial velocity!
        boidDefinition.id = -1;
        boidDefinition.currentGoal = AgentManager.getRandomGoal();
        AgentManager.RegisterAgent(this);
    }



    //kill the agent
    public void destroy()
    {
        if(boidDefinition.id == -1) //already destroyed, but can still be hit LOL
            return;
        AgentManager.UnregisterAgent(this);
        UIManager.IncreaseScore();
        gameObject.GetComponentInChildren<Renderer>().enabled = false; //dont destroy because i need it to stay alive until i figure out garbage collection :D... shhh keep it a secret :P
        // Destroy(gameObject);
    }



    void OnTriggerEnter(Collider collider)
    {
        // if (Time.time - OOB_start_time > OOBCollisionProtection)
        //     Debug.Log("Collision!");
        // else
        //     Debug.Log("Collision (OOB)");
    }

}