using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public float FOV;

    public PlayerController player;
    public Transform firstPersonCameraPosition;
    public Transform thirdPersonCameraPosition;

    float xRotation; //used for First Person
    float yRotation;
    float yPosition; //used for Third Person

    float mouseX;
    float mouseY;


    private bool firstPerson = true;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        yPosition = thirdPersonCameraPosition.position.y;
    }

    void Update()
    {
        if (player.screen == PlayerController.ScreenState.hud)
        {
            //get mouse input
            mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * 10;
            mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * 10;
        }
        else
        {
            mouseX = 0f;
            mouseY = 0f;
        }
        yRotation += mouseX;

        //switch between first and third person;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            firstPerson = !firstPerson;
        }

        if (firstPerson)
        {
            player.SetHitRange(3f);
            //calculate rotation
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //position camera
            transform.position = firstPersonCameraPosition.position;

            //rotate camera
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
        else
        {
            player.SetHitRange(20f);
            //calculate position
            yPosition -= mouseY / 2;
            yPosition = Mathf.Clamp(yPosition, -4.5f, 10f);

            //position camera
            transform.position = new Vector3(thirdPersonCameraPosition.position.x, thirdPersonCameraPosition.position.y + yPosition, thirdPersonCameraPosition.position.z);

            //rotate camera
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
            transform.LookAt(player.transform.position);
        }

        //rotate player
        player.transform.rotation = Quaternion.Euler(0, yRotation, 0);

        //set Field of View
        Camera.main.fieldOfView = FOV;
    }
}
