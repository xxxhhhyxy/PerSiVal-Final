using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Barracuda;
using UnityEngine;

public class LocalNN : MonoBehaviour
{
    private readonly LocalBarracudaAdapter barracudaAdapter;
    public LocalNN(NNModel modelAsset)
    {
        Debug.Log("Creation of Distributed NN....");
       
        //load local model
        this.barracudaAdapter = new LocalBarracudaAdapter(modelAsset);
        Debug.Log("Local NN has been built");
    }

    public float[] Predict(float[] input)
    {
        return barracudaAdapter.Predict(input);
    }
}
