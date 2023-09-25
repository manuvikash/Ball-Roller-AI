using UnityEngine;

public class NameExample : MonoBehaviour
{
    void Start()
    {
        // Access and print the name of the GameObject this script is attached to.
        string objectName = gameObject.name;
        Debug.Log("GameObject Name: " + objectName);
    }
}