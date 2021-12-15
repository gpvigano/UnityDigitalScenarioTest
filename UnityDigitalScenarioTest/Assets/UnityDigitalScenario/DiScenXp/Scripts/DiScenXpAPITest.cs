using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DiScenFw;

public class DiScenXpAPITest : MonoBehaviour
{

    private void TestAPI()
    {
        DiScenXpApi.SetCurrentGoal("LED Circuit Test");

        const string config =
"PowerSupplyDC Battery 6000 50\n" +
"LED LED1 Red\n" +
"Resistor R1 2200 500\n" +
"Resistor R2 50 250\n" +
"Switch SW1 12000 40\n";
        DiScenXpApi.SetConfiguration(config);

        DiScenXpApi.AddSuccessCondition("LED1", "lit up", "true");
        DiScenXpApi.AddSuccessCondition("SW1", "connections", "2");
        DiScenXpApi.AddSuccessCondition("SW1", "position", "1");

        DiScenXpApi.NewEpisode();
        //int i = 0;
        //string entityId = DiScenXpApi.GetChangedEntity(0);
        //while (!string.IsNullOrEmpty(entityId))
        //{
        //    Debug.Log(entityId);
        //    i++;
        //    entityId = DiScenXpApi.GetChangedEntity(i);
        //}

        string[] entities = DiScenXpApi.GetChangedEntities();
        foreach (string entity in entities)
        {
            Debug.Log(entity);
        }

        {
            string entityId = entities[0];
            PropertyData[] entityProps = DiScenXpApi.GetEntityProperties(entityId);
            Debug.Log(entityId + " props: " + entityProps.Length);
            foreach (PropertyData prop in entityProps)
            {
                string msg = prop.PropertyId + "=" + prop.PropertyValue;
                Debug.Log(msg);
            }
        }

        Debug.Log("Battery connected=" + DiScenXpApi.CheckEntityProperty("Battery", "connected", "true"));
        Debug.Log(DiScenXpApi.TakeAction("connect", new string[] { "Battery", "+", "Battery", "-" }, true));
        Debug.Log("Battery connected=" + DiScenXpApi.CheckEntityProperty("Battery", "connected", "true"));
        Debug.Log("Battery burnt out=" + DiScenXpApi.GetEntityProperty("Battery", "burnt out"));
        Debug.Log(DiScenXpApi.LastResult());

        {
            string entityId = entities[0];
            RelationshipData[] entityRels = DiScenXpApi.GetEntityRelationships(entityId);
            Debug.Log(entityId + " rel: " + entityRels.Length);
            foreach (RelationshipData rel in entityRels)
            {
                string msg = entityId + "/" + rel.RelationshipId + " : " + rel.RelatedEntityId + "/" + rel.RelatedEndPoint;
                Debug.Log(msg);
            }
        }
        DiScenXpApi.NewEpisode();
        ActionData[] forbiddenActions = DiScenXpApi.GetForbiddenActions();
        foreach (ActionData forbidden in forbiddenActions)
        {
            string msg = "Do not " + forbidden.ActionId + " ";
            for (int i = 0; i < forbidden.Params.Length; i++) msg += forbidden.Params[i] + " ";
            Debug.Log(msg);
        }
        ActionData actionData = new ActionData();
        actionData.ActionId = "connect";
        actionData.Params = new string[4];
        actionData.Params[0] = "Battery";
        actionData.Params[1] = "+";
        actionData.Params[2] = "Battery";
        actionData.Params[3] = "-";
        Debug.Log(DiScenXpApi.TakeAction(actionData, false));
        //ActionData available = DiScenXpApi.GetAvailableAction(0);
        //string msg = available.ActionId + " ";
        //for (int i = 0; i < available.Params.Length; i++) msg += available.Params[i] + " ";
        //Debug.Log(msg);
        ActionData[] availableActions = DiScenXpApi.GetAvailableActions();
        foreach (ActionData available in availableActions)
        {
            string msg = available.ActionId + " ";
            for (int i = 0; i < available.Params.Length; i++) msg += available.Params[i] + " ";
            Debug.Log(msg);
        }
    }


    private void Start()
    {
        TestAPI();
    }

    private void Update()
    {

    }
}
