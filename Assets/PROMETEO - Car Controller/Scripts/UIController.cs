using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private Image UP;
    private Image DOWN;
    private Image LEFT;
    private Image RIGHT;
    private Image SPACE;

    private Text Speed;

    public GameObject cars;
    public int carIndex = 0;

    public CameraFollow cameraFollow;

    private CarAgent currentCarAgent;
    private PrometeoCarController currentCarController;

    public Color pressedColor;
    public Color releasedColor;

    void Start() {
        Transform keys = transform.Find("Keys");

        UP = keys.Find("UP").gameObject.GetComponent<Image>();
        DOWN = keys.Find("DOWN").gameObject.GetComponent<Image>();
        LEFT = keys.Find("LEFT").gameObject.GetComponent<Image>();
        RIGHT = keys.Find("RIGHT").gameObject.GetComponent<Image>();
        SPACE = keys.Find("SPACE").gameObject.GetComponent<Image>();
        
        Transform data = transform.Find("Data");
        Speed = data.Find("Speed").gameObject.GetComponent<Text>();

        pressedColor.a = 1;
        releasedColor.a = 1;

        SwitchCar(0);

        //Debug.Log(currentCarAgent);
    }

    private void UpdateKeys(int[] keys) {
        UP.color = keys[0] == 1 ? pressedColor : releasedColor;
        DOWN.color = keys[1] == 1 ? pressedColor : releasedColor;
        LEFT.color = keys[2] == 1 ? pressedColor : releasedColor;
        RIGHT.color = keys[3] == 1 ? pressedColor : releasedColor;
        SPACE.color = keys[4] == 1 ? pressedColor : releasedColor;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) SwitchCar(carIndex+1); 
        if (Input.GetKeyDown(KeyCode.LeftArrow)) SwitchCar(carIndex-1); 


        UpdateKeys(currentCarAgent.outputs);
    }

    private void SwitchCar(int i) {
        i = i % cars.transform.childCount;
        if (currentCarController) currentCarController.useUI = false;

        GameObject currentCar = cars.transform.GetChild(i).gameObject;

        cameraFollow.carTransform = currentCar.transform;
        currentCarAgent = currentCar.GetComponent<CarAgent>();
        currentCarController = currentCar.GetComponent<PrometeoCarController>();
        carIndex = i;

        currentCarController.useUI = true;
        currentCarController.carSpeedText = Speed;
    }
}
