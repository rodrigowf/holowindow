using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;  //its a must to access new UI in script

public class SocketCam: MonoBehaviour
{
	// 1. Declare Variables
	Thread receiveThread; //1
	UdpClient client; //2
	int port; //3

    public GameObject cameraObject;
    Transform cameraTrans;

    byte[] recivedData;

    Vector4 inBox;
    Vector3 camPosRef; // Camera Reference (Original) Position
    Vector3 camPosDev; // Camera deviation from Reference (original Position)
    Vector3 camRotRef; // Camera Reference (Original) Rotation
    Vector3 camRotTarget; // Camera deviation from Reference (original Rotation)

    Vector3 posMult;
    Vector3 rotMult;


	// 2. Initialize variables
	void Start ()
	{
		port = 5065;

		posMult = new Vector3(10, 10, 20);
		rotMult = new Vector3(50, 40, 0);

		inBox = new Vector4(0, 0, 0, 0);
		camPosDev = new Vector3(0, 0, 0);
		camRotTarget = new Vector3(0, 0, 0);

		cameraTrans = cameraObject.GetComponent<Transform>();
		camPosRef = cameraTrans.position;
		camRotRef = cameraTrans.rotation.eulerAngles;

		InitUDP(); //4
	}

	// 3. InitUDP
	private void InitUDP()
	{
		print ("UDP Initialized");

		receiveThread = new Thread (new ThreadStart(ReceiveData)); //1
		receiveThread.IsBackground = true; //2
		receiveThread.Start (); //3

	}

	// 4. Receive Data
	private void ReceiveData()
	{
		client = new UdpClient (port); //1
		while (true) //2
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
				recivedData = client.Receive(ref anyIP);

			} catch(Exception e)
			{
				print (e.ToString()); //7
			}
		}
	}

	void parseBox()
	{
	    string input = Encoding.UTF8.GetString(recivedData);
        string[] arr = input.Split(char.Parse("."));

        // (startX, startY, endX, endY)

//        print (">> " + input);

        inBox.x = float.Parse("0." + arr[1]);  // start X
        inBox.y = float.Parse("0." + arr[2]); // start Y
        inBox.z = float.Parse("0." + arr[3]); // end X
        inBox.w = float.Parse("0." + arr[4]); // end Y
	}

	public void moveCamera()
	{
	    parseBox();

//	    print(inBox);

        // Position
        camPosDev.x = ( inBox.z + inBox.x ) / 2;
        camPosDev.y = 1 - (( inBox.w + inBox.y ) / 2 );
        camPosDev.z = 1 - ((( inBox.z - inBox.x ) + ( inBox.w - inBox.y )) / 2);

        cameraTrans.position = camPosRef + (Vector3.Scale(camPosDev, posMult) - posMult/2);

        // Rotation
        camRotTarget.x = camRotRef.x + ((camPosDev.y * rotMult.x) - rotMult.x/2);
        camRotTarget.y = camRotRef.y + ((camPosDev.x * rotMult.y) - rotMult.y/2); // aumenta com o mesmo aumento que x da posição

        print (camRotTarget);

        cameraTrans.rotation = Quaternion.Euler(camRotTarget.x, camRotTarget.y, camRotRef.z);
	}

	// 6. Check for variable value, and make the Player Jump!
	void Update ()
	{
	    moveCamera();
	}
}
