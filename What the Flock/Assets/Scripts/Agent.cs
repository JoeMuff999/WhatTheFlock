using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/*

    TODO: have agents try to kill player?
    intelligently limit their y movement (really just dont let them crash into the ground)

*/
public class Agent : MonoBehaviour
{
    public struct Vec3{
        float x;
        float y;
        float z;
        public Vec3(float ax, float ay, float az)
        {
            x = ax;
            y = ay;
            z = az;
        }
    }
    public struct BoidDefinition
    {
        public Vector3 position;
        public float heading;
        public Vector3 velocity;
        public int id;

        public BoidDefinition(float x, float y, float heading, float speed)
        {
            position = new Vector2(x, y);
            this.heading = heading;
            this.velocity = new Vector2(speed * Mathf.Cos(Mathf.Deg2Rad * heading), speed * Mathf.Sin(Mathf.Deg2Rad * heading));
            id = -1; //leave unassigned (must be assigned when registered into manager!)
        }
        //I don't like this -> potentially having an unassigned agent id
        public BoidDefinition(BoidDefinition bp)
        {
            this.position = new Vector2(bp.position.x, bp.position.y);
            this.heading = bp.heading;
            this.velocity = bp.velocity;
            this.id = bp.id;
        }
        public BoidDefinition(BoidDefinition bp, int id)
        {

            this.position = new Vector2(bp.position.x, bp.position.y);
            this.heading = bp.heading;
            this.velocity = bp.velocity;
            this.id = id;
        }

        
    public void UpdatePosition(List<BoidDefinition> bds)
    {
        //TODO: move outside of loop
        // if (Vector3.Distance(transform.position, currentGoal) < goalThreshold)
        // {
        //     currentGoal = randomGoal();
        // }
        List<BoidDefinition> neighborhood = getNeighborhood(bds);
        // float deltaAngle = separationWeight * separation(neighborhood) + alignmentWeight * alignment(neighborhood);
        float deltaAngle = 0.0f;
        Vector3 deltaAccel = separationWeight * accelSeparation(neighborhood) + alignmentWeight * accelAlignment(neighborhood) + goalWeight * goalVector();

        deltaAccel = Vector3.ClampMagnitude(deltaAccel, maxAcceleration);
        velocity += deltaAccel;

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Vector3 total_movement = velocity * Time.deltaTime;

        //TODO: move these outside of the loop
        // transform.Translate(total_movement, Space.World);
        // transform.eulerAngles = new Vector3(0, -1 * Mathf.Atan2(velocity.z, velocity.x) * Mathf.Rad2Deg, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
        

        position = new Vector3(transform.position.x, transform.position.y);
        heading = heading + deltaAngle;
        velocity = new Vector3(velocity.x + deltaAccel.x, velocity.y + deltaAccel.y);

    }
    }
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float rotationSpeed;

    [SerializeField]
    private float maxAcceleration;

    public BoidDefinition boidDefinition;

    [SerializeField]
    private float offScreenPadding;


    [Header("Boid Paramters")]

    [SerializeField]
    private float neighborhoodRadius;
    [SerializeField]
    public float separationRadius;
    [SerializeField]
    private float separationWeight;
    [SerializeField]
    private float alignmentWeight;
    [SerializeField]
    private float cohesionWeight;
    [SerializeField]
    private float goalWeight;

    [SerializeField]
    private float OOBCollisionProtection = .5f;

    private bool OOB = false;

    private Vector3 currentGoal;

    public float goalThreshold = 5.0f;


    private void Start()
    {
        boidDefinition = new BoidDefinition(transform.position.x, transform.position.y, transform.eulerAngles.z, maxSpeed);
        AgentManager.RegisterAgent(this);
        currentGoal = randomGoal();
    }

    private List<BoidDefinition> getNeighborhood(List<BoidDefinition> boidDefinitions)
    {
        List<BoidDefinition> neighborhood = new List<BoidDefinition>();
        foreach (BoidDefinition boidDef in boidDefinitions)
        {
            //skip yourself
            if (this.boidDefinition.id == boidDef.id)
                continue;
            if (Vector3.Distance(boidDef.position, this.boidDefinition.position) < neighborhoodRadius)
            {
                neighborhood.Add(boidDef);
            }
        }
        return neighborhood;
    }

    private Vector3 accelAlignment(List<BoidDefinition> neighborhood)
    {
        Vector3 sum = new Vector3(0, 0);
        int count = 0;
        foreach (BoidDefinition boid in neighborhood)
        {
            float d = Vector3.Distance(this.boidDefinition.position, boid.position);
            sum += boid.velocity;
            count++;
        }
        if (count > 0)
        {
            sum /= ((float)count);
            sum.Normalize();
            sum *= maxSpeed; //TODO: figure this out lol
            Vector3 steer = sum - this.boidDefinition.velocity;
            return steer;
        }
        else
        {
            return new Vector3(0, 0);
        }
    }
    //all need to agree who will decelerate
    private Vector3 accelSeparation(List<BoidDefinition> neighborhood)
    {
        Vector3 steer = new Vector3(0, 0);
        int count = 0;
        // For every boid in the neighborhood, check if it's too close
        foreach (BoidDefinition boidDef in neighborhood)
        {
            float d = Vector3.Distance(this.boidDefinition.position, boidDef.position);
            // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
            if (d > 0 && d < separationRadius) //why are checking if distance is greater than 0?
            {
                // Calculate vector pointing away from neighbor
                Vector3 diff = this.boidDefinition.position - boidDef.position;
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
            steer -= this.boidDefinition.velocity;
        }
        return steer;
    }

    private Vector3 goalVector()
    {
        return (currentGoal - transform.position).normalized;
    }


    private Vector3 randomGoal()
    {
        Bounds bounds = AgentManager.GameBounds;
        Vector3 randomGoal = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        return randomGoal;
    }

    //kill the agent
    public void destroy()
    {
        AgentManager.UnregisterAgent(this);
        UIManager.IncreaseScore();
        Destroy(gameObject);
    }

    private float OOB_start_time;
    // private void reflectPosition()
    // {
    //     if (OOBX())
    //     {
    //         OOB_start_time = Time.time;
    //         transform.position = new Vector3(transform.position.x * -1, transform.position.y, 0);
    //     }
    //     else if (OOBY())
    //     {
    //         OOB_start_time = Time.time;
    //         transform.position = new Vector3(transform.position.x, transform.position.y * -1, 0);
    //     }
    //     //should never happen
    //     else
    //     {
    //         Assert.IsTrue(false);
    //     }
    // }

    // private bool isOutOfBounds()
    // {
    //     return OOBX() || OOBY() || OOBZ();
    // }

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

    void OnTriggerEnter(Collider collider)
    {
        if (Time.time - OOB_start_time > OOBCollisionProtection)
            Debug.Log("Collision!");
        else
            Debug.Log("Collision (OOB)");
    }

}