using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

/// <summary>
/// this script is for muscle activation level
/// input: the elbow movement data
/// output: the activation level of 5 different muscles
/// </summary>
public class ActiManager : MonoBehaviour
{
    public NNModel modelAsset;
    private Model myRuntimeModel;
    private IWorker myWorker;
    /// <summary>
    /// the elbow angle of current frame
    /// </summary>
    public float CurElbowAngle { get; private set; }
    /// <summary>
    /// the elbow angle of last frame
    /// </summary>
    public float LastElbowAngle { get; private set; }
    public float CurVelocity { get; private set; }
    public float LastVelocity { get; private set; }
    public float CurAcceleration { get; private set; }
    public float CurWeight { get; private set; }
    /// <summary>
    /// the final output of the muscle activation neural network
    /// </summary>
    public float[] activations { get; private set; }
    private bool isInited = false;
    /// <summary>
    /// initialization of the activation level calculator
    /// </summary>
    public void f_Init()
    {
        CurWeight = 0;
        myRuntimeModel = ModelLoader.Load(modelAsset);
        myWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, myRuntimeModel);
        isInited = true;
    }
    public void UpdateActivation()
    {
        if (!isInited)
            return;
        if (!GlobalCtrl.M_UIManager.tg_tracked.isOn)
            return;
        CurElbowAngle = Vector3.Angle(GlobalCtrl.M_TrackManager.LWrist - GlobalCtrl.M_TrackManager.LElbow,
            GlobalCtrl.M_TrackManager.LShoulder - GlobalCtrl.M_TrackManager.LElbow);
        CurVelocity = CurElbowAngle - LastElbowAngle;
        CurAcceleration = CurVelocity - LastVelocity;
        LastElbowAngle = CurElbowAngle;
        LastVelocity = CurVelocity;
        activations = GetActivationsFromData(GetInputData());
        //float[] hh = GetInputData();
        //Debug.Log(hh[0] + "/" + hh[1] + "/" + hh[2] + "/" + hh[3] + "/");
        //Debug.Log(activations[0]+"/"+ activations[1]+"/"+ activations[2]);
        //GlobalCtrl.M_UIManager.f_txt_debug(activations[0].ToString());

        //Debug.Log(activations[0] + "/" + activations[1]);
    }
    public float[] GetInputData()
    {
        return new float[4] {
                CurWeight,
                CurElbowAngle,
                CurVelocity,
                CurAcceleration };
    }
    public void SetWeight(float input)
    {
        CurWeight = input;
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


        int[] shape = { -1, input.Length };
        Tensor inTensor = new Tensor(shape, modelInput);
        Tensor outTensor = myWorker.Execute(inTensor).PeekOutput();
        new WaitForCompletion(outTensor);
        inTensor.Dispose();
        return outTensor.AsFloats();
    }
}
