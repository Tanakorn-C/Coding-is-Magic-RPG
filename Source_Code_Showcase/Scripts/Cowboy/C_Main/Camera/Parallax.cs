using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;          // Input Camera
    public Transform subject;   // Input Subject

    // เก็บตำแหน่งเริ่ม
    private Vector2 camStartPosition;
    private Vector2 bgStartPosition;
    private float startZ;

    private Vector2 travel => (Vector2)cam.transform.position - camStartPosition;

    float distanFromSubject => transform.position.z - subject.position.z;
    float clippingPlane => (cam.transform.position.z + (distanFromSubject > 0 ? cam.farClipPlane : cam.nearClipPlane));
    float parallaxFactor => Mathf.Abs(distanFromSubject) / clippingPlane;

    void Start()
    {
        camStartPosition = cam.transform.position;
        bgStartPosition = transform.position;
        startZ = transform.position.z;
    }

    void Update()
    {
        Vector2 newPos = bgStartPosition + travel * parallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }
}
