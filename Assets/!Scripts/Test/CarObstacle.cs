using UnityEngine;

public class CarObstacle : MonoBehaviour
{
    public enum CarObstacleType
    { 
        Barrier,
        Tree,
        Car,
        Ground
    }

    [SerializeField]
    private CarObstacleType carObstacleType = CarObstacleType.Barrier;

    public CarObstacleType CarObstacleTypeValue { get { return this.carObstacleType; } }

    private CarAgent agent = null;

    void Awake()
    {
        // cache agent
        agent = transform.parent.parent.GetComponentInChildren<CarAgent>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.tag.ToLower() == "player")
        {

            Debug.Log("HIT SOMETHING");
            agent.TakeAwayPoints(1);
        }
    }

    void OnCollisionEnter(Collision other) 
    {
        if (other.transform.tag.ToLower() == "player")
        {
            Debug.Log("HIT SOMETHING");

            //Debug.Log(gameObject.name);
            agent.TakeAwayPoints(1000,true);
        }
    }
}
