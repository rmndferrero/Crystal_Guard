using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public Transform cam;

    private float xRotation = 0f;
    private Vector2 lookInput;
    private bool isSettled = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(SettleMouse());
    }

    void Update()
    {
        if (isSettled)
        {
            HandleMouseLook();
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    IEnumerator SettleMouse()
    {
        yield return null;
        isSettled = true;
    }
}