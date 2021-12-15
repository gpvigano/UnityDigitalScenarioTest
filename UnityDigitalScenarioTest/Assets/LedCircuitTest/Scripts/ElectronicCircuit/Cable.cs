using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyCurvedLine;

namespace ElectronicCircuit
{
    /// <summary>
    /// Cable connectiong two points, automatically updated to follow connected positions.
    /// </summary>
    public class Cable : MonoBehaviour
    {
        /// <summary>
        /// Start anchor transform. Middle points are automatically calculated from start to end anchors.
        /// </summary>
        [Tooltip("Start anchor transform. Middle points are automatically calculated from start to end anchors.")]
        public Transform StartAnchor;

        /// <summary>
        /// End anchor transform. Middle points are automatically calculated from start to end anchors.
        /// </summary>
        [Tooltip("End anchor transform. Middle points are automatically calculated from start to end anchors.")]
        public Transform EndAnchor;

        private Vector3 StartAnchorOldPos;
        private Vector3 EndAnchorOldPos;
        private Transform OldStartAnchor;
        private Transform OldEndAnchor;

        private CurvedLineRenderer curvedLineRenderer;


        private void Awake()
        {
            curvedLineRenderer = GetComponent<CurvedLineRenderer>();
        }


        private void Start()
        {
            transform.rotation = Quaternion.identity;
            Build();
        }


        public void Build()
        {
            if (curvedLineRenderer)
            {
                curvedLineRenderer.UpdateLineRenderer();
                if (curvedLineRenderer.LinePoints.Length == 4 && StartAnchor != null && EndAnchor != null)
                {
                    transform.position = StartAnchor.position;
                    Vector3 middle = (EndAnchor.position + StartAnchor.position) / 2f;
                    Vector3 pos1 = StartAnchor.position;
                    pos1.y = middle.y;
                    Vector3 pos2 = EndAnchor.position;
                    pos2.y = middle.y;
                    curvedLineRenderer.LinePoints[0].transform.position = StartAnchor.position;
                    curvedLineRenderer.LinePoints[1].transform.position = pos1;
                    curvedLineRenderer.LinePoints[2].transform.position = pos2;
                    curvedLineRenderer.LinePoints[3].transform.position = EndAnchor.position;
                    curvedLineRenderer.UpdateLineRenderer();
                    //curvedLineRenderer.autoUpdate = false;
                }
            }
        }


        private void Update()
        {
            if (StartAnchor != null && EndAnchor != null)
            {
                if (StartAnchorOldPos != StartAnchor.position || EndAnchorOldPos != EndAnchor.position
                    || OldStartAnchor != StartAnchor || OldEndAnchor != EndAnchor)
                {
                    Build();
                    StartAnchorOldPos = StartAnchor.position;
                    EndAnchorOldPos = EndAnchor.position;
                    OldStartAnchor = StartAnchor;
                    OldEndAnchor = EndAnchor;
                }
            }
        }
    }
}
