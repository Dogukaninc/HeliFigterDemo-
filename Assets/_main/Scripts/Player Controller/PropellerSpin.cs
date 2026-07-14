using UnityEngine;

public class PropellerSpin : MonoBehaviour
{
    [SerializeField] private Vector3 spinAxis = Vector3.forward;
    [SerializeField] private float spinSpeed = 1080f; // derece/saniye

    private void Update()
    {
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
    }
}
