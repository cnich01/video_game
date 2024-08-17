using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform player;
    public Transform firstPersonCameraPosition;
    public Transform thirdPersonCameraPosition;

    float xRotation;
    float yRotation;


    private bool firstPerson = true;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        //get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam and player
        transform.rotation = Quaternion.Euler(xRotation,yRotation,0);
        player.rotation = Quaternion.Euler(0, yRotation, 0);

        //switch between first and third person;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            firstPerson = !firstPerson;
        }

        if (firstPerson)
        {
            transform.position = firstPersonCameraPosition.position;
        }
        else
        {
            transform.position = thirdPersonCameraPosition.position;
            transform.LookAt(player.position);
        }
    }
}
