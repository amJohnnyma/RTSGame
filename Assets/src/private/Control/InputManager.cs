
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputBinding
{
    public string action;
    public KeyCode key;
}

public class InputManager : MonoBehaviour
{
    public List<InputBinding> bindings;

    public BuildMenu buildMenu;
    

    void Update()
    {
        if (buildMenu.CanCheckInput())
        {
            foreach (var binding in bindings)
            {
                if (Input.GetKeyDown(binding.key))
                {
                    KeyPressAction(binding.action);
                }
            }
        }
    }

    private void KeyPressAction(string action)
    {

        switch (action)
        {
            case "selectBO1":
                buildMenu.PressButton(1);
                break;
            case "selectBO2":
                buildMenu.PressButton(2);
                break;
            case "selectBO3":
                buildMenu.PressButton(3);
                break;
            case "toggleBM": // build mode
                buildMenu.ToggleBuildMode();
                break;
            case "toggleCM": // destroy mode
                buildMenu.ToggleDestroyMode();
                break;
            case "toggleSM": //select mode
                buildMenu.ToggleSelectMode();
                break;
            case null:
                break;
            
        }

    }
}