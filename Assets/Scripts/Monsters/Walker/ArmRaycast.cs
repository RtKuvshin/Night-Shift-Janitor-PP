using System;
using UnityEngine;

public class ArmRaycast : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform armTransform;
    [SerializeField] private float rayLength = 1f;
    [SerializeField] private int attackDamage = 25;

    private void DealDamage()
    {
        PlayerHealth.Instance.ReceiveDamage(attackDamage);
    }

    private void LateUpdate()
    {
        Vector3 origin = armTransform.position;
        Vector3 direction = -armTransform.forward; 

        Debug.DrawRay(origin, direction * rayLength, Color.red);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayLength, playerLayer))
        {
            DealDamage();
        }
    }
}