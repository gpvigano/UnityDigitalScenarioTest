using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectronicCircuit
{
    public class SwitchComponent : CircuitComponent
    {

        public int milliVoltMax = 250000;
        public int milliAmpereMax = 5000;

        [SerializeField]
        private Transform switchCursorTransform;
        [SerializeField]
        private Vector3 offPosition = Vector3.zero;
        [SerializeField]
        private Vector3 onPosition = Vector3.zero;

        public GameObject SwitchCursor
        {
            get { return switchCursorTransform ? switchCursorTransform.gameObject : null; }
        }
        public void Switch(bool on)
        {
            switchCursorTransform.localPosition = on ? onPosition : offPosition;
        }

        protected override void Awake()
        {
            base.Awake();
            data.Type = "Switch";
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            data.Type = "Switch";
            if (switchCursorTransform == null)
            {
                switchCursorTransform = transform.Find("SWITCH/SwitchCursor");
                if (switchCursorTransform != null)
                {
                    offPosition = switchCursorTransform.localPosition;
                    onPosition = offPosition + new Vector3(0, 0, 0.36f);
                }
            }
        }
#endif
    }
}
