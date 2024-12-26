using UnityEngine;
using System.Collections;

public class CarGoal : MonoBehaviour
{
    private CarAgent agent = null;

    [SerializeField]
    private GoalType goalType = GoalType.Milestone;

    [SerializeField]
    private float goalReward = 0.1f;

    [SerializeField]
    private bool enforceGoalMinRotation = false;

    [SerializeField]
    private float goalMinRotation = 10.0f;

    [SerializeField]
    private float requiredStayTime = 2.0f;

    [SerializeField]
    private float stayReward = 0.01f;

    [SerializeField]
    private float rewardInterval = 1.0f;

    [SerializeField]
    private float minSpeedForReward = 0.1f;

    [SerializeField]
    private float firstEntryReward = 0.2f;

    public enum GoalType
    {
        Milestone,
        FinalDestination
    }

    private Coroutine stayCoroutine;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            agent = collider.GetComponent<CarAgent>();
            if (agent != null)
            {
                Debug.Log("ENTER");
                agent.GivePoints(firstEntryReward);

                if (stayCoroutine != null)
                {
                    StopCoroutine(stayCoroutine);
                }
                stayCoroutine = StartCoroutine(StayInTrigger(agent));
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            //Debug.Log("EXIT");

            if (stayCoroutine != null)
            {
                StopCoroutine(stayCoroutine);
                stayCoroutine = null;
            }

            if (agent != null)
            {
                agent.TakeAwayPoints(firstEntryReward);
            }
        }
    }

    private IEnumerator StayInTrigger(CarAgent agent)
    {
        Rigidbody rb = agent.GetComponent<Rigidbody>();

        float elapsedTime = 0.0f; // Время нахождения в триггере
        float timeSinceLastReward = 0.0f; // Время с момента последней награды

        while (true)
        {
            float timeSinceLastAction = agent.GetTimeSinceLastAction();

            yield return new WaitForFixedUpdate();

            if (rb != null && rb.linearVelocity.magnitude <= minSpeedForReward)
            {
                elapsedTime += Time.fixedDeltaTime;
            }
            else
            {
                elapsedTime = 0.0f; // сбрасываем время, если машина двигается
                timeSinceLastReward = 0f;
            }

            // Проверка, прошло ли время для награды
            if (elapsedTime > 0 && rb.linearVelocity.magnitude <= minSpeedForReward && timeSinceLastAction >= rewardInterval)
            {
                timeSinceLastReward += Time.fixedDeltaTime; // Обновляем время с последней награды

                if (timeSinceLastReward >= rewardInterval )
                {
                    // Награда за нахождение в триггере
                    Debug.Log("INSIDE");
                    agent.GivePoints(stayReward);

                    // Сбрасываем таймер награды
                    timeSinceLastReward = 0f;
                }
            }

            // Если агент уже в пределах триггера достаточно времени, проверяем для парковки
            if (elapsedTime >= requiredStayTime)
            {
                if (timeSinceLastAction >= requiredStayTime)
                {
                    if (goalType == GoalType.Milestone)
                    {
                        agent.GivePoints(goalReward);
                        agent.EndEpisode();
                        yield break;
                    }
                    else
                    {
                        Debug.Log("PARKED");

                        if (Mathf.Abs(agent.transform.rotation.eulerAngles.y) <= goalMinRotation || Mathf.Abs(agent.transform.rotation.eulerAngles.y) >= (360 - goalMinRotation) || !enforceGoalMinRotation)
                        {
                            Debug.LogWarning("GOOD PARKING");
                            agent.GivePoints(goalReward, true);
                        }
                        else
                        {
                            Debug.LogWarning("BAD PARKING");
                            agent.GivePoints(goalReward / 3, true);
                        }
                        agent.EndEpisode();
                        yield break;
                    }
                }
            }
        }
    }

}