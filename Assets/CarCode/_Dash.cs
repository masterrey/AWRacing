using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Dash : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float dashTime;
    [SerializeField] private GameObject speedIndicator;
    [SerializeField] private GameObject rpmIndicator;
    [SerializeField] private _CarControl carControl;
    [SerializeField] private Text rpmText;
    [SerializeField] private Text speedText;
    [SerializeField] private Text gearText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (carControl == null)
        {
            carControl = GameObject.Find("Car").GetComponent<_CarControl>();
        }
        speed = carControl.GetSpeed();
        speedText.text = speed.ToString("000");
        rpmText.text = carControl.GetRPM().ToString("0000");
        speedIndicator.transform.localRotation = Quaternion.Euler(0, 0, speed-100);
        rpmIndicator.transform.localRotation = Quaternion.Euler(0, 0, carControl.GetRPM() * 0.01f);
        gearText.text = carControl.GetGear().ToString("0");

    }
}
