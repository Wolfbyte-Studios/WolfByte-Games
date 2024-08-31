using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

// This class receives input from a PlayerInput component and dispatches it
// to the appropriate Cinemachine InputAxis. The playerInput component should
// be on the same GameObject, or specified in the PlayerInput field.
class CustomInputHandler : InputAxisControllerBase<CustomInputHandler.Reader>
{
    [Header("Input Source Override")]
    public PlayerInput PlayerInput;
    public float gain = 1.0f;

    void Awake()
    {
        // When the PlayerInput receives an input, send it to all the controllers
        if (PlayerInput == null)
            TryGetComponent(out PlayerInput);
        if (PlayerInput == null)
            Debug.LogError("Cannot find PlayerInput component");
        else
        {
            PlayerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            PlayerInput.onActionTriggered += (value) =>
            {
                for (var i = 0; i < Controllers.Count; i++)
                    Controllers[i].Input.ProcessInput(value.action, gain);
            };
        }
    }

    // We process user input on the Update clock
    void Update()
    {
        if (Application.isPlaying)
            UpdateControllers();
    }

    // Controllers will be instances of this class.
    [Serializable]
    public class Reader : IInputAxisReader
    {
        public InputActionReference Input;
        Vector2 m_Value; // the cached value of the input
        private float gain = 1.0f;

        // Method to process input with gain parameter
        public void ProcessInput(InputAction action, float gain)
        {
            this.gain = gain;

            // If it's my action then cache the new value
            if (Input != null && Input.action.id == action.id)
            {
                if (action.expectedControlType == "Vector2")
                    m_Value = action.ReadValue<Vector2>() * gain;
                else
                    m_Value.x = m_Value.y = action.ReadValue<float>() * gain;
            }
        }

        // IInputAxisReader interface: Called by the framework to read the input value
        public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            return (hint == IInputAxisOwner.AxisDescriptor.Hints.Y ? m_Value.y : m_Value.x);
        }
    }
}
