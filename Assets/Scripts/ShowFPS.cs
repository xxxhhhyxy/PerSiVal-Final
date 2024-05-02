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
    float _updateInterval = 1f;
    float _accum = 0;
    int _frame = 0;
    float timeleft;

    // Start is called before the first frame update
    void Start()
    {
        timeleft = _updateInterval;
    }

    // Update is called once per frame
    void Update()
    {

        //double fps = 1.0f / Time.deltaTime;
        //GlobalCtrl.M_UIManager.f_txt_debug(Time.deltaTime.ToString("F2")+"/"+fps.ToString("F2"));

        timeleft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        _frame++;
        if(timeleft<=0)
        {
            float fps = _accum / _frame;
            GlobalCtrl.M_UIManager.f_txt_debug(Time.deltaTime.ToString("F2") + "/" + fps.ToString("F6"));
            timeleft = _updateInterval;
            _accum = 0;
            _frame = 0; 
        }
    }

    private void OnDestroy()
    {
        //tw.Close();
    }

}
