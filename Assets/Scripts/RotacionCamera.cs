using UnityEngine;

public class RotacionCamera : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 5 * Time.deltaTime,0 ) ;
    }
}
