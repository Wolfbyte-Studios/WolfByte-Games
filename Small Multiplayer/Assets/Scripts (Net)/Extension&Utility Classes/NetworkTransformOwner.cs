using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
public class NetworkTransformOwner : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}