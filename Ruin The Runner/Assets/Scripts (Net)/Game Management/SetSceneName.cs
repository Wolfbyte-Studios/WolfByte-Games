using Mirror;
using TMPro;
using UnityEngine;

public class SetSceneName : NetworkBehaviour
{
    public TextMeshProUGUI tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartClient()
    {
        base.OnStartClient();
    
        tmp = GetComponent<TextMeshProUGUI>();
        SceneStuff.Instance.ChooseRandomScene();
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
