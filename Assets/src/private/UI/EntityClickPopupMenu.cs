using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityClickPopupMenu : MonoBehaviour
{
    public UIDocument document;
    public VisualElement root;
    private Button button;
    public Select selectScript;
    // Start is called before the first frame update
    void OnEnable()
    {
        root = document.rootVisualElement;
        button = root.Q<Button>("closeBtn");
        button.RegisterCallback<ClickEvent>(OnCloseClicked);

    }


    private void OnCloseClicked(ClickEvent evt)
    {
        Debug.Log("Close click");
        selectScript.CloseEntityClickPopup();
    }
}
