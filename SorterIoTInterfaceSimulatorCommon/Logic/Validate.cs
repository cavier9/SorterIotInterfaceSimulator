using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using Glory.SorterInterface.Message;

namespace SorterIotInterfaceSimulator.Common.Logic
{
    /// <summary>
    /// validate message
    /// </summary>
    public class Validate : IDisposable
    {
        #region parameter

        /// <summary>
        /// schema file name dictionary
        /// </summary>
        private IDictionary<MessageType, Dictionary<MessageName, string>> schemaFileNameDict;

        #endregion

        #region constractor

        /// <summary>
        /// constractor
        /// </summary>
        /// <param name="schemaFolderPath"></param>
        public Validate()
        {
            this.schemaFileNameDict = this.GetSchemaFileNameDictionary();
        }

        #endregion

        #region IDisposable interface implement

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            schemaFileNameDict.Clear();
        }

        #endregion

        #region GetSchemaFileNameDictionary

        /// <summary>
        /// GetSchemaFileNameDictionary
        /// </summary>
        private Dictionary<MessageType, Dictionary<MessageName, string>> GetSchemaFileNameDictionary()
        {
            Dictionary<MessageType, Dictionary<MessageName, string>> dict =
                new Dictionary<MessageType, Dictionary<MessageName, string>>();

            // command
            dict.Add(MessageType.Command, new Dictionary<MessageName, string>());
            dict[MessageType.Command].Add(MessageName.CreateSession, "schema_CreateSession_command.json");
            dict[MessageType.Command].Add(MessageName.CloseSession, "schema_CloseSession_command.json");
            dict[MessageType.Command].Add(MessageName.Reboot, "schema_Reboot_command.json");
            dict[MessageType.Command].Add(MessageName.SetExclusiveControl, "schema_SetExclusiveControl_command.json");
            dict[MessageType.Command].Add(MessageName.GetDeviceInformation, "schema_GetDeviceInformation_command.json");
            dict[MessageType.Command].Add(MessageName.GetDenominationInformation, "schema_GetDenominationInformation_command.json");
            dict[MessageType.Command].Add(MessageName.GetDeviceStatus, "schema_GetDeviceStatus_command.json");
            dict[MessageType.Command].Add(MessageName.StartTransaction, "schema_StartTransaction_command.json");
            dict[MessageType.Command].Add(MessageName.EndTransaction, "schema_EndTransaction_command.json");
            dict[MessageType.Command].Add(MessageName.CancelTransaction, "schema_CancelTransaction_command.json");
            dict[MessageType.Command].Add(MessageName.GetCountingResult, "schema_GetCountingResult_command.json");
            dict[MessageType.Command].Add(MessageName.RequestUploadLogFile, "schema_RequestUploadLogFile_command.json");
            dict[MessageType.Command].Add(MessageName.RequestUploadSettingFile, "schema_RequestUploadSettingFile_command.json");
            dict[MessageType.Command].Add(MessageName.RequestDownloadSettingFile, "schema_RequestDownloadSettingFile_command.json");
            dict[MessageType.Command].Add(MessageName.RequestDownloadUpgradeFile, "schema_RequestDownloadUpgradeFile_command.json");
            dict[MessageType.Command].Add(MessageName.StartCount, "schema_StartCount_command.json");
            dict[MessageType.Command].Add(MessageName.StopCount, "schema_StopCount_command.json");
            dict[MessageType.Command].Add(MessageName.SetBatchSize, "schema_SetBatchSize_command.json");
            dict[MessageType.Command].Add(MessageName.SetStackerPatternNumber, "schema_SetStackerPatternNumber_command.json");
            dict[MessageType.Command].Add(MessageName.GetStackerPatternNumber, "schema_GetStackerPatternNumber_command.json");
            dict[MessageType.Command].Add(MessageName.GetStackerPatternDetail, "schema_GetStackerPatternDetail_command.json");
            dict[MessageType.Command].Add(MessageName.GetTransactionList, "schema_GetTransactionList_command.json");
            dict[MessageType.Command].Add(MessageName.GetTransactionData, "schema_GetTransactionData_command.json");
            dict[MessageType.Command].Add(MessageName.SetDateTime, "schema_SetDateTime_command.json");
            dict[MessageType.Command].Add(MessageName.SetOperator, "schema_SetOperator_command.json");
            dict[MessageType.Command].Add(MessageName.GetOperator, "schema_GetOperator_command.json");
            dict[MessageType.Command].Add(MessageName.SetCurrency, "schema_SetCurrency_command.json");
            dict[MessageType.Command].Add(MessageName.GetCurrency, "schema_GetCurrency_command.json");
            dict[MessageType.Command].Add(MessageName.GetCurrencyInformation, "schema_GetCurrencyInformation_command.json");
            dict[MessageType.Command].Add(MessageName.ResetStacker, "schema_ResetStacker_command.json");
            // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↓
            dict[MessageType.Command].Add(MessageName.GetDateTime, "schema_GetDateTime_command.json");
            dict[MessageType.Command].Add(MessageName.GetBatchSize, "schema_GetBatchSize_command.json");
            // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↑
            dict[MessageType.Command].Add(MessageName.SetHotList, "schema_SetHotList_command.json");

            // response
            dict.Add(MessageType.Response, new Dictionary<MessageName, string>());
            dict[MessageType.Response].Add(MessageName.CreateSession, "schema_CreateSession_response.json");
            dict[MessageType.Response].Add(MessageName.CloseSession, "schema_CloseSession_response.json");
            dict[MessageType.Response].Add(MessageName.Reboot, "schema_Reboot_response.json");
            dict[MessageType.Response].Add(MessageName.SetExclusiveControl, "schema_SetExclusiveControl_response.json");
            dict[MessageType.Response].Add(MessageName.GetDeviceInformation, "schema_GetDeviceInformation_response.json");
            dict[MessageType.Response].Add(MessageName.GetDenominationInformation, "schema_GetDenominationInformation_response.json");
            dict[MessageType.Response].Add(MessageName.GetDeviceStatus, "schema_GetDeviceStatus_response.json");
            dict[MessageType.Response].Add(MessageName.StartTransaction, "schema_StartTransaction_response.json");
            dict[MessageType.Response].Add(MessageName.EndTransaction, "schema_EndTransaction_response.json");
            dict[MessageType.Response].Add(MessageName.CancelTransaction, "schema_CancelTransaction_response.json");
            dict[MessageType.Response].Add(MessageName.GetCountingResult, "schema_GetCountingResult_response.json");
            dict[MessageType.Response].Add(MessageName.RequestUploadLogFile, "schema_RequestUploadLogFile_response.json");
            dict[MessageType.Response].Add(MessageName.RequestUploadSettingFile, "schema_RequestUploadSettingFile_response.json");
            dict[MessageType.Response].Add(MessageName.RequestDownloadSettingFile, "schema_RequestDownloadSettingFile_response.json");
            dict[MessageType.Response].Add(MessageName.RequestDownloadUpgradeFile, "schema_RequestDownloadUpgradeFile_response.json");
            dict[MessageType.Response].Add(MessageName.StartCount, "schema_StartCount_response.json");
            dict[MessageType.Response].Add(MessageName.StopCount, "schema_StopCount_response.json");
            dict[MessageType.Response].Add(MessageName.SetBatchSize, "schema_SetBatchSize_response.json");
            dict[MessageType.Response].Add(MessageName.SetStackerPatternNumber, "schema_SetStackerPatternNumber_response.json");
            dict[MessageType.Response].Add(MessageName.GetStackerPatternNumber, "schema_GetStackerPatternNumber_response.json");
            dict[MessageType.Response].Add(MessageName.GetStackerPatternDetail, "schema_GetStackerPatternDetail_response.json");
            dict[MessageType.Response].Add(MessageName.GetTransactionList, "schema_GetTransactionList_response.json");
            dict[MessageType.Response].Add(MessageName.GetTransactionData, "schema_GetTransactionData_response.json");
            dict[MessageType.Response].Add(MessageName.SetDateTime, "schema_SetDateTime_response.json");
            dict[MessageType.Response].Add(MessageName.SetOperator, "schema_SetOperator_response.json");
            dict[MessageType.Response].Add(MessageName.GetOperator, "schema_GetOperator_response.json");
            dict[MessageType.Response].Add(MessageName.SetCurrency, "schema_SetCurrency_response.json");
            dict[MessageType.Response].Add(MessageName.GetCurrency, "schema_GetCurrency_response.json");
            dict[MessageType.Response].Add(MessageName.GetCurrencyInformation, "schema_GetCurrencyInformation_response.json");
            dict[MessageType.Response].Add(MessageName.ResetStacker, "schema_ResetStacker_response.json");
            // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↓
            dict[MessageType.Response].Add(MessageName.GetDateTime, "schema_GetDateTime_response.json");
            dict[MessageType.Response].Add(MessageName.GetBatchSize, "schema_GetBatchSize_response.json");
            // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↑
            dict[MessageType.Response].Add(MessageName.SetHotList, "schema_SetHotList_response.json");

            // event
            dict.Add(MessageType.Event, new Dictionary<MessageName, string>());
            dict[MessageType.Event].Add(MessageName.Connected, "schema_Connected_event.json");
            dict[MessageType.Event].Add(MessageName.Disconnected, "schema_Disconnected_event.json");
            dict[MessageType.Event].Add(MessageName.SessionClosed, "schema_SessionClosed_event.json");
            dict[MessageType.Event].Add(MessageName.DeviceStatusUpdated, "schema_DeviceStatusUpdated_event.json");
            dict[MessageType.Event].Add(MessageName.CountingResultUpdated, "schema_CountingResultUpdated_event.json");
            dict[MessageType.Event].Add(MessageName.ButtonTouched, "schema_ButtonTouched_event.json");
            dict[MessageType.Event].Add(MessageName.TransactionDetected, "schema_TransactionDetected_event.json");
            dict[MessageType.Event].Add(MessageName.OperatorChanged, "schema_OperatorChanged_event.json");
            dict[MessageType.Event].Add(MessageName.CurrencyChanged, "schema_CurrencyChanged_event.json");
            dict[MessageType.Event].Add(MessageName.StackerPatternChanged, "schema_StackerPatternChanged_event.json");
            dict[MessageType.Event].Add(MessageName.NoteTransported, "schema_NoteTransported_event.json");
            dict[MessageType.Event].Add(MessageName.CounterfeitDetected, "schema_CounterfeitDetected_event.json");
            dict[MessageType.Event].Add(MessageName.OperationOccured, "schema_OperationOccured_event.json");
            dict[MessageType.Event].Add(MessageName.ScreenChanged, "schema_ScreenChanged_event.json");

            return dict;
        }

