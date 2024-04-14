using UnityEngine;

public class ButtonScroller : MonoBehaviour
{
    public float movementSpeed;
    void Update()
    {
        transform.localPosition += movementSpeed * Time.deltaTime * Vector3.left;
    }
}
