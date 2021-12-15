using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPVUDK;
using UnityDigitalScenario;
using DiScenFw;


namespace ElectronicCircuit
{
    public class CircuitComponent : ScenarioElement
    {
        public SwitchObject burntOutIcon;

        public CircuitComponentLead Lead(string leadName)
        {
            if (leadList.ContainsKey(leadName))
            {
                return leadList[leadName];
            }
            return null;
        }

        public CircuitComponentLead[] Leads
        {
            get
            {
                UpdateLeadsList();
                return leadsArray;
            }
        }

        public void SetBurntOut(bool burntOut)
        {
            if (burntOutIcon != null)
            {
                burntOutIcon.Switch(burntOut ? "BurntOut" : "Empty");
            }
        }

        // TODO: shown in Inspector for debugging, to be hidden in future
        [SerializeField]
        private CircuitComponentLead[] leadsArray;

        private Dictionary<string, CircuitComponentLead> leadList = new Dictionary<string, CircuitComponentLead>();

#if UNITY_EDITOR
        private bool wasAwakened = false;
#endif


#if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            wasAwakened = true;
        }
#endif


        private void UpdateLeadsList()
        {
            if (leadList.Count == 0)
            {
                CircuitComponentLead[] leads = GetComponentsInChildren<CircuitComponentLead>();
                if (leads != null)
                {
                    leadsArray = leads;
                    foreach (CircuitComponentLead lead in leads)
                    {
                        if (lead.leadName != null && lead != null)
                        {
                            leadList.Add(lead.leadName, lead);
                        }
                    }
                }
            }
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // fill data if empty, based on component properties
            if (string.IsNullOrEmpty(data.Type))
            {
                string componentType = data.Identifier.TrimEnd("0123456789".ToCharArray());
                switch (componentType)
                {
                    case "SW":
                        componentType = "Switch";
                        break;
                    case "LED":
                        componentType = "LED";
                        break;
                    case "R":
                        componentType = "Resistor";
                        break;
                    case "Battery":
                        componentType = "PowerSupplyDC";
                        break;
                }
                data.Type = componentType;
            }
            data.ClassName = "Element";
            if (string.IsNullOrEmpty(data.Asset.AssetType))
            {
                data.Category = "CircuitComponent";
            }
            if (wasAwakened && !Application.isPlaying && DiScenXpApi.IsCyberSystemLoaded() && !string.IsNullOrEmpty(data.Configuration))
            {
                data.Configuration = DiScenXpApi.GetEntityConfiguration(data.Identifier);
            }
            if (burntOutIcon == null)
            {
                Transform iconTransform = transform.Find("IconBurntOut");
                if (iconTransform != null)
                {
                    burntOutIcon = iconTransform.GetComponent<SwitchObject>();
                }
            }
            UpdateLeadsList();
        }
#endif

    }
}
