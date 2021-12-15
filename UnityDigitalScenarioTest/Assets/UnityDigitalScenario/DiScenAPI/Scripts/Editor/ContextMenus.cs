#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityDigitalScenario;
using ElectronicCircuit;

public class ContextMenus : MonoBehaviour
{

    [MenuItem("CONTEXT/ScenarioManager/Save Scenario")]
    static void SaveScenario(MenuCommand menuCommand)
    {
        ScenarioManager scenario = menuCommand.context as ScenarioManager;
        scenario.SaveScenario();
    }

    [MenuItem("CONTEXT/ScenarioManager/Load Scenario")]
    static void LoadScenario(MenuCommand menuCommand)
    {
        ScenarioManager scenario = menuCommand.context as ScenarioManager;
        scenario.LoadScenario();

        LedCircuitTest ledCirc = FindObjectOfType<LedCircuitTest>();
        ledCirc.UpdateComponentsList();
        ledCirc.NewEpisode();
    }

    [MenuItem("CONTEXT/ScenarioElement/Copy Transform To Data")]
    static void TransformToData(MenuCommand menuCommand)
    {
        ScenarioElement elem = menuCommand.context as ScenarioElement;
        elem.TransformToData();
    }

    [MenuItem("CONTEXT/ScenarioElement/Copy Data To Transform")]
    static void DataToTransform(MenuCommand menuCommand)
    {
        ScenarioElement elem = menuCommand.context as ScenarioElement;
        elem.DataToTransform();
    }
}
#endif
