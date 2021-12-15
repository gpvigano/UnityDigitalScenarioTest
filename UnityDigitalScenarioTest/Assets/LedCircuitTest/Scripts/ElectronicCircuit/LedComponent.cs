using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectronicCircuit
{
    public class LedComponent : CircuitComponent
    {

        public string ledModel = "Red";

        [SerializeField]
        private Light ledLight;

        public void LightUp(bool litUp)
        {
            ledLight.enabled = litUp;
        }

        protected override void Awake()
        {
            base.Awake();
            data.Type = "LED";
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            data.Type = "LED";
            ledLight = GetComponentInChildren<Light>();
        }
#endif
    }
}
