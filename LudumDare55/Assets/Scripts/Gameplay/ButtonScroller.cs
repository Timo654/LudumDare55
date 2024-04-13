using UnityEngine;

public class ButtonScroller : MonoBehaviour
{
    public float movementSpeed;
    void FixedUpdate()
    {
        transform.localPosition += movementSpeed * Time.fixedDeltaTime * Vector3.left;
    }
}
