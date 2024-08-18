using TMPro;
using UnityEngine;

public class SetSceneName : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        tmp.text = SceneStuff.Instance.SceneToLoadString;
    }
}
