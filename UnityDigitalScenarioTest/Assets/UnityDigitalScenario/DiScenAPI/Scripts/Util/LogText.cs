using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiScenFw;

namespace UnityDigitalScenario
{
    /// <summary>
    /// Manages a list of text messages for OSD, like in Unreal Engine.
    /// </summary>
    public class LogText : MonoBehaviour
    {
        protected class LogTextMessage
        {
            public string message;
            public string colorCode;
            public string alphaCode;
            public string startTag;
            public string endTag;
            public string msgTag;
            public bool enableTimeout = false;
            public float timeOut = 0;
            public float timeLeft = 0;
        }

        [Tooltip("Text for displaying messages (rich text must be enabled).")]
        [SerializeField]
        protected Text logOnScreenText = null;

        [Tooltip("Maximum number of messages that can be displayed.")]
        [SerializeField]
        protected int maxMessages = 10;

        protected List<LogTextMessage> messages = new List<LogTextMessage>();
        protected Dictionary<string, LogTextMessage> taggedMessages = new Dictionary<string, LogTextMessage>();
        protected bool updateNeeded = false;
        protected Dictionary<string, string> colorMap = new Dictionary<string, string>();
        protected const int alphaIndex = 14;// AA position in <color=#RRGGBBAA>

        /// <summary>
        /// Add a new message.
        /// </summary>
        /// <param name="severity">Severity (see LogLevel in DiScenFwNET documentation).</param>
        /// <param name="message">Message content.</param>
        /// <param name="msgTag">Tag used to update an existing message.</param>
        public void AddMessage(LogLevel severity, string message, string msgTag)
        {
            /* HTML sample:
<color=white>Debug</color>
<color=cyan>Verborse</color>
<color=lime>Log</color>
<color=yellow>Warning</color>
<color=red><b>Error</b></color>
<color=red><b><i>Fatal</i></b></color>
            */
            LogTextMessage newMsg = new LogTextMessage();
            float timeOut = 0;
            string colorCode = "white";
            bool bold = false;
            bool italic = false;
            bool enableTimeout = true;
            switch (severity)
            {
                // TODO: color customization?
                case LogLevel.Debug:
                    colorCode = colorMap["white"];
                    timeOut = 5f;
                    break;
                case LogLevel.Verbose:
                    colorCode = colorMap["cyan"];
                    timeOut = 10f;
                    break;
                case LogLevel.Log:
                    colorCode = colorMap["lime"];
                    timeOut = 15f;
                    break;
                case LogLevel.Warning:
                    colorCode = colorMap["yellow"];
                    timeOut = 20f;
                    break;
                case LogLevel.Error:
                    colorCode = colorMap["red"];
                    bold = true;
                    enableTimeout = false;
                    break;
                case LogLevel.Fatal:
                    colorCode = colorMap["red"];
                    bold = true;
                    italic = true;
                    enableTimeout = false;
                    break;
            }
            string startTag = "";
            if (bold) startTag += "<b>";
            if (italic) startTag += "<i>";
            string endTag = "";
            if (italic) endTag += "</i>";
            if (bold) endTag += "</b>";
            newMsg.message = message;
            newMsg.colorCode = colorCode;
            newMsg.alphaCode = "ff";
            newMsg.startTag = startTag;
            newMsg.endTag = endTag;
            newMsg.timeOut = timeOut;
            newMsg.timeLeft = timeOut;
            newMsg.enableTimeout = enableTimeout;
            newMsg.msgTag = msgTag;
            if (!string.IsNullOrEmpty(msgTag))
            {
                if (taggedMessages.ContainsKey(msgTag))
                {
                    messages.RemoveAll(m => m.msgTag == msgTag);
                    taggedMessages[msgTag] = newMsg;
                    messages.Add(newMsg);
                }
                else
                {
                    taggedMessages.Add(msgTag, newMsg);
                    messages.Add(newMsg);
                }
            }
            else
            {
                messages.Add(newMsg);
            }
            updateNeeded = true;
        }


        /// <summary>
        /// Clean up the messages in the text box.
        /// </summary>
        public void CleanUpText()
        {
            // cleanup elapsed messages
            float dt = Time.deltaTime;
            for (int i = 0; i < messages.Count; i++)
            {
                LogTextMessage logMsg = messages[i];
                if (logMsg.enableTimeout)
                {
                    updateNeeded = true;
                    float alpha = messages[i].timeLeft / messages[i].timeOut;
                    messages[i].timeLeft -= dt;
                    messages[i].alphaCode = ((int)(255f * alpha)).ToString("X2");
                    if (messages[i].timeLeft < 0)
                    {
                        if (!string.IsNullOrEmpty(logMsg.msgTag))
                        {
                            taggedMessages.Remove(logMsg.msgTag);
                        }
                        messages.RemoveAt(i);
                        i--;
                    }
                }
            }
        }


        /// <summary>
        /// Update the text box according to stored messages.
        /// </summary>
        public void UpdateText()
        {
            string message = "";
            int startIndex = messages.Count - maxMessages;
            if (startIndex < 0) startIndex = 0;
            for (int i = startIndex; i < messages.Count; i++)
            {
                string msg = "<color=#" + messages[i].colorCode + messages[i].alphaCode + ">"
                    + messages[i].startTag + messages[i].message + messages[i].endTag + "</color>";
                message += msg;
            }
            logOnScreenText.text = message;
            updateNeeded = false;
        }


        protected virtual void Start()
        {
            logOnScreenText.text = "";
            logOnScreenText.supportRichText = true;
        }


        protected virtual void Awake()
        {
            colorMap.Add("white", "ffffff");
            colorMap.Add("cyan", "00ffff");
            colorMap.Add("lime", "00ff00");
            colorMap.Add("yellow", "ffff00");
            colorMap.Add("red", "ff0000");
        }


        protected virtual void Update()
        {
            CleanUpText();
            if (updateNeeded)
            {
                UpdateText();
            }
        }


#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (logOnScreenText == null)
            {
                logOnScreenText = GetComponent<Text>();
            }
        }
#endif
    }
}