using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShowFPS : MonoBehaviour
{
    public float showTime = 1f;

    private int count = 0;
    private float deltaTime = 0f;

    public string fileName = "fps-record";
    private TextWriter tw;
    // Start is called before the first frame update
    void Start()
    {
        fileName = fileName + (Random.Range(1,10000)).ToString("N4") + ".txt";
        if (File.Exists(fileName))
        {
            Debug.Log(fileName + " already exists.");
        }
        tw = File.CreateText(fileName);
    }

    // Update is called once per frame
    void Update()
    {

        count++;
        deltaTime += Time.deltaTime;
        //count fps every 1 second
        if (deltaTime > showTime)
        {
            float fps = count / deltaTime;
            float milliSecond = deltaTime * 1000 / count;
            string strFPS = string.Format("{0:0.0} fps, rendering interval: {1:0.0}ms", fps,milliSecond);
            GlobalCtrl.M_UIManager.f_txt_debug(strFPS);
            tw.WriteLine(strFPS);
            count = 0;
            deltaTime = 0f;

        }
    }

    private void OnDestroy()
    {
        tw.Close();
    }

}
