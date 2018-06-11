using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    private GameObject CameraObject;
    public float sensitivityX = 3F;
    public float sensitivityY = 3F;
    public float gravity = 2f;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationX = 0F;
    float rotationY = 0F;

    public float WalkSpeed = 2f;
    public float RunSpeed = 3.5f;
    public float JumpHeight = 50f;
    private float moveHorizontal = 0;
    private float moveForward = 0;
    private Vector3 MovementVector = new Vector3(0,0,1);
    Quaternion originalRotation;

    private CharacterController _controller;
    // Use this for initialization
    void Start ()
    {
        //Screen.lockCursor = true;
        Cursor.lockState = CursorLockMode.Locked;
        CameraObject = gameObject.transform.GetChild(0).gameObject;
        _controller = gameObject.GetComponent<CharacterController>();
        // Make the rigid body not change rotation
        if (gameObject.GetComponent<Rigidbody>())
            gameObject.GetComponent<Rigidbody>().freezeRotation = true;
        originalRotation = transform.localRotation;
    }

    Vector3 velocity = new Vector3(0, 0, 0);
    // Update is called once per frame
    void Update () {
        
	    rotationX += Input.GetAxis("Mouse X") * sensitivityX;
	    rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

	    rotationX = ClampAngle(rotationX, minimumX, maximumX);
	    rotationY = ClampAngle(rotationY, minimumY, maximumY);

	    Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
	    Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
	    transform.localRotation = originalRotation * xQuaternion;
	    CameraObject.transform.localRotation = yQuaternion;

        MovementVector = xQuaternion * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical") );
        if (Input.GetButton("Run"))
            MovementVector *= RunSpeed;
        else
            MovementVector *= WalkSpeed;

        if (Input.GetButton("Crouch"))
        {
            MovementVector /= 2f;
            gameObject.transform.localScale = new Vector3(1,0.4f,1);
        }
        else
        {
            gameObject.transform.localScale = new Vector3(1, 1, 1);

        }
        if (!_controller.isGrounded)
	    {
	        velocity += Physics.gravity * gravity * Time.deltaTime;
	    }
        else
	    {
	        velocity.y = 0;
            if (Input.GetButton("Jump"))
                velocity.y += Mathf.Sqrt(JumpHeight * -2f * (gravity * Physics.gravity.y));
            velocity.x = MovementVector.x;
            velocity.z = MovementVector.z;
        }
	    _controller.Move(velocity * Time.deltaTime);

        //keyboard inputs
        switch (Input.inputString)
        {
            /*
            case "1":
                WorldScript.ActiveWorld.ActiveBlockType = BlockClass.BlockType.BedRock;
                break;
            case "2":
                WorldScript.ActiveWorld.ActiveBlockType = BlockClass.BlockType.Dirt;
                break;
            case "3":
                WorldScript.ActiveWorld.ActiveBlockType = BlockClass.BlockType.Grass;
                break;
            case "4":
                WorldScript.ActiveWorld.ActiveBlockType = BlockClass.BlockType.Water;
                break;
            case "5":
                WorldScript.ActiveWorld.ActiveBlockType = BlockClass.BlockType.Granite;
                break;
            case "6":
                WorldScript.ActiveWorld.ActiveBlockType = BlockClass.BlockType.Sand;
                break;
*/
        }


    }
    public static float ClampAngle(float angle, float min, float max)
    {
        while(angle <= -360F)
        {
            angle += 360F;
        }
        while (angle >= 360F)
        {
            angle -= 360F;
        }
        return Mathf.Clamp(angle, min, max);
    }
}
