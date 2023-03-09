using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class GameManager : NetworkBehaviour
{
    private enum GameEndEnum {
        TimeOut,
        TasksCompleted,
        OutlawsShot,
        BulletsGone,
        NotEnded
    };

    private GameEndEnum gameEndType = GameEndEnum.NotEnded;

    private float gameLength = 120f;
    private float gameStartTime;
    private TaskController[] tasks = { };
    private int noTasksCompleted = 0;
    [SerializeField] private GameObject WinLossScreen;  
    [SerializeField] private TextMeshProUGUI WinLossText;  
    [SerializeField] private TextMeshProUGUI WinLossDescText;

    public override void OnNetworkSpawn()
    {
        tasks = FindObjectsOfType<TaskController>();
        gameStartTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - gameStartTime >= gameLength)
        {
            gameEndType = GameEndEnum.TimeOut;
            Debug.Log("Sherrifs Win - Time Ran Out");
        }

        if(gameEndType != GameEndEnum.NotEnded)
        {
            WinLossScreen.SetActive(true);
            switch (gameEndType)
            {
                case GameEndEnum.TimeOut:
                    WinLossText.text = "You Lose";
                    WinLossDescText.text = "You Ran Out Of Time!";
                    break;
                case GameEndEnum.TasksCompleted:
                    WinLossText.text = "You Win";
                    WinLossDescText.text = "You Completed All The Tasks";
                    break;
                case GameEndEnum.OutlawsShot:
                    WinLossText.text = "You Lose";
                    WinLossDescText.text = "All The OutLaws Were Shot!";
                    break;
                case GameEndEnum.BulletsGone:
                    WinLossText.text = "You Win";
                    WinLossDescText.text = "All The Bullets Are Gone!";
                    break;
                default:
                    break;
                
            }

        }
    }

    [ServerRpc]
    public void IncTasksCompletedServerRpc()
    {
        noTasksCompleted++;
        if (noTasksCompleted >= tasks.Length) 
        {
            gameEndType = GameEndEnum.TasksCompleted;
            Debug.Log("Outlaws Win - Completed All Tasks");
        }
    }
}
