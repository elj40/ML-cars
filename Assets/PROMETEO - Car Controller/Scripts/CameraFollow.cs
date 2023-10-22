using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	public Vector3 cameraOffset;
	Vector3 initialCameraPosition;
	Vector3 initialCarPosition;
	Vector3 absoluteInitCameraPosition;


	void Start(){
		initialCameraPosition = gameObject.transform.position;
		initialCarPosition = carTransform.position;
		absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
		Application.targetFrameRate = 60;
	}

	void FixedUpdate()
	{	



		//Look at car
		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);


		Vector3 cameraOffsetY = new Vector3(0,cameraOffset.y,0);
		float cameraOffsetZ = cameraOffset.z;
		//Move to car 
		//Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
		Vector3 _targetPos = carTransform.transform.position + carTransform.rotation * Vector3.forward * -cameraOffsetZ + cameraOffsetY;
		transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);

	}

}
