using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPVUDK;

namespace ElectronicCircuit
{
    public class CircuitComponentLead : MonoBehaviour
    {
        public string leadName;
        public SwitchObject hintIcon;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(leadName))
            {
                leadName = gameObject.name;
            }
            if (hintIcon == null)
            {
                Transform iconTransform = transform.Find("Icon");
                if (iconTransform != null)
                {
                    hintIcon = iconTransform.GetComponent<SwitchObject>();
                }
            }
        }
#endif

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
