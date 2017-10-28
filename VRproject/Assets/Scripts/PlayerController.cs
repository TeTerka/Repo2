using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//obycejny player controller pouzivany na zactku, kdyz jsem jeste nemela vive....ted uz k nicemu

public class PlayerController : MonoBehaviour
{
    public Camera cam;

    [Header("Movement")]
    public float startSpeed;
    float speed;
    public float jumpForce;
    bool hasJumped = false;
    float upDown = 0;
    CharacterController cc;
    float mouseSensitivity = 7f;
    float currentCameraRotationX = 0f;
    float viewRange = 70;


    void Start()
    {
        cc = GetComponent<CharacterController>();
        speed = startSpeed;
    }

    void Update()
    {

        //sprint check
        if(Input.GetKey(KeyCode.LeftShift))
        {
            //sprint
            speed = 2 * startSpeed;
        }
        else
        {
            //walk
            speed = startSpeed;
        }

        //move
        float horizontal = Input.GetAxis("Horizontal") * speed;
        float vertical = Input.GetAxis("Vertical") * speed;
        var rotx = Input.GetAxis("Mouse X") * mouseSensitivity;
        var roty = Input.GetAxis("Mouse Y") * mouseSensitivity;

        var mov = new Vector3(horizontal, upDown, vertical);
        transform.Rotate(0, rotx, 0);

        currentCameraRotationX -= roty;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -viewRange, viewRange);
        cam.transform.eulerAngles = new Vector3(currentCameraRotationX, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);

        mov = transform.rotation * mov;
        cc.Move(mov*Time.deltaTime);

        //jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasJumped = true;
        }
        ApplyGravity();

        //crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (transform.localScale.y == 1)
            {
                this.transform.localScale = new Vector3(1, 0.5f, 1);
            }
            else
            {
                this.transform.localScale = new Vector3(1, 1, 1);
            }
        }

    }

    void ApplyGravity()
    {
        if(cc.isGrounded)//hrac je nohama na zemi
        {
            if(hasJumped)
            {
                upDown = jumpForce;
            }
            else
            {
                upDown = Physics.gravity.y;
            }
        }
        else//hrac je ve vzduchu
        {
            upDown += Physics.gravity.y * Time.deltaTime;
            upDown = Mathf.Clamp(upDown, -50, jumpForce);
            hasJumped = false;
        }
    }

}

