using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;

public class SetSceneName : NetworkBehaviour
{
    public TextMeshProUGUI tmp;
    public SceneStuff scenestuff;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartServer()
    {
        base.OnStartServer();
    
        tmp = GetComponent<TextMeshProUGUI>();
    }
    

    // Update is called once per frame
    void Update()
    {
        if (GameManager.singleton != null)
        {
            tmp.text = SceneStuff.Instance.SceneToLoadString;
        }

    }
}
