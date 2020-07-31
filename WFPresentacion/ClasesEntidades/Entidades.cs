
namespace WFPresentacion.ClasesEntidades
{

    using System;
    using System.Collections.Generic;

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    /// <summary>
    /// trae detalle de lectura de dinero con los seriales
    /// </summary>
    public class Note
    {
        public int DenomiIndex { get; set; }
        public string SerialNumber { get; set; }
        public string SerialNumberLeft { get; set; }
        public string SerialNumberRight { get; set; }
        public bool Reject { get; set; }
        public string Category { get; set; }
        public string Factor { get; set; }
        public string Key { get; set; }
        public string BvResult { get; set; }
        public string UnfitCode { get; set; }
        public string CounterFeit { get; set; }
        public string SuspResult { get; set; }
        public string SuspDetail { get; set; }
        public string FitDetail { get; set; }

        public string TranKey { get; set; }

    }

    public class Detail
    {
        public List<Note> Notes { get; set; }

        /**********************Trae detalle del aceptar de lo contado********************/
        public int CurrencyIndex { get; set; }
        public int PatternIndex { get; set; }
        public TransactionData TransactionData { get; set; }
        public string TranKey { get; set; }

        /**********************FIN Trae detalle del aceptar de lo contado********************/

        /**********************Trae detalle del aceptar de lo contado********************/
        public string Result { get; set; }
        public List<Denomination> Denomination { get; set; }

        /***********************  devices status ************************************/
        public bool Transaction { get; set; }
        public bool Maintenance { get; set; }
        public string ApplicationStatus { get; set; }
        public DeviceStatus DeviceStatus { get; set; }

    }

    public class Root
    {
        public Detail Detail { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

    }

    /***********************************************************************************/

    /// <summary>
    /// trae la id de transaccion de lo contado al acceptar
    /// </summary>
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Count
    {
        public int DenomiIndex { get; set; }
        public int ATM { get; set; }
        public int UNFIT { get; set; }
        public int TELLER { get; set; }
        public int Suspect { get; set; }
        public int Counterfeit { get; set; }

    }

    public class TransactionData
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int TotalCount { get; set; }
        public int TotalAmount { get; set; }
        public List<Count> Counts { get; set; }

        public string TranKey { get; set; }

    }

    //public class Detail
    //{
    //    public int CurrencyIndex { get; set; }
    //    public int PatternIndex { get; set; }
    //    public TransactionData TransactionData { get; set; }
    //    public string TranKey { get; set; }

    //}

    public class RootTotal
    {
        public Detail Detail { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

    }

    /******************************************************/

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Denomination
    {
        public int DenomiIndex { get; set; }
        public int Value { get; set; }
        public int CurrencyIndex { get; set; }
        public int Version { get; set; }

    }

    //public class Detail
    //{
    //    public string Result { get; set; }
    //    public List<Denomination> Denomination { get; set; }

    //}

    public class RootDenominaciones
    {
        public Detail Detail { get; set; }
        public string ID { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

    }

    /***************************** getcurrency ************************/
    public class RootCurrency
    {
        public Detail Detail { get; set; }
        public string ID { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

    }
    /******************************************************************/


    /**********************************************************************/


    public class HopperStatus
    {
        public bool Exist { get; set; }
        public bool FailFeed { get; set; }

    }

    public class RejectStatus
    {
        public bool Exist { get; set; }
        public bool Full { get; set; }
        public bool Waiting { get; set; }

    }

    public class StackerStatu
    {
        public bool Exist { get; set; }
        public bool Full { get; set; }
        public bool Batch { get; set; }
        public bool Error { get; set; }
        public bool Waiting { get; set; }
        public bool Restoring { get; set; }

    }

    public class DeviceStatus
    {
        public string Status { get; set; }
        public bool CountMode { get; set; }
        public HopperStatus HopperStatus { get; set; }
        public RejectStatus RejectStatus { get; set; }
        public List<StackerStatu> StackerStatus { get; set; }

    }

    

    public class RootDeviseStatus
    {
        public Detail Detail { get; set; }
        public string ID { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

    }



}
