using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private UIDocument document;
    private Button button;
    private void Awake()
    {
        document = GetComponent<UIDocument>();
        button = document.rootVisualElement.Q("StartBtn") as Button;

        button.RegisterCallback<ClickEvent>(OnStartClick);

    }

    private void OnDisable()
    {
        button.UnregisterCallback<ClickEvent>(OnStartClick);
    }

    private void OnStartClick(ClickEvent evt)
    {
        Debug.Log("StartBtn Clicked");
    }
}
