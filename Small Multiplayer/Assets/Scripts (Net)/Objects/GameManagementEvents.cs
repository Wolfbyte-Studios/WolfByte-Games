using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class GameManagementEvents : MonoBehaviour
{

    public UnityEvent OnStateUi;
    public UnityEvent OnStateInGame;
    public UnityEvent OnStateOther;

    public UnityEvent OnModeStandard;
    public CurrentSessionStats.GameStateEnum state;
    public CurrentSessionStats.GameStateEnum State
    {
        get { return state; }
        set
        {
            if (state != value)
            {
                state = value;
                OnStateChanged(state);
            }
        }
    }
    public CurrentSessionStats.GameModeEnum mode;
    public CurrentSessionStats.GameModeEnum Mode
    {
        get { return mode; }
        set
        {
            if (mode != value)
            {
                mode = value;
                OnModeChanged(mode);
            }
        }
    }
    private void OnEnable()
    {
        OnStateChanged(state);
        OnModeChanged(mode);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public void OnStateChanged(CurrentSessionStats.GameStateEnum state)
    {
        if (!CurrentSessionStats.Instance.netActive)
        { return; }
        switch (state)
        {
            case CurrentSessionStats.GameStateEnum.InGame:
                OnStateInGame?.Invoke();
                break;
            case CurrentSessionStats.GameStateEnum.UI:
                OnStateUi?.Invoke();
                break;
            case CurrentSessionStats.GameStateEnum.Other:
                OnStateOther?.Invoke();
                break;
        }
    }
    public void OnModeChanged(CurrentSessionStats.GameModeEnum mode)
    {
        if (!CurrentSessionStats.Instance.netActive)
        { return; }
        switch (mode)
        {
            case CurrentSessionStats.GameModeEnum.Standard:
                OnModeStandard?.Invoke();
                break;
           
        }
    }
    // Update is called once per frame
    void Update()
    {
        check();
    }
    public void check()
    {
        if (!CurrentSessionStats.Instance.netActive)
        { return; }
        State = CurrentSessionStats.Instance.GameState ;
        Mode = CurrentSessionStats.Instance.GameMode ;
    }
}
