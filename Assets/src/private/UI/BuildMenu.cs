using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildMenu : MonoBehaviour
{
    private UIDocument buildPanelDoc;
    private Button btn1;
    private Button btn2;
    private Button btn3;

    private Toggle toggleBuild;

    private bool buildMode = false;

    [SerializeField] private GameObject sphere;
    private Select select;

    private void Awake()
    {
        select = sphere.GetComponent<Select>();
        buildPanelDoc = GetComponent<UIDocument>();

        btn1 = buildPanelDoc.rootVisualElement.Q("BuildOption1") as Button;
        btn1.RegisterCallback<ClickEvent>(evt => PressButton(evt, 1));

        btn2 = buildPanelDoc.rootVisualElement.Q("BuildOption2") as Button;
        btn2.RegisterCallback<ClickEvent>(evt => PressButton(evt, 2));

        btn3 = buildPanelDoc.rootVisualElement.Q("BuildOption3") as Button;
        btn3.RegisterCallback<ClickEvent>(evt => PressButton(evt, 3));

        toggleBuild = buildPanelDoc.rootVisualElement.Q("BuildToggle") as Toggle;
        toggleBuild.RegisterCallback<ClickEvent>(evt => ToggleBuildMode(evt));



    }

    private void OnDisable()
    {
    }


    private void PressButton(ClickEvent evt, int id)
    {
        Debug.Log($"Option {id} clicked");
        select.SetBuildOption(buildMode, id);


    }

    private void ToggleBuildMode(ClickEvent evt)
    {
        buildMode = !buildMode;
        select.SetBuildOption(buildMode, -1);

    }
}
