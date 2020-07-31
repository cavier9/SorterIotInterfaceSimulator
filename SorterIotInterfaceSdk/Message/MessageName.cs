using System;

namespace Glory.SorterInterface.Message
{
    /// <summary>
    /// message name
    /// </summary>
    public enum MessageName
    {
        /// <summary>
        /// Undeffine message name
        /// </summary>
        Undef,

        // Command/Response

        /// <summary>
        /// command/response CreateSession
        /// </summary>
        CreateSession,

        /// <summary>
        /// command/response CloseSession
        /// </summary>
        CloseSession,

        /// <summary>
        /// command/response Reboot
        /// </summary>
        Reboot,

        /// <summary>
        /// command/response SetExclusiveControl
        /// </summary>
        SetExclusiveControl,

        /// <summary>
        /// command/response GetDeviceInformation
        /// </summary>
        GetDeviceInformation,

        /// <summary>
        /// command/response GetDenominationInformation
        /// </summary>
        GetDenominationInformation,

        /// <summary>
        /// command/response GetDeviceStatus
        /// </summary>
        GetDeviceStatus,

        /// <summary>
        /// command/response StartTransaction
        /// </summary>
        StartTransaction,

        /// <summary>
        /// command/response EndTransaction
        /// </summary>
        EndTransaction,

        /// <summary>
        /// command/response CancelTransaction
        /// </summary>
        CancelTransaction,

        /// <summary>
        /// command/response GetCountingResult
        /// </summary>
        GetCountingResult,

        /// <summary>
        /// command/response RequestUploadLogFile
        /// </summary>
        RequestUploadLogFile,

        /// <summary>
        /// command/response RequestUploadSettingFile
        /// </summary>
        RequestUploadSettingFile,

        /// <summary>
        /// command/response RequestDownloadSettingFile
        /// </summary>
        RequestDownloadSettingFile,

        /// <summary>
        /// command/response RequestDownloadUpgradeFile
        /// </summary>
        RequestDownloadUpgradeFile,

        /// <summary>
        /// command/response StartCount
        /// </summary>
        StartCount,

        /// <summary>
        /// command/response StopCount
        /// </summary>
        StopCount,

        /// <summary>
        /// command/response SetBatchSize
        /// </summary>
        SetBatchSize,

        // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↓
        /// <summary>
        /// command/response GetBatchSize
        /// </summary>
        GetBatchSize,
        // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↑

        /// <summary>
        /// command/response SetStackerPatternNumber
        /// </summary>
        SetStackerPatternNumber,

        /// <summary>
        /// command/response GetStackerPatternNumber
        /// </summary>
        GetStackerPatternNumber,

        /// <summary>
        /// command/response GetStackerPatternDetail
        /// </summary>
        GetStackerPatternDetail,

        /// <summary>
        /// command/response GetTransactionList
        /// </summary>
        GetTransactionList,

        /// <summary>
        /// command/response GetTransactionData
        /// </summary>
        GetTransactionData,

        /// <summary>
        /// command/response SetDateTime
        /// </summary>
        SetDateTime,

        // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↓
        /// <summary>
        /// command/response GetDateTime
        /// </summary>
        GetDateTime,
        // @@@ add USF-200_IoT対応 2017.12.20 by Hemmi ↑

        /// <summary>
        /// command/response SetOperator
        /// </summary>
        SetOperator,

        /// <summary>
        /// command/response GetOperator
        /// </summary>
        GetOperator,

        /// <summary>
        /// command/response SetCurrency
        /// </summary>
        SetCurrency,

        /// <summary>
        /// command/response GetCurrency
        /// </summary>
        GetCurrency,

        /// <summary>
        /// command/response GetCurrencyInformation
        /// </summary>
        GetCurrencyInformation,

        /// <summary>
        /// command/response ResetStacker
        /// </summary>
        ResetStacker,

        /// <summary>
        /// command/response SetHotList
        /// </summary>
        SetHotList,

        /// <summary>
        /// command/response GetSerialNumberImage
        /// </summary>
        GetSerialNumberImage,

        // Event

        /// <summary>
        /// event Connected
        /// </summary>
        Connected,

        /// <summary>
        /// event Disconnectde
        /// </summary>
        Disconnected,

        /// <summary>
        /// event SessionClosed
        /// </summary>
        SessionClosed,

        /// <summary>
        /// event DeviceStatusUpdated
        /// </summary>
        DeviceStatusUpdated,

        /// <summary>
        /// event CountingResultUpdated
        /// </summary>
        CountingResultUpdated,

        /// <summary>
        /// event ButtonTouched
        /// </summary>
        ButtonTouched,

        /// <summary>
        /// event TransactionDetected
        /// </summary>
        TransactionDetected,

        /// <summary>
        /// event BanknoteTransported
        /// </summary>
        BanknoteTransported,

        /// <summary>
        /// event OperatorChanged
        /// </summary>
        OperatorChanged,

        /// <summary>
        /// event CurrencyChanged
        /// </summary>
        CurrencyChanged,

        /// <summary>
        /// event StackerPatternChanged
        /// </summary>
        StackerPatternChanged,

        /// <summary>
        /// event NoteTransported
        /// </summary>
        NoteTransported,

        /// <summary>
        /// eventConter CounterfeitDetected
        /// </summary>
        CounterfeitDetected,

        /// <summary>
        /// event OperationOccured
        /// </summary>
        OperationOccured,

        /// <summary>
        /// event ScreenChanged
        /// </summary>
        ScreenChanged

    }
}
