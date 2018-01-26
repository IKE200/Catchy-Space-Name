using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour {

    public float speed = 100;
    public float xSpeed = 10;
    public float ySpeed = 10;
    public float xMax = 10;
    public float yMax = 10;
    public float controlRollFactor = 20;
    public float controlPitchFactor = 20;
    public float controlYawFactor = 10;
    public float positionPitchFactor = -5;
    public float positionYawFactor = 2;
    public Ship ship;
    public WayPointSystem wayPointSystem;

    // Use this for initialization
    void Start () {
        wayPointSystem.Initialise();
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = wayPointSystem.UpdatePositionBaseOnSpeed(speed);
        wayPointSystem.UpdateRotation(transform);
        float horizontalMove = CrossPlatformInputManager.GetAxis("Horizontal");
        float verticalMove = CrossPlatformInputManager.GetAxis("Vertical");

        float xOffset = xSpeed * horizontalMove * Time.deltaTime;
        float yOffset = ySpeed * verticalMove * Time.deltaTime;

        

        Vector3 pos = ship.transform.localPosition;
        float newX = Mathf.Clamp(pos.x + xOffset, -xMax, xMax);
        float newY = Mathf.Clamp(pos.y + yOffset, -yMax, yMax);
        ship.transform.localPosition = new Vector3(newX, newY, pos.z);

        float roll = -controlRollFactor * horizontalMove;
        float pitch = controlPitchFactor * verticalMove + positionPitchFactor * ship.transform.localPosition.y;
        float yaw = controlYawFactor * horizontalMove + positionYawFactor * ship.transform.localPosition.x;
        ship.transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
	}
}
