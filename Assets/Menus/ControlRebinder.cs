using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlRebinder : MonoBehaviour
{
    [SerializeField] private InputActionReference inputActionRef = null;
    public string actionName = "Unknown";
    public TMP_Text actionNameText = null;
    public string buttonLabelInitial = "Unknown";
    public TMP_Text buttonLabel = null;
    public bool isCompositeBinding = false;
    public int compositeBindingIndex = 1;
    public GameObject startRebindObject = null;
    public GameObject inputWaitObject = null;
    public InputActionAsset inputActionAsset = null;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    public void Start()
    {
        actionNameText.SetText(actionName);
        int bindingIndex = 0;
        if (!isCompositeBinding)
            bindingIndex = inputActionRef.action.GetBindingIndexForControl(inputActionRef.action.controls[0]);
        else
            bindingIndex = compositeBindingIndex;
        buttonLabel.text = InputControlPath.ToHumanReadableString(inputActionRef.action.bindings[bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    public void StartRebinding()
    {
        startRebindObject.SetActive(false);
        inputWaitObject.SetActive(true);
        if(!isCompositeBinding)
        {
            Debug.Log("Normal Rebind");
            rebindOperation = inputActionRef.action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindingComplete())
            .Start();
        } else
        {
            Debug.Log("Composite Rebind");
            rebindOperation = inputActionRef.action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse/delta")
            .WithTargetBinding(compositeBindingIndex)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindingComplete())
            .Start();
        }
        
    }

    private void RebindingComplete()
    {
        Debug.Log("Rebind Complete");
        int bindingIndex = 0;
        if (!isCompositeBinding)
            bindingIndex = inputActionRef.action.GetBindingIndexForControl(inputActionRef.action.controls[0]);
        else
            bindingIndex = compositeBindingIndex;
        buttonLabel.text = InputControlPath.ToHumanReadableString(inputActionRef.action.bindings[bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        rebindOperation.Dispose();
        startRebindObject.SetActive(true);
        inputWaitObject.SetActive(false);
        SaveRebinds(inputActionAsset);
    }

    public void SaveRebinds(InputActionAsset actionAsset)
    {
        string rebinds = actionAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds", rebinds);
        PlayerPrefs.Save();
    }
}
