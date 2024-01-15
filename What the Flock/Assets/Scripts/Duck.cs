using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duck : MonoBehaviour
{
    public float DuckSpeed;
    public float DestroyTimer;
    private float currentTimer;
    public float PitchMin;
    public float PitchMax;

    public MeshRenderer me;

    public Vector3 PlayerPosition;

    public AudioSource quack;
    // Start is called before the first frame update
    void Start()
    {
        currentTimer = DestroyTimer;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * DuckSpeed * Time.deltaTime;
        currentTimer -= Time.deltaTime;
        if(currentTimer < 0)
            Destroy(gameObject);
    }

    private IEnumerator killPlane(GameObject agent)
    {
        // transform.position = PlayerPosition;
        quack.pitch = Random.Range(PitchMin, PitchMax);
        quack.Play();
        agent.GetComponentInParent<Agent>().destroy();//destroy agent
        me.enabled = false;
        yield return new WaitForSeconds(quack.clip.length);

        Destroy(gameObject); //destroy yourself
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            StartCoroutine(killPlane(other.gameObject));
        }
    }

    private void OnDestroy() {
        // Debug.Log("destroyed duck");
    }
}
