using System;
using Glory.SorterInterface.SorterInterfaceClient.Client.Session;
using Glory.SorterInterface.Message;

namespace Glory.SorterInterface.Protocol.Mqtt
{
    public class Topic
    {
        #region fixed parameter

        /// <summary>
        /// topic header. (/glory/sorter)
        /// </summary>
        private const string header = "/glory/sorter";

        /// <summary>
        /// event topic out connection. (/glory/sorter/event/{event name})
        /// </summary>
        private const string eventTopicOutConnection = header + "/event/{0}";

        /// <summary>
        /// event topic in connection. (/glory/sorter/{deviceName}/event/{event name})
        /// </summary>
        private const string eventTopicInConnection = header + "/{0}/event/{1}";

        /// <summary>
        /// command topic out session. (/glory/sorter/{deviceName}/command)
        /// </summary>
        private const string commandTopicOutSession = header + "/{0}/command";

        /// <summary>
        /// command topic in session. (/glory/sorter/{deviceName}/{sessionID}/command)
        /// </summary>
        private const string commandTopicInSession = header + "/{0}/{1:000}/command";

        /// <summary>
        /// response topic out session. (/glory/sorter/{deviceName}/response)
        /// </summary>
        private const string responseTopicOutSession = header + "/{0}/response";

        /// <summary>
        /// response topic in session. (/glory/sorter/{deviceName}/{sessionID}/response)
        /// </summary>
        private const string responseTopicInSession = header + "/{0}/{1:000}/response";

        #endregion

        #region getter

        /// <summary>
        /// event topic out connection. (/glory/sorter/event/{event name})
        /// </summary>
        /// <param name="eventName">eventName</param>
        /// <returns>event topic</returns>
        static public string GetEventTopic(string eventName)
        {
            return String.Format(Topic.eventTopicOutConnection, eventName);
        }

        /// <summary>
        /// event topic out connection. (/glory/sorter/event/{event name})
        /// </summary>
        /// <param name="eventName">eventName</param>
        /// <returns>event topic</returns>
        static public string GetEventTopic(MessageName eventName)
        {
            return String.Format(Topic.eventTopicOutConnection, eventName);
        }

        /// <summary>
        /// event topic in connection. (/glory/sorter/{deviceName}/event/{event name})
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <param name="eventName">eventName</param>
        /// <returns>topic</returns>
        static public string GetEventTopic(string deviceName, string eventName)
        {
            return String.Format(Topic.eventTopicInConnection, deviceName, eventName);
        }

        /// <summary>
        /// event topic in connection. (/glory/sorter/{deviceName}/event/{event name})
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <param name="eventName">eventName</param>
        /// <returns>topic</returns>
        static public string GetEventTopic(string deviceName, MessageName eventName)
        {
            return String.Format(Topic.eventTopicInConnection, deviceName, eventName);
        }

        /// <summary>
        /// command topic out session. (/glory/sorter/{deviceName}/command)
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <returns>topic</returns>
        static public string GetCommandTopic(string deviceName)
        {
            return String.Format(Topic.commandTopicOutSession, deviceName);
        }

        /// <summary>
        /// command topic in session. (/glory/sorter/{deviceName}/{sessionID}/command)
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <param name="sessionId">sessionId</param>
        /// <returns>topic</returns>
        static public string GetCommandTopic(string deviceName, int sessionId)
        {
            return String.Format(Topic.commandTopicInSession, deviceName, sessionId);
        }

        /// <summary>
        /// response topic out session. (/glory/sorter/{deviceName}/response)
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <returns>topic</returns>
        static public string GetResponseTopic(string deviceName)
        {
            return String.Format(Topic.responseTopicOutSession, deviceName);
        }

        /// <summary>
        /// response topic in session. (/glory/sorter/{deviceName}/{sessionID}/response)
        /// </summary>
        /// <param name="deviceName">deviceName</param>
        /// <param name="sessionId">sessionId</param>
        /// <returns>topic</returns>
        static public string GetResponseTopic(string deviceName, int sessionId)
        {
            return String.Format(Topic.responseTopicInSession, deviceName, sessionId);
        }

        #endregion

        #region GetDeviceName / GetSessionId

        /// <summary>
        /// get device name
        /// </summary>
        /// <param name="topic">topic</param>
        /// <returns>device name (or empty when out connection)</returns>
        static public string GetDeviceName(string topic)
        {
            try
            {
                const int INDEX = 3;

                if (topic == null)
                {
                    return string.Empty;
                }

                char[] delimiterChars = { '/' };
                string[] splitTopic = topic.Split(delimiterChars);

                if (splitTopic.Length < (INDEX + 1))
                {
                    return string.Empty;
                }

                // Topic の3節目が event でなければ、3節目からデバイス名を取得する
                string deviceName = splitTopic[INDEX];
                if (deviceName == "event")
                {
                    return string.Empty;
                }
                return deviceName;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// get session ID
        /// </summary>
        /// <param name="topic">topic</param>
        /// <returns>session Id (or empty when out session)</returns>
        static public int GetSessionId(string topic)
        {
            try
            {
                if (topic == null)
                {
                    return SessionManager.SESSION_ID_UNDEF;
                }

                char[] delimiterChars = { '/' };
                string[] splitTopic = topic.Split(delimiterChars);

                if ((splitTopic.Length == 6) &&
                    ((splitTopic[5] == "command") || (splitTopic[5] == "response")))
                {
                    // Topic が6節から成っており、6節目が command or response ならば、5節目からセッションIDを取得する
                    int sessionId = Int32.Parse(splitTopic[4]);
                    return sessionId;
                }
                else
                {
                    return SessionManager.SESSION_ID_UNDEF;
                }
            }
            catch
            {
                return SessionManager.SESSION_ID_UNDEF;
            }
        }
        #endregion
    }
}
