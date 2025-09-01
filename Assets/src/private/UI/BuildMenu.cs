using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildMenu : MonoBehaviour
{
    private bool buildMode = false;

    [SerializeField] private GameObject sphere;
    [SerializeField] private UIDocument document;
    private Select select;

    private void Awake()
    {
        select = sphere.GetComponent<Select>();
        

    }


    private void OnDisable()
    {
    }


    public void PressButton(int id)
    {
        Debug.Log($"Option {id} clicked");
        select.SetBuildOption(buildMode, id);


    }

    public void ToggleBuildMode()
    {
        buildMode = !buildMode;
        select.SetBuildOption(buildMode, -1);

    }
}
