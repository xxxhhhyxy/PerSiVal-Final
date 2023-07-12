using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using System;
using UnityEngine.XR.ARSubsystems;
public class ParticalVisualizer : MonoBehaviour
{
    bool isInited = false;
    bool isRendered = false;
    ParticleSystem m_ParticleSystem;
    public ParticleSystem.Particle[] m_Particles { get; private set; }
    GameObject bicepsObj;
    GameObject[] bicepsObjPoints;
    public void f_Init()
    {
        bicepsObj = new GameObject("Biceps");
        bicepsObjPoints = new GameObject[GlobalCtrl.M_MeshCtrl.M_Vertices.Length];
        for (int i = 0; i < GlobalCtrl.M_MeshCtrl.M_Vertices.Length; i++)
        {
            bicepsObjPoints[i] = new GameObject(i.ToString());
            bicepsObjPoints[i].transform.SetParent(bicepsObj.transform);
        }
        InitParticle();
        //m_ParticleSystem = GetComponent<ParticleSystem>();
        //m_Particles = new ParticleSystem.Particle[GlobalCtrl.M_MeshCtrl.M_Vertices.Length];
        isInited = true;
    }

    private void InitParticle()
    {
        if (m_ParticleSystem == null)        
            m_ParticleSystem = GetComponent<ParticleSystem>();
      
        if (m_Particles == null||m_Particles.Length< GlobalCtrl.M_MeshCtrl.M_Vertices.Length)
            m_Particles= new ParticleSystem.Particle[GlobalCtrl.M_MeshCtrl.M_Vertices.Length];
    }

    public void ChangeStatus(bool isOn)
    {
        isRendered = isOn;
        if (isOn && !isInited)
            f_Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void RefreshParticle()
    {

    }

    // Update is called once per frame
    public void UpdateVisualizer()
    {
        if (!isInited)
            return;
        if (!GlobalCtrl.M_Instance.M_HumanBodyTracker.isTracked)
            return;
        if (!isRendered)
            return;

        InitParticle();

        bicepsObj.transform.position = GlobalCtrl.M_Instance.LShoulder;
        for (int i = 0; i < GlobalCtrl.M_MeshCtrl.M_Vertices.Length; i++)
        {
            bicepsObjPoints[i].transform.localPosition = GlobalCtrl.M_MeshCtrl.M_Vertices[i];

            m_Particles[i].startColor = m_ParticleSystem.main.startColor.color;
            m_Particles[i].startSize = m_ParticleSystem.main.startSize.constant;
            m_Particles[i].position = bicepsObjPoints[i].transform.position;
            m_Particles[i].remainingLifetime = 1f;
        }
        m_ParticleSystem.SetParticles(m_Particles, GlobalCtrl.M_MeshCtrl.M_Vertices.Length);
        
    }
}
