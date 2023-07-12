using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using Unity.Collections;
public class MuscleActiCtrl : MonoBehaviour
{
    LocalNN myNetwork;
    bool isInited = false;
    // Local neuronal network
    public NNModel modelAsset;
    public float CurElbowAngle { get; private set; }
    public float LastElbowAngle { get; private set; }
    public float CurVelocity { get; private set; }
    public float LastVelocity { get; private set; }
    public float CurAcceleration { get; private set; }
    public float CurWeight { get; private set; }
    /// <summary>
    /// the final output of the muscle activation neural network
    /// </summary>
    public float[] activations { get; private set; }
    /// <summary>
    /// when activating color shading, the color of the biceps will interpolate between red and this color
    /// </summary>
    public Color DestinColor;
    public enum Weights : int
    {
        weight0=0,
        weight5=5,
        weight10=10,
        weight20=20,
    }
    public void SetWeight(float input)
    {
        CurWeight = input;
    }

    // Update is called once per frame
    public void UpdateActivation()
    {
        if (!isInited)
            return;
        if (!GlobalCtrl.M_TrackManager.m_HumanBodyTracker.isTracked)
            return;
        CurElbowAngle = Vector3.Angle(GlobalCtrl.M_Instance.LWrist - GlobalCtrl.M_Instance.LElbow, GlobalCtrl.M_Instance.LShoulder - GlobalCtrl.M_Instance.LElbow);
        CurVelocity = CurElbowAngle - LastElbowAngle;
        CurAcceleration = CurVelocity - LastVelocity;
        LastElbowAngle = CurElbowAngle;
        LastVelocity = CurVelocity;

        activations = GetActivationsFromData(GetInputData());
        if (GlobalCtrl.M_UIManager.Tg_ColorShading.isOn)
        {
            GlobalCtrl.M_Instance.Mat_Biceps.color = Color.Lerp(Color.red, DestinColor, activations[0] * GetMaxActivation((int)CurWeight));
        }
        else
        {
            GlobalCtrl.M_Instance.Mat_Biceps.color = Color.red;
        }
    }

    /// <summary>
    /// call by the UIManager.cs
    /// </summary>
    public void f_Init()
    {
        myNetwork = new LocalNN(modelAsset);
        isInited = true;
    }
    /// <summary>
    /// Calls the predict method of the neuronal network and returns the result.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public float[] GetActivationsFromData(float[] input)
    {
        float weight = 0;
        switch ((int)input[0])
        {
            case 0:
                weight = 0;
                break;
            case 5:
                weight = 1;
                break;
            case 10:
                weight = 2;
                break;
            case 20:
                weight = 3;
                break;
        }
        float[] modelInput = { 1 - input[1] / 180, input[2] / 500, input[3] / 10000, weight };
        return myNetwork.Predict(modelInput);
    }

    public float[] GetInputData()
    {
        return new float[4] {
                CurWeight,
                CurElbowAngle,
                CurVelocity,
                CurAcceleration };
    }


    
    /// <summary>
    /// Returns the maximum possible activation depending on the weight
    /// </summary>
    /// <param name="weight"></param>
    /// <returns></returns>
    private float GetMaxActivation(int weight)
    {
        switch (weight)
        {
            case 0:
                return 0.4f;
            case 5:
                return 0.6f;
            case 10:
                return 0.8f;
            case 20:
                return 1.0f;
        }
        return 0.4f;
    }
}
