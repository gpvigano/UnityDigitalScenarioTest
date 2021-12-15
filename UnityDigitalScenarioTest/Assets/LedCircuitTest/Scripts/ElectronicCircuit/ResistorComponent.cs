using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElectronicCircuit
{
    public class ResistorComponent : CircuitComponent
    {

        public int ohm = 1000;
        public int milliWattMax = 1000;

        protected override void Awake()
        {
            base.Awake();
            data.Type = "Resistor";
        }
    }
}
