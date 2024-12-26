using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    private BehaviorParameters behaviorParameters;
    private CarController carController;
    private Rigidbody carControllerRigidBody;
    private CarSpots carSpots;

    private float timeSinceLastAction = 0f;
    private bool isActionActive = false;

    private float previousDistanceToGoal;

    [SerializeField]
    private Material standartMaterial = null;

    [SerializeField]
    private Material successMaterial = null;

    [SerializeField]
    private Material failureMaterial = null;

    [SerializeField]
    private MeshRenderer groundMesh;

    public override void Initialize()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
        carController = GetComponent<CarController>();
        carControllerRigidBody = carController.GetComponent<Rigidbody>();
        carSpots = transform.parent.GetComponentInChildren<CarSpots>();

        ResetParkingLotArea();
    }

    public override void OnEpisodeBegin()
    {
        ResetParkingLotArea();
    }

    private void ResetParkingLotArea()
    {
        carController.IsAutonomous = behaviorParameters.BehaviorType == BehaviorType.Default;
        transform.localPosition = new Vector3(Random.Range(-900, 900) / 100, 1.5f, -5.5f);
        transform.localRotation = Quaternion.Euler(0, Random.Range(-1500, 1500) / 100, 0);
        carControllerRigidBody.linearVelocity = Vector3.zero;
        carControllerRigidBody.angularVelocity = Vector3.zero;

        carSpots.Setup();
        previousDistanceToGoal = Vector3.Distance(transform.localPosition, carSpots.CarGoal.transform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation);

        sensor.AddObservation(carSpots.CarGoal.transform.position);

        sensor.AddObservation(carControllerRigidBody.linearVelocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var direction = Mathf.FloorToInt(actionBuffers.DiscreteActions[0]);
        isActionActive = direction != 0; // Если действия выполняются, то isActionActive = true

        switch (direction)
        {
            case 0: // no action
                carController.CurrentDirection = CarController.Direction.None;
                break;
            case 1: // forward
                carController.CurrentDirection = CarController.Direction.MoveForward;
                break;
            case 2: // backward
                carController.CurrentDirection = CarController.Direction.MoveBackward;
                break;
            case 3: // turn left
                carController.CurrentDirection = CarController.Direction.TurnLeft;
                break;
            case 4: // turn right
                carController.CurrentDirection = CarController.Direction.TurnRight;
                break;
        }

        if (isActionActive)
        {
            timeSinceLastAction = 0f;
        }
        else
        {
            timeSinceLastAction += Time.deltaTime;
        }

        AddReward(-1f / MaxStep);

        // Добавить награду за приближение к цели
        float currentDistanceToGoal = Vector3.Distance(transform.localPosition, carSpots.CarGoal.transform.localPosition);
        if (currentDistanceToGoal < previousDistanceToGoal)
        {
            AddReward(0.01f); // Награда за приближение
        }
        else
        {
            AddReward(-0.01f); // Наказание за отдаление от цели
        }
        previousDistanceToGoal = currentDistanceToGoal;
    }

    public float GetTimeSinceLastAction()
    {
        return timeSinceLastAction;
    }

    public void GivePoints(float amount = 1.0f, bool isFinal = false)
    {
        AddReward(amount);

        if (isFinal)
        {
            StartCoroutine(SwapGroundMaterial(successMaterial, 1f));
            EndEpisode();
        }
    }

    public void TakeAwayPoints(float reward, bool isFinal = false)
    {
        AddReward(-reward);

        if (isFinal)
        {
            StartCoroutine(SwapGroundMaterial(failureMaterial, 1f));
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && carController.canApplyTorque())
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && carController.canApplyTorque())
        {
            discreteActionsOut[0] = 4;
        }
    }

    private System.Collections.IEnumerator SwapGroundMaterial(Material material, float time)
    {
        if (groundMesh != null)
        {
            groundMesh.material = material;
        }
        yield return new WaitForSecondsRealtime(time);
        if (groundMesh != null)
        {
            groundMesh.material = standartMaterial; // Set this to your original material if needed
        }
    }
}
