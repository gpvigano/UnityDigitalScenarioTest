using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiScenFw;
using UnityDigitalScenario;

public class DiScenApiTest : ScenarioManager
{
    private bool firstFrame = true;

#if UNITY_EDITOR
    [Tooltip("Copy of the list of scenario elements (just for inspecting the scenario).")]
    [SerializeField]
    protected ScenarioElement[] scenarioElementsCache;
#endif


    protected override void OnSyncScene()
    {
        base.OnSyncScene();
#if UNITY_EDITOR
        scenarioElementsCache = new ScenarioElement[Root.ElementsCount()];
        Root.GetElements().CopyTo(scenarioElementsCache, 0);
#endif
        //Debug.Log("SyncSceneEvent");
        DiScenApiUnity.Log("SyncSceneEvent", LogLevel.Debug, "DiScenApiTest", true, true);
        EntityData[] entities = DiScenApiUnity.GetEntities();
        string msg = "Entities:";
        foreach (EntityData entity in entities)
        {
            msg += " " + entity.Identifier + "(" + entity.Type + "):";
            msg += entity.Asset.Uri + "[" + entity.Asset.Catalog + "]";
            msg += DiScenApiUnity.GetElementLocation(entity.Identifier).ToString();
        }
        Debug.Log(msg);
        ElementData[] elements = DiScenApiUnity.GetElements();
        msg = "Elements:";
        Root.Clear();
        foreach (ElementData element in elements)
        {
            msg += "\n ";
            //msg += element.ClassName + ":";
            msg += element.Identifier + ",";
            msg += element.Type + ",";
            msg += element.Category + ",";
            msg += element.Description + ",";
            msg += "\n  Asset: ";
            msg += element.Asset.Source.ToString() + ",";
            msg += element.Asset.Catalog + ",";
            msg += element.Asset.AssetType + ",";
            msg += element.Asset.Uri + ",";
            msg += element.Asset.PartId;
            msg += "\n  ParentId: " + element.LocalTransform.ParentId;
            Debug.Log(msg);
            DiScenApiUnity.Log(element.Type + " " + element.Identifier, LogLevel.Verbose, "DiScenApiTest", false, true);
        }
        DiScenApiUnity.Log(elements.Length.ToString() + " elements defined in the scenario.", LogLevel.Log, "DiScenApiTest", false, true);
    }


    protected override void OnSyncScenario()
    {
        base.OnSyncScenario();
        DiScenApiUnity.Log("SyncScenarioEvent", LogLevel.Debug, "DiScenApiTest", true, true);
        //Debug.Log("SyncScenarioEvent");
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        scenarioFileName = "sample_scenario";
        unitScale = 0.01f;
    }
#endif


    protected override void Start()
    {
        base.Start();
        //LoadScenario();
    }


    protected override void Update()
    {
        base.Update();
        if (firstFrame)
        {
            LoadScenario();
            firstFrame = false;
        }
    }


}
