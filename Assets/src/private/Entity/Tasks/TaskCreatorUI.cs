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

        Transform targetTransform = position != null ? position.transform : null;
        Vector3 returnPos = returnPoint != null ? returnPoint.transform.position : Vector3.zero;

        taskCreator.CreateTask(type, specialization, targetTransform, priority);
        createTask = false;
    }

    private void OnCreateTaskClicked()
    {

    }
}