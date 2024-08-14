using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[CustomEditor(typeof(Clickable))]
public class ClickableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Clickable clickable = (Clickable)target;
        if (GUILayout.Button("Trigger Event"))
        {
            clickable.TriggerEvent();
        }

        if (GUI.changed) // Check if the inspector GUI changed
        {
            HandleClickableTypeChange(clickable);
        }
    }

    public void HandleClickableTypeChange(Clickable clickable)
    {
        // Clear all existing listeners from the event
        if(clickable.ClickableType == Clickable.clickableType.Custom)
        {
            return;
        }
        removeEvents(clickable.myEvent);
        removeEvents(clickable.OnCoolDown);
        removeEvents(clickable.secondaryEvent);
        removeEvents(clickable.onSelect);
        removeEvents(clickable.onDeselect);

        switch (clickable.ClickableType)
        {
            case Clickable.clickableType.Moving:
                AddComponentIfNotExists<LerpMovement>(clickable);
                UnityEventTools.AddPersistentListener(clickable.myEvent, clickable.GetComponent<LerpMovement>().Trigger);
                break;

            case Clickable.clickableType.Rotating:
                AddComponentIfNotExists<Spin>(clickable);
                UnityEventTools.AddPersistentListener(clickable.myEvent, () => clickable.GetComponent<Spin>().OverrideSpeed(2));
                break;

            case Clickable.clickableType.Pause:
                AddComponentIfNotExists<Sine>(clickable);
                UnityEventTools.AddPersistentListener(clickable.myEvent, clickable.GetComponent<Sine>().Pause);
                UnityEventTools.AddPersistentListener(clickable.OnCoolDown, clickable.GetComponent<Sine>().Resume);
                clickable.CoolDown = true;
                clickable.coolDown = 3f;
                break;

            case Clickable.clickableType.Holdable:
                AddComponentIfNotExists<HoldItem>(clickable);
                UnityEventTools.AddPersistentListener(clickable.myEvent, clickable.GetComponent<HoldItem>().Trigger);
                UnityEventTools.AddPersistentListener(clickable.secondaryEvent, clickable.GetComponent<HoldItem>().Throw);
                break;

                // Uncomment and add more cases as needed
                /*
                case Clickable.clickableType.Shake:
                    AddComponentIfNotExists<Earthquake>(clickable);
                    UnityEventTools.AddPersistentListener(clickable.myEvent, clickable.GetComponent<Earthquake>().Trigger);
                    break;

                case Clickable.clickableType.Toggle:
                    AddComponentIfNotExists<Toggle>(clickable);
                    UnityEventTools.AddPersistentListener(clickable.myEvent, clickable.GetComponent<Toggle>().Trigger);
                    break;
                */
        }
    }
    public void removeEvents(UnityEvent unityEvent)
    {
        int eventCount = unityEvent.GetPersistentEventCount();
        for (int i = 0; i < eventCount; i++)
        {
            UnityEventTools.RemovePersistentListener(unityEvent, 0); // Always remove the first listener (index 0) repeatedly
        }
    }

    private void AddComponentIfNotExists<T>(Clickable clickable) where T : Component
    {
        if (clickable.GetComponent<T>() == null)
        {
            clickable.gameObject.AddComponent<T>();
        }
    }
}
