using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleFixed : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    
    private void Update()
    {
        transform.eulerAngles = new Vector3(offset.x, offset.y, offset.z);
    }
}
