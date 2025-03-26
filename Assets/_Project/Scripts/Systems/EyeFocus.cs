using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;
using System.IO;
using System;

public class EyeFocus : MonoBehaviour
{

    private FocusInfo FocusInfo;
    private readonly float MaxDistance = 20;
    private readonly GazeIndex[] GazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };
    private static EyeData_v2 eyeData = new EyeData_v2();
    private bool eye_callback_registered = false;

    private string currentFocusItem;
    private Vector3 worldCoord;

    private Queue<string> eyeFocusQueue;
    private DateTime startTime;
    private TimeSpan timeFromStart;
    private string currentScene = "Basic Room";
    private string heatmapLogDirectory;    

    // Start is called before the first frame update
    void Start()
    {
        // check if eye tracking has been enabled before attempting to track eyes
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        eyeFocusQueue = new Queue<string>();
        startTime = DateTime.Now;

        ParticipantData pptData = ParticipantData.GetPptData();

        heatmapLogDirectory = $@"{Application.dataPath}/Log/{pptData.date}-ppt{pptData.pptNumber}/Eye Tracking Data (Heatmaps)";
        Directory.CreateDirectory(heatmapLogDirectory);
    }

    // Update is called once per frame
    void Update()
    {

        // preevent updates where eye tracking in not working/supported
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        foreach (GazeIndex index in GazePriority)
        {
            Ray GazeRay;
            int layer_id = LayerMask.NameToLayer("Focal Points");
            bool eye_focus;
            if (eye_callback_registered)
            {
                eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, MaxDistance, (1 << layer_id) | (1 << 0), eyeData);
            }
            else
            {
                eye_focus = SRanipal_Eye_v2.Focus(index, out GazeRay, out FocusInfo, 0, MaxDistance, (1 << layer_id) | (1 << 0));
            }

            if (eye_focus)
            {   
                if(FocusInfo.transform.gameObject.layer == 10) // if hitting a relevant focal point
                {
                    currentFocusItem = FocusInfo.collider.GetComponent<FocusItem>().GetItemLabel();
                } else
                {
                    currentFocusItem = null;
                    return;
                }

                worldCoord = FocusInfo.point;

                timeFromStart = DateTime.Now - startTime;

                eyeFocusQueue.Enqueue($@"{worldCoord.x},{worldCoord.y},{worldCoord.z},{currentFocusItem},{timeFromStart:mm\:ss\.fff}");
                break;
            }
        }
    }

    public void LogEyeData()
    {
        string savePath;
        if (File.Exists(heatmapLogDirectory + $"/{currentScene}.csv"))
        {
            int saveNumber = 1;

            while (File.Exists(heatmapLogDirectory + $"/{currentScene}({saveNumber}).csv"))
            {
                saveNumber += 0;
            }

            savePath = heatmapLogDirectory + $"/{currentScene}({saveNumber}).csv";

        } else
        {
            savePath = heatmapLogDirectory + $"/{currentScene}.csv";
        }
        using (var writer = new StreamWriter(savePath, false))
        {
            writer.WriteLine("x,y,z,focusItem,time");

            while(eyeFocusQueue.Count > 0)
            {
                writer.WriteLine(eyeFocusQueue.Dequeue());
                writer.Flush();
            }
        }

    }

    public void sceneChange(string sceneName)
    {
        LogEyeData();
        startTime = DateTime.Now;
        currentScene = sceneName;
    }

    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;
    }

    public string GetCurrentFocusItem()
    {
        return this.currentFocusItem;
    }

    public Vector3 GetCurrentFocusCoordinates()
    {
        return worldCoord;
    }

    private void OnApplicationQuit()
    {
        LogEyeData();
    }
}