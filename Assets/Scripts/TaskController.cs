using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TaskController : NetworkBehaviour
{
    [SerializeField] private float timeToComplete = 2f;
    public float completingStart { get; private set; } = Mathf.Infinity;

    private void Update()
    {
        // Temp Code : Progress Bar For Task Completion
        float timePassed = timeToComplete - (Time.time - completingStart);
        if (timePassed > timeToComplete) timePassed = timeToComplete;
        gameObject.transform.GetChild(1).transform.localScale = new Vector3(timePassed / timeToComplete, 0.2f, 1f);

        // Code When Task Gets Completed
        if (Time.time - completingStart >= timeToComplete)
        {
            FindObjectOfType<GameManager>().IncTasksCompletedServerRpc();
            gameObject.SetActive(false);
        }
    }

    [ServerRpc]
    public void StartTaskServerRpc(float start)
    {
        completingStart = start;
    }

    [ServerRpc]
    public void StopTaskServerRpc()
    {
        completingStart = Mathf.Infinity;
    }
}
