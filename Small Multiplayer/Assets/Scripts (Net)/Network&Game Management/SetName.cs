using TMPro;
using UnityEngine;

public class SetName : MonoBehaviour
{
    public TMP_InputField playerName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerName = GetComponent<TMP_InputField>();
        playerName.text = PlayerPrefs.GetString("Name");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Set()
    {
        PlayerPrefs.SetString("Name", playerName.text);
    }
}
