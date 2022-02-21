using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BoxWireframe : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}