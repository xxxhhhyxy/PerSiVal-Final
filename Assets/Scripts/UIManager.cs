using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StructClass;
using System;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// switch on/off debug panel
    /// </summary>
    public Toggle tg_debug;

    public GameObject panel;
    /// <summary>
    /// uninteractive, this is the indicator of tracking status
    /// </summary>
    public Toggle tg_tracked;
    /// <summary>
    /// activate the activation level NNModel
    /// </summary>
    public Toggle tg_acti;
    /// <summary>
    /// the toggle group for all muscles
    /// </summary>
    public MuscleStruct<Toggle> tg_muscle;
    /// <summary>
    /// activate the mesh calculation
    /// </summary>
    public Toggle tg_mesh;
    /// <summary>
    /// activate the mesh rendering
    /// </summary>
    public Toggle tg_render;
    public Toggle tg_pointcloud;
    public Slider sli_weight;
    public Text txt_debug;
    /// <summary>
    /// 
    /// </summary>
    public void f_Init()
    {
        tg_debug.onValueChanged.AddListener(f_tg_debug);
        tg_acti.onValueChanged.AddListener(f_tg_acti);
        tg_mesh.onValueChanged.AddListener(f_tg_mesh);
        tg_render.onValueChanged.AddListener(f_tg_render);
        sli_weight.onValueChanged.AddListener(f_Sli_Weight);
        tg_tracked.isOn = false;
        tg_render.interactable = false;

    }
    private void f_tg_debug(bool isOn)
    {
        panel.SetActive(isOn);
    }
    private void f_tg_acti(bool isOn)
    {
        GlobalCtrl.M_ActiManager.f_Init();
        //tg_mesh.gameObject.SetActive(true);
        if (isOn)
            tg_acti.interactable = false;
    }
    private void f_tg_mesh(bool isOn)
    {
        Debug.Log("mesh activated");
        GlobalCtrl.M_MeshManager.f_Init();
        if (isOn)
        {
            tg_mesh.interactable = false;
            tg_render.interactable = true;
            foreach (MuscleEnum a in Enum.GetValues(typeof(MuscleEnum)))
            {
                tg_muscle.SearchMuscle(a).interactable = false; ;
            }
        }
    }
    private void f_tg_render(bool isOn)
    {
        tg_pointcloud.interactable = false;
        GlobalCtrl.M_FaceVisualizer.f_Init(tg_pointcloud.isOn);

    }
    public void f_Sli_Weight(float input)
    {
        if (input < 5)
            GlobalCtrl.M_ActiManager.SetWeight(0);
        else if (input < 10)
            GlobalCtrl.M_ActiManager.SetWeight(5);
        else if (input < 20)
            GlobalCtrl.M_ActiManager.SetWeight(10);
        else
            GlobalCtrl.M_ActiManager.SetWeight(20);
        //Txt_Weight.text = input.ToString();

    }
    public void f_txt_debug(string input)
    {
        //txt_debug.text += input;
        txt_debug.text = input;
    }
   
}
