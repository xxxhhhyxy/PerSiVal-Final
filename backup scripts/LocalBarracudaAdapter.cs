using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Barracuda;

public class LocalBarracudaAdapter : MonoBehaviour
{
    private Model myRuntimeModel;
    private IWorker myWorker;
    private NNModel modelAsset;

    public LocalBarracudaAdapter(NNModel modelAsset)
    {
        Debug.Log("Loading local Model...");
        myRuntimeModel = ModelLoader.Load(modelAsset);
        myWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, myRuntimeModel);
        Debug.Log("Loaded Barracuda Model");
    }


    private Tensor Tensorize(float[] input)
    {
        int[] shape = { -1, input.Length };
        Tensor tensor = new Tensor(shape, input);
        return tensor;
    }


    public float[] Predict(float[] input)
    {
        Tensor inTensor = Tensorize(input);
        Tensor outTensor = myWorker.Execute(inTensor).PeekOutput();
        new WaitForCompletion(outTensor);
        inTensor.Dispose();
        return outTensor.AsFloats();
    }
}
