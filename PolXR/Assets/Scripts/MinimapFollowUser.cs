using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollowUser : MonoBehaviour
{
    [SerializeField] private Transform user;
    void Update()
    {
        Vector3 newPosition = user.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }
}
