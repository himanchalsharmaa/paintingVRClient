using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//using UnityEngine.InputSystem;
using UnityEngine.XR;
public class controllerInputActions : MonoBehaviour
{

    private UnityEngine.XR.InputDevice leftHand;
    private UnityEngine.XR.InputDevice rightHand;
    public TMP_Text info1;

    [System.NonSerialized]
    public string btnpress = "";
    private  InputDeviceCharacteristics controllerCharacteristics;

#if UNITY_EDITOR
    private InputAction _lefttriggerAction;
    private InputAction _righttriggerAction;
    private InputAction _leftprimaryAction;
    private InputAction _rightprimaryAction;
    private InputAction _leftsecondaryAction;
    private InputAction _rightsecondaryAction;
#endif


    void Start() //controllerInputActions
    {
     //   Application.logMessageReceived += HandleLog;
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        foreach(var dev in devices){
            if(dev.name.Contains("Left")){
                leftHand = dev;
            }
            if(dev.name.Contains("Right")){
                rightHand = dev;
            }
        }

#if UNITY_EDITOR
        _lefttriggerAction = new InputAction("Trigger", binding: "<XRController>{LeftHand}/triggerButton");
        _lefttriggerAction.Enable();

        _righttriggerAction = new InputAction("Trigger", binding: "<XRController>{RightHand}/triggerButton");
        _righttriggerAction.Enable();

        _leftprimaryAction = new InputAction("Trigger", binding: "<XRController>{LeftHand}/primaryButton");
        _leftprimaryAction.Enable();

        _rightprimaryAction = new InputAction("Trigger", binding: "<XRController>{RightHand}/primaryButton");
        _rightprimaryAction.Enable();

        _leftsecondaryAction = new InputAction("Trigger", binding: "<XRController>{LeftHand}/secondaryButton");
        _leftsecondaryAction.Enable();

        _rightsecondaryAction = new InputAction("Trigger", binding: "<XRController>{RightHand}/secondaryButton");
        _rightsecondaryAction.Enable();
#endif

    }
    private void Update()
    {
        btnpress = "";

#if UNITY_EDITOR

        if (_lefttriggerAction.ReadValue<float>() > 0.1f)
        {
            btnpress += "00";
        }
        if (_righttriggerAction.ReadValue<float>() > 0.1f)
        {
            btnpress += "10";
        }
        if (_leftprimaryAction.ReadValue<float>() > 0.1f)
        {
            btnpress += "01";
        }
        if (_rightprimaryAction.ReadValue<float>() > 0.1f)
        {
            btnpress += "11";
        }
        if (_leftsecondaryAction.ReadValue<float>() > 0.1f)
        {
            btnpress += "02";
        }
        if (_rightsecondaryAction.ReadValue<float>() > 0.1f)
        {
            btnpress += "12";
        }

#endif
        leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerleft);
        rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerright);
        leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryButtonleft);
        rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryButtonright);
        leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryleft);
        rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryright);

        if (triggerleft>0)
        {
            btnpress += "00";
          //  Debug.Log("triggerleft value:" + triggerleft);
        }
        if (triggerright>0)
        {
            btnpress += "10";
         //   Debug.Log("triggerright value:" + triggerright);
        }
        if (primaryButtonleft)
        {
            btnpress += "01";
            //Debug.Log("primaryButtonleft value:" + primaryButtonleft);
        }
        if (primaryButtonright)
        {
            btnpress += "11";
          //  Debug.Log("primaryButtonright value:" + primaryButtonright);
        }
        if (secondaryleft)
        {
            btnpress += "02";
           // Debug.Log("secondaryleft value:" + secondaryleft);
        }
        if (secondaryright)
        {
            btnpress += "12";
          //  Debug.Log("secondaryright value:" + secondaryright);
        }
        if(btnpress != ""){
        Debug.Log(btnpress);
        }
        
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        
       info1.text = logString ; //+ " : " + framecount
        //info1.text += logString + "<br>";
    }
}