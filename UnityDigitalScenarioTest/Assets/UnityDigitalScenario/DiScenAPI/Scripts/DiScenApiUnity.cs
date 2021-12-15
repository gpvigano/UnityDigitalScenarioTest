using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using GPVUDK;
using DiScenFw;

namespace UnityDigitalScenario
{
    public delegate void ScreenDisplayMessageAction(
        LogLevel severity, string message, string msgTag);

    /// <summary>
    /// Unity adaptor for DiScenFwNET-DiScenApi (see DiScenFwNET documentation).
    /// </summary>
    public class DiScenApiUnity : DiScenApi
    {
        /// <summary>
        /// Global scaling for units, used in conversions.
        /// </summary>
        public static float unitScale = 1f;


        /// <summary>
        /// Event triggered on message display.
        /// </summary>
        public static event ScreenDisplayMessageAction ScreenDisplayMessageEvent;


        /// <summary>
        /// Convert a DiScenFwNET vector to Unity vector.
        /// </summary>
        /// <param name="vec">DiScenFwNET vector</param>
        public static Vector3 ConvertVec(Vector3D vec)
        {
            return new Vector3(vec.Right, vec.Up, vec.Forward);
        }


        /// <summary>
        /// Convert a Unity vector to DiScenFwNET vector.
        /// </summary>
        /// <param name="vec">Unity vector</param>
        public static Vector3D ConvertVector3(Vector3 vec)
        {
            Vector3D vec3d = new Vector3D();
            vec3d.Right = vec.x;
            vec3d.Forward = vec.z;
            vec3d.Up = vec.y;
            return vec3d;
        }


        /// <summary>
        /// Initialize DiScenFwNET and bind to the message displaying event.
        /// This must be called before any other method.
        /// </summary>
        public static new void Initialize()
        {
            if (!DiScenApi.Initialized)
            {
                DiScenApi.Initialize();
                DiScenApi.DisplayMessageEvent += DisplayMessageCallback;
            }
        }


        private static void DisplayMessageCallback(
            LogLevel severity, string message, string category,
            bool onConsole, bool onScreen, string msgTag)
        {
            string msg = "[" + category + "] " + message;
            if (onConsole)
            {
                switch (severity)
                {
                    case LogLevel.Debug:
                    case LogLevel.Verbose:
                    case LogLevel.Log:
                        Debug.Log(msg);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(msg);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Fatal:
                        Debug.LogError(msg);
                        break;
                }
            }
            if (onScreen)
            {
                if (ScreenDisplayMessageEvent != null)
                {
                    ScreenDisplayMessageEvent(severity, msg, msgTag);
                }
            }
        }

    }
}
