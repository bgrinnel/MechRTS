using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalArmor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Vector3 hitDirection = (other.transform.position - transform.position).normalized;
        DetermineArmorSide(hitDirection);
    }

    void DetermineArmorSide(Vector3 hitDirection)
    {
        if (Vector3.Dot(hitDirection, transform.forward) > 0.5f) // Front
        {
            Debug.Log("Hit on Front Armor");
        }

        else if (Vector3.Dot(hitDirection, -transform.forward) > 0.5f) // Back
        {
            Debug.Log("Hit on Back Armor");
        }
        else if (Vector3.Dot(hitDirection, transform.right) > 0.5f) // Right side
        {
            Debug.Log("Hit on Right Side Armor");
        }
        else if (Vector3.Dot(hitDirection, -transform.right) > 0.5f) // Left side
        {
            Debug.Log("Hit on Left Side Armor");
        }
        else if (Vector3.Dot(hitDirection, transform.up) > 0.5f) // Top
        {
            Debug.Log("Hit on Top Armor");
        }
    }
}
