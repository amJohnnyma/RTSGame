using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TaskCreatorUI : MonoBehaviour
{
    [Header("UI References")]
    public TaskType type;
    public string specialization;
    public GameObject position;
    public GameObject returnPoint;
    public int priority;

    public bool createTask;


    public TaskCreator taskCreator;

    public void Start()
    {
       // createButton.onClick.AddListener(OnCreateTaskClicked);
        taskCreator = GetComponent<TaskCreator>();
    }

    void Update()
    {
        if (!Application.isPlaying || !createTask) return;

        taskCreator.CreateTask(type, specialization, position, returnPoint, priority);
        createTask = false;
    }

    private void OnCreateTaskClicked()
    {

    }
}