using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InputProfileSaver : MonoBehaviour
{
    private string jsonPath = "this is the json file path";
    
    //function for reading all to json file
    public void WriteProfileToJSON(List<InputProfile> profile)
    {
        var json = JsonUtility.ToJson(profile);
        File.WriteAllText(Application.persistentDataPath + jsonPath, json);
    }
    
    //function for reading all from json file
        //could be to specific list??? (profile selector)
    public List<InputProfile> ReadProfilesFromJSON(List<InputProfile> outList)
    {
        var profileString = File.ReadAllText(Application.persistentDataPath + jsonPath);
        
        return outList = JsonUtility.FromJson<List<InputProfile>>(profileString);
    }
}