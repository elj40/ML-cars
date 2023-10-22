using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUI : MonoBehaviour
{

    public Graphic[] outputKeys = new Graphic[5];
    public CarAgent carAgent;

    public Color releasedColour;
    public Color pressedColour;


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < carAgent.outputs.Length; i++) {
            outputKeys[i].color = (carAgent.outputs[i] == 1) ? pressedColour : releasedColour;
        }
    }
}