        #endregion

        #region GetSchemaFileName

        /// <summary>
        /// GetSchemaFileFullPath
        /// </summary>
        /// <param name="type">message type</param>
        /// <param name="name">message name</param>
        /// <returns>schema file name(null: not found)</returns>
        private string GetSchemaFileName(MessageType type, MessageName name)
        {
            // get schema file path
            if (this.schemaFileNameDict.ContainsKey(type) == false)
            {
                return null;
            }
            if (this.schemaFileNameDict[type].ContainsKey(name) == false)
            {
                return null;
            }
            string schemaFileName = this.schemaFileNameDict[type][name];

            return schemaFileName;
        }

        #endregion

        #region ValidateMessage

        /// <summary>
        /// validate message
        /// </summary>
        /// <param name="jsonMessage"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ValidateMessage(string jsonMessage, MessageType type, MessageName name)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("JSON schema validate : ");

            if (this.ValidateCommon(jsonMessage) == true)
            {
                sb.Append("common=OK");
            }
            else
            {
                sb.Append("common=NG");
            }

            sb.Append(", ");

            string schemaFileName = GetSchemaFileName(type, name);
            if( schemaFileName == null)
            {
                sb.Append("detail schema is nothing.");
            }
            else
            {
                if (this.ValidateDetail(jsonMessage, schemaFileName) == true)
                {
                    sb.Append("detail=OK.");
                }
                else
                {
                    sb.Append("detail=NG.");
                }
            }

