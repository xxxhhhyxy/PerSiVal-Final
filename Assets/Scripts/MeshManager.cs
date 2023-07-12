using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructClass;
using Unity.Barracuda;
using System;

public class MeshManager : MonoBehaviour
{
    public MuscleStruct<TextAsset> DistriAssets;
    public MuscleStruct<NNModel> ModelAssets;
    public Dictionary<MuscleEnum, MuscleClass> Dic_trackedMuscle { get; private set; }
    private bool isInited = false;
    public void f_Init()
    {
        Dic_trackedMuscle = new Dictionary<MuscleEnum, MuscleClass>();
        //iterate each muscle in the enumeration
        foreach (MuscleEnum a in Enum.GetValues(typeof(MuscleEnum)))
        {
            //if the corresponding toggle ui is on
            if (GlobalCtrl.M_UIManager.tg_muscle.SearchMuscle(a).isOn)
            {
                //we create a muscleclass for this muscle
                Dic_trackedMuscle.Add(a, new MuscleClass(a, DistriAssets.SearchMuscle(a), ModelAssets.SearchMuscle(a)));
            }
        }
        isInited = true;
    }

    /// <summary>
    /// this is called by GlobalCtrl.cs
    /// </summary>
    public void UpdateMesh()
    {
        if (!isInited)
            return;
        if (!GlobalCtrl.M_UIManager.tg_tracked.isOn)
            return;
        //Debug.Log("isInited");
        foreach (var a in Dic_trackedMuscle)
            a.Value.UpdateMesh(GlobalCtrl.M_ActiManager.activations);
    }
}
