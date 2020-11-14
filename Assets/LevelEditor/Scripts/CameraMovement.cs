using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
 
    [AddComponentMenu("Camera-Control/Keyboard Orbit")]
    public Transform target;
    public float distance = 0.0f;
    public float zoomSpd = 2.0f;

    [SerializeField] Canvas HomeMenuCanvas;
    [SerializeField] Canvas BuildUICanvas;
    [SerializeField] Camera SelfCamera;
    public bool EnableCameraMovement = false;

    public int yMinLimit = -20;
    public int yMaxLimit = 20;

    private float x = 22.0f;
    private float y = 33.0f;

    public void Start()
    {


        x = 22f;
        y = 33f;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;

        BuildUICanvas.enabled = false;
    }

    public float previousX;
    public float previousY;

    public void EnterBuildMode()
    {
        EnableCameraMovement = true;
        HomeMenuCanvas.enabled = false;
        BuildUICanvas.enabled = true;
        SelfCamera.transform.position = new Vector3(50,20,50);
    }

    public void LateUpdate()
    {
        if (target && EnableCameraMovement)
        {
            bool isLeftButtonDown = Input.GetMouseButton(0);
            bool isRightButtonDown = Input.GetMouseButton(1);
            bool isMiddleButtonDown = Input.GetMouseButton(2);
            
            distance = 0;
            var ScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (isRightButtonDown)
            {
                x -= (previousX - Input.mousePosition.x) * 0.1f;
                y += (previousY - Input.mousePosition.y) * 0.1f;
                if (ScrollWheel > 0f)
                {
                    zoomSpd++;
                }
                else if (ScrollWheel < 0f)
                {
                    zoomSpd--;
                }
            }
            else 
            {
                if (ScrollWheel > 0f)
                {
                    distance -= zoomSpd * 0.08f;
                }
                else if (ScrollWheel < 0f)
                {
                    distance += zoomSpd * 0.08f;
                }
            }

            zoomSpd = Mathf.Clamp(zoomSpd, 2, 999999);

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            float LeftRight = 0;

            if (Input.GetKey("w"))
                distance -= zoomSpd * 0.02f;
            if (Input.GetKey("s"))
                distance += zoomSpd * 0.02f;
            if (Input.GetKey("a"))
                LeftRight -= zoomSpd * 0.5f;
            if (Input.GetKey("d"))
                LeftRight += zoomSpd * 0.5f;
            var xRot = ClampAngle(target.rotation.eulerAngles.x, 1, 360)/360 * LeftRight;
            var zRot = ClampAngle(target.rotation.eulerAngles.z, 1, 360)/360 * LeftRight;

            var timeFactor = Time.deltaTime + 1;
            Quaternion rotation = Quaternion.Euler(y*timeFactor, x*timeFactor, 0.0f);
            Vector3 position = rotation * new Vector3(xRot* timeFactor, 0, (zRot - distance)* timeFactor) + target.position;
            transform.rotation = rotation;
            transform.position = position;

            previousX = Input.mousePosition.x;
            previousY = Input.mousePosition.y;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360.0f)
            angle += 360.0f;
        if (angle > 360.0f)
            angle -= 360.0f;
        return Mathf.Clamp(angle, min, max);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
