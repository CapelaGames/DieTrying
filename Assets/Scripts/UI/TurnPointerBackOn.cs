using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TurnPointerBackOn : MonoBehaviour
{
    void Awake()
    {
        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Confined;
        UnityEngine.Cursor.visible = true;
    }
}

