using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectronicCircuit
{
    public class PowerSupplyComponent : CircuitComponent
    {
        public int milliVolt = 6000;
        public int milliAmpere = 50;

        protected override void Awake()
        {
            base.Awake();
            data.Type = "PowerSupplyDC";
        }
    }
}
