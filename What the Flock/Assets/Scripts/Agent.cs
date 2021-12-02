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
        foreach(BoidDefinition boid in neighborhood)
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

    public void UpdatePosition(List<BoidDefinition> bds)
    {

        if(Vector3.Distance(transform.position, currentGoal) < goalThreshold)
        {
            currentGoal = randomGoal();
        }
        List<BoidDefinition> neighborhood = getNeighborhood(bds);
        // float deltaAngle = separationWeight * separation(neighborhood) + alignmentWeight * alignment(neighborhood);
        float deltaAngle = 0.0f;
        Vector3 deltaAccel = separationWeight * accelSeparation(neighborhood) + alignmentWeight * accelAlignment(neighborhood) + goalWeight * goalVector();
        // float deltaAngle = separationWeight * separation(neighborhood);

        // float deltaAngle = alignmentWeight * alignment(neighborhood) + separationWeight * separation(neighborhood) + cohesionWeight * cohesion(neighborhood);
        // float totalRotation = rotationSpeed * Time.deltaTime;

        // deltaAngle = Mathf.Clamp(deltaAngle, -1 * totalRotation, totalRotation);

        // Debug.Log("seapration = " + separation(neighborhood));
        // Debug.Log(deltaAngle);
        // transform.Rotate(0, 0, deltaAngle, Space.World);
        // float total_movement = movementSpeed * Time.deltaTime;
        deltaAccel = Vector3.ClampMagnitude(deltaAccel, maxAcceleration);
        this.boidDefinition.velocity += deltaAccel;

        this.boidDefinition.velocity = Vector3.ClampMagnitude(this.boidDefinition.velocity, maxSpeed);
        Vector3 total_movement = this.boidDefinition.velocity * Time.deltaTime;
        //check out of bounds and then restrict movement
        // if(OOBX(transform.position + total_movement))
        // {
        //     total_movement.x = 0;
        // }
        // if(OOBY(transform.position + total_movement))
        // {
        //     total_movement.y = 0;
        //     this.boidDefinition.velocity.y = -this.boidDefinition.velocity.y; 
        // }
        // if(OOBZ(transform.position + total_movement))
        // {
        //     total_movement.z = 0;
        // }
        // Debug.Log(this.boidDefinition.velocity);
        // Debug.Log(deltaAccel);


        transform.Translate(total_movement, Space.World);
        transform.eulerAngles = new Vector3(0,  -1 * Mathf.Atan2(this.boidDefinition.velocity.z, this.boidDefinition.velocity.x) * Mathf.Rad2Deg, Mathf.Atan2(this.boidDefinition.velocity.y, this.boidDefinition.velocity.x) * Mathf.Rad2Deg);
        // transform.Translate(total_movement * Vector3.right);


        this.boidDefinition.position = new Vector3(transform.position.x, transform.position.y);
        this.boidDefinition.heading = this.boidDefinition.heading + deltaAngle;
        this.boidDefinition.velocity = new Vector3(this.boidDefinition.velocity.x + deltaAccel.x, this.boidDefinition.velocity.y + deltaAccel.y);

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
    public void destroy() {
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
        return new_pos.z < bounds.min.z  || new_pos.z > bounds.max.z ;
    }

    void OnTriggerEnter(Collider collider)
    {
        if(Time.time - OOB_start_time > OOBCollisionProtection)
            Debug.Log("Collision!");
        else
            Debug.Log("Collision (OOB)");
    }

}