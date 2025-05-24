
using System;
using System.Net.Mime;
using System.Security.Cryptography;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float panSpeed = 20f;
    public float panBoarderThickness = 5f;

    public float scrollSpeed = 5f;

    public float minX = 2f;
    public float maxX = 42f;
    public float minY = 72f; // Updated Y min
    public float maxY = 100f; // Updated Y max
    public float minZ = -100f; // Note: -100 is lower than -46
    public float maxZ = -46f;  // Note: -46 is higher than -100

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.GameIsOver)
        {
            this.enabled = false;
            return;
        }


        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBoarderThickness)
        {
            transform.Translate(Vector3.forward * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("s") || Input.mousePosition.y <= panBoarderThickness)
        {
            transform.Translate(Vector3.back * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBoarderThickness)
        {
            transform.Translate(Vector3.right * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("a") || Input.mousePosition.x <= panBoarderThickness)
        {
            transform.Translate(Vector3.left * panSpeed * Time.deltaTime, Space.World);
        }

        float Scroll = Input.GetAxis("Mouse ScrollWheel");

        Vector3 pos = transform.position;
        
        pos.y -= Scroll * 1000 * scrollSpeed * Time.deltaTime;


        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Clamp X position
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        // Clamp Z position
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        transform.position = pos;

        Debug.Log(Scroll);

    }
}
