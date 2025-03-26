using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable] public class ParticipantData
{

    public string date;
    public int pptNumber;

    private static string saveDir = Application.dataPath + "/SaveData";
    private static string pptDataFilePath = saveDir + "/ParticipantData.json";

    public ParticipantData(DateTime date, int ParticipantNumber)
    {
        this.date = $"{date.Date:yyyy-MM-dd}";
        this.pptNumber = ParticipantNumber;
    }

    public ParticipantData(string date, int ParticipantNumber)
    {
        this.date = date;
        this.pptNumber = ParticipantNumber;
    }

    public static ParticipantData GetPptData()
    {

        if (File.Exists(pptDataFilePath))
        {
            string jsonStr = File.ReadAllText(pptDataFilePath);
            return JsonUtility.FromJson<ParticipantData>(jsonStr);
        }
        else
        {
            return null;
        }
    }

    public static ParticipantData GetNextParticipantID()
    {
        ParticipantData previousPptData = GetPptData();

        if (previousPptData == null)
        {
            return new ParticipantData(DateTime.Now.Date, 0);
        }
        else
        {
            if (previousPptData.date == $"{DateTime.Now:yyyy-MM-dd}")
            {
                return new ParticipantData(previousPptData.date, previousPptData.pptNumber + 1);
            }
            else
            {
                return new ParticipantData(DateTime.Now.Date, 0);
            }
        }
    }

    public void Save()
    {

        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }

        File.WriteAllText(pptDataFilePath, JsonUtility.ToJson(this));
    }
}
