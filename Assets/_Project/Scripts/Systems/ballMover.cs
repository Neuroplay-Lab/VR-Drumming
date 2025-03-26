using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballMover : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private EyeFocus eyeTracker;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = eyeTracker.GetCurrentFocusCoordinates();

        if(position != null)
        {
            this.transform.position = position;
        }
    }
}
