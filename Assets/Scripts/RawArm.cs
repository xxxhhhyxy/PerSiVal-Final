using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructClass;
using System;

public class RawArm : MonoBehaviour
{
    public Transform shoulder;
    public Transform elbow;
    public Transform wrist;
    public Vector3 RawS2E { get => elbow.position - shoulder.position; }
    public Vector3 RawE2W { get => wrist.position - elbow.position; }
    private Vector3 lowerNormal;
    private Vector3 upperNormal;
    public MeshFilter upperBoneObj;
    public MeshFilter lowerBoneObj;
    public MuscleStruct<MeshFilter> muscleObj;
    public GameObject upperTrans { get; private set; }
    public GameObject lowerTrans { get; private set; }
    //public MuscleStruct<List<TendonInfo>> tendonInfo;
    public MuscleStruct<TendonInfo> tendonInfo;
    private bool isInited = false;

    public Dictionary<MuscleEnum, GameObject> dic_UpperEnds;
    public Dictionary<MuscleEnum, GameObject> dic_LowerEnds;
    public Dictionary<MuscleEnum, float> dic_rot;


    public void f_Init()
    {
        dic_UpperEnds = new Dictionary<MuscleEnum, GameObject>();
        dic_LowerEnds = new Dictionary<MuscleEnum, GameObject>();
        dic_rot= new Dictionary<MuscleEnum, float>();

        lowerNormal = Vector3.ProjectOnPlane(-RawS2E, RawE2W).normalized;
        upperNormal = Vector3.ProjectOnPlane(RawE2W, RawS2E).normalized;


        foreach (var a in GlobalCtrl.M_MeshManager.Dic_trackedMuscle)
        {


            GameObject tendonMarkL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tendonMarkL.transform.localScale = Vector3.one * 0.3f;
            GameObject tendonMarkU = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tendonMarkU.transform.localScale = Vector3.one * 0.3f;
            tendonMarkL.transform.position = MuscleClass.ReflectX(tendonInfo.SearchMuscle(a.Key).lowerPos);
            tendonMarkU.transform.position = MuscleClass.ReflectX(tendonInfo.SearchMuscle(a.Key).upperPos);



            GameObject upperEndObj = new GameObject(a.Key + "_UpEnd");
            upperEndObj.transform.position =MuscleClass.ReflectX( tendonInfo.SearchMuscle(a.Key).upperPos);
            upperEndObj.transform.SetParent(upperBoneObj.transform);
            dic_UpperEnds.Add(a.Key, upperEndObj);

            GameObject lowerEndObj = new GameObject(a.Key + "_LowerEnd");
            lowerEndObj.transform.position = MuscleClass.ReflectX(tendonInfo.SearchMuscle(a.Key).lowerPos); 
            lowerEndObj.transform.SetParent(lowerBoneObj.transform);
            dic_LowerEnds.Add(a.Key, lowerEndObj);

            //GameObject muscleTrans = new GameObject(a.Key+"_Trans");
            //muscleTrans.transform.position = (lowerEndObj.transform.position+upperEndObj.transform.position)/2;
            //muscleTrans.transform.LookAt(upperEndObj.transform.position,lowerNormal);
            //muscleObj.SearchMuscle(a.Key).transform.SetParent(muscleTrans.transform);
            //dic_muscleTrans.Add(a.Key, muscleTrans);

            Vector3 thisUp = Vector3.ProjectOnPlane(muscleObj.SearchMuscle(a.Key).transform.up, RawS2E).normalized;
            dic_rot.Add(a.Key, Vector3.SignedAngle(upperNormal,thisUp, RawS2E));
        }

        upperTrans = new GameObject("upperTrans");
        upperTrans.transform.position = upperBoneObj.mesh.bounds.center;
        upperTrans.transform.LookAt(shoulder,upperNormal);
        upperBoneObj.transform.SetParent(upperTrans.transform);

        lowerTrans = new GameObject("lowerTrans");
        lowerTrans.transform.position = lowerBoneObj.mesh.bounds.center;
        lowerTrans.transform.LookAt(elbow, lowerNormal);
        lowerBoneObj.transform.SetParent(lowerTrans.transform);
        isInited = true;
    }

    public void UpdateBone()
    {
        if (!isInited)
            return;
        upperTrans.transform.position = (GlobalCtrl.M_TrackManager.LShoulder + GlobalCtrl.M_TrackManager.LElbow) / 2;
        upperTrans.transform.LookAt(GlobalCtrl.M_TrackManager.LShoulder, GlobalCtrl.M_TrackManager.UpperNormal);
        upperTrans.transform.localScale = GlobalCtrl.M_TrackManager.LS2E.magnitude / RawS2E.magnitude * Vector3.one;

        lowerTrans.transform.position = (GlobalCtrl.M_TrackManager.LWrist + GlobalCtrl.M_TrackManager.LElbow) / 2;
        lowerTrans.transform.LookAt(GlobalCtrl.M_TrackManager.LElbow, GlobalCtrl.M_TrackManager.LowerNormal);
        lowerTrans.transform.localScale = GlobalCtrl.M_TrackManager.LE2W.magnitude / RawE2W.magnitude * Vector3.one;
    }


}
