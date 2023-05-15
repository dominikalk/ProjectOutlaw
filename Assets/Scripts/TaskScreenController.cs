using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TaskScreenController : MonoBehaviour
{
    private RectTransform tasksScreen;
    private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private int lineSpacing;

    private TaskController[] tasks;
    private Dictionary<ulong, TextMeshProUGUI> taskLines = new Dictionary<ulong, TextMeshProUGUI>();
    private int tasksCompleted = 0;

    // Start is called before the first frame update
    void Start()
    {
        tasksScreen = GetComponent<RectTransform>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tasks == null)
        {
            CreateTaskScreen();
        }
    }

    // If tasks are correct number, sets the tasks ui
    private void CreateTaskScreen()
    {
        TaskController[] tempTasks = FindObjectsOfType<TaskController>();
        if (tempTasks.Length != gameManager.noOfTasks.Value) return;
        tasks = tempTasks.OrderBy(x => x.taskName).ToArray();
        for (int i = 0; i < tasks.Length; i++)
        {
            CreateTaskText(tasks[i], i);
        }
        progressText.text = $"Completed 0/{tasks.Length}";
        tasksScreen.sizeDelta =
            new Vector2(
                tasksScreen.rect.width,
                (tasks.Length + 1) * (taskText.GetComponent<RectTransform>().rect.height + lineSpacing) + lineSpacing + 40
            );
    }

    // Creates the text for each task
    private void CreateTaskText(TaskController task, int index)
    {
        TextMeshProUGUI taskLine = Instantiate(taskText, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        taskLine.transform.SetParent(tasksScreen.transform, true);
        taskLine.text = task.taskName;
        taskLine.color = new Color32(188, 33, 33, 255);
        RectTransform taskRect = taskLine.GetComponent<RectTransform>();
        taskRect.localPosition = new Vector3(30, -(lineSpacing * (index + 1) + (taskRect.rect.height * index) + 20), 0);
        taskRect.offsetMax = new Vector3(30, taskRect.offsetMax.y);
        taskRect.localScale = Vector3.one;
        taskLines.Add(task.GetComponent<NetworkObject>().NetworkObjectId, taskLine);
    }

    // Edits task text to show completed
    public void CompleteTask(ulong taskId)
    {
        TextMeshProUGUI taskLine = taskLines[taskId];
        taskLine.color = new Color32(30, 147, 30, 255);
        taskLine.fontStyle = FontStyles.Strikethrough;
        tasksCompleted++;
        progressText.text = $"Completed {tasksCompleted}/{tasks.Length}";
    }
}