            return sb.ToString();
        }

        #endregion

        #region ValidateCommon

        /// <summary>
        /// validate message common
        /// </summary>
        /// <param name="jsonMessage"></param>
        /// <returns></returns>
        public bool ValidateCommon(string jsonMessage)
        {
            // validate JSON
            bool retCommon = false;
            {
                using (StreamReader file = File.OpenText(@".\\Schema\\schema_common_message.json"))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JSchema schema = JSchema.Load(reader);

                    JObject user = JObject.Parse(jsonMessage);
                    if (user != null)
                    {
                        retCommon = user.IsValid(schema);
                    }
                }
            }

            return retCommon;
        }

        #endregion

        #region ValidateDetail

        /// <summary>
        /// validate message detail
        /// </summary>
        /// <param name="jsonMessage"></param>
        /// <param name="schemaFileName"></param>
        /// <returns></returns>
        public bool ValidateDetail(string jsonMessage, string schemaFileName)
        {
            // validate JSON
            bool retDetail = false;
            {
                using (StreamReader file = File.OpenText(@".\\Schema\\" + schemaFileName))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JSchema schema = JSchema.Load(reader);
                    JSchemaType schemaType = schema.Type.Value;

                    JObject user = JObject.Parse(jsonMessage);
                    if (user != null)
                    {
                        JToken userDetail = user.GetValue("Detail");
                        if (userDetail == null)
                        {
                            bool isAbleNullType = ((int)(schemaType) & (int)(JSchemaType.Null)) != 0;
                            if (isAbleNullType == true)
                            {
                                retDetail = true;
                            }
                            else
                            {
                                retDetail = false;
                            }
                        }
                        else
                        {
                            retDetail = userDetail.IsValid(schema);
                        }
                    }
                }
            }

            return retDetail;
        }

        #endregion
    }
}
