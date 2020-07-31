
namespace WFPresentacion.ClasesEntidades
{
    //using DocumentFormat.OpenXml.Drawing.Charts;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Configuration;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
   
    using System.ComponentModel;
    using Newtonsoft.Json.Linq;

    public class ClsAccess
    {
        System.Data.OleDb.OleDbConnection conn = new
                        System.Data.OleDb.OleDbConnection();

      
        public ClsAccess()
        {
        }
        private void ConnectToAccess()
        {
            var pathtxt = Directory.GetCurrentDirectory();
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathtxt + @"\DatosSerial.mdb;Persist Security Info=True";
        }

        #region Consultas
        public void cargarDataTotales(DataGridView dg, DataGridView dgt, DataGridView dgRechazados, string transaccion, bool reject)
        {
            try
            {
                string where = " where 1=1 ";
                string wheretransaccion = "";
                string wherereject = "";

                if (!string.IsNullOrEmpty(transaccion))
                {
                    wheretransaccion = " and TranKey = '" + transaccion + "'";
                }

                if (reject == true)
                {
                    wherereject = " and Reject = false";
                }


                ConnectToAccess();
                OleDbCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.Connection = conn;

                List<Denomination> listadedenominaciones = ListaDenominaciones(ref cmd);

                if (listadedenominaciones == null)
                {
                    throw new Exception("No existe listado de denominaciones");
                }

                string whereconstruction = where + wheretransaccion + wherereject;
                List<Note> listamoneys = ListaDinerosSerial(ref cmd, whereconstruction);


                whereconstruction = where + wheretransaccion;
                List<Detail> listatransacciones = ListaTransacciones(ref cmd, whereconstruction);

                var result = from m in listamoneys
                             join t in listatransacciones on new { m.TranKey } equals new { t.TranKey }
                             join d in listadedenominaciones
                                             on new { m.DenomiIndex }
                                             equals new { d.DenomiIndex }
                             select new
                             {
                                 m.TranKey,
                                 Denom = d.Value,
                                 m.SerialNumber,
                                 m.Reject
                             }
                                            ;
                dg.DataSource = result.ToList();


                var resulttotales = (from m in listamoneys
                                    join t in listatransacciones on new { m.TranKey } equals new { t.TranKey }
                                    join d in listadedenominaciones
                                                    on new { m.DenomiIndex }
                                                    equals new { d.DenomiIndex }
                                    where m.Reject == false
                                    orderby d.Value
                                    group new { d.Value } by d.Value into grupo
                                    select new {                                        
                                        Denom = "$ " + grupo.Key.ToString(), Cant = grupo.Count(), TotalValor = (grupo.Key * grupo.Count()) 
                                    }).ToList();


                int totalcant = resulttotales.Sum(item => item.Cant);
                int totalvalor = resulttotales.Sum(item => item.TotalValor);

                var o1 = new { Denom = "TOTALES ", Cant = totalcant, TotalValor = totalvalor };
                resulttotales.Add(o1);


                dgt.DataSource = resulttotales.ToList();

                var resulttotalesR = from m in listamoneys
                                     join t in listatransacciones on new { m.TranKey } equals new { t.TranKey }
                                     join d in listadedenominaciones
                                                     on new { m.DenomiIndex }
                                                     equals new { d.DenomiIndex }
                                     where m.Reject == true
                                     orderby d.Value
                                     group new { d.Value } by d.Value into grupo
                                     select new { Cant_Rechazadas = grupo.Count()};

                 
                dgRechazados.DataSource = resulttotalesR.ToList();

            }
            catch (OleDbException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public System.Data.DataTable TraeInformacion()
        {
            try
            {
                ConnectToAccess();
                OleDbCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.Connection = conn;

                List<Denomination> listadedenominaciones = ListaDenominaciones(ref cmd);
                if (listadedenominaciones == null)
                {
                    throw new Exception("No existe listado de denominaciones");
                }


                List<Note> listamoneys = ListaDinerosSerial(ref cmd, " where Reject = false");


                List<Detail> listatransacciones = ListaTransacciones(ref cmd, "");

                List<TransactionData> listatransaccionesdata = ListaTransaccionesData(cmd);

                var result = (from m in listamoneys
                             join t in listatransacciones on new { m.TranKey } equals new { t.TranKey }
                             join td in listatransaccionesdata on new { m.TranKey } equals new { td.TranKey }
                             join d in listadedenominaciones
                                             on new { m.DenomiIndex }
                                             equals new { d.DenomiIndex }
                             select new
                             {
                                 m.TranKey,
                                 td.StartDateTime,
                                 td.EndDateTime,
                                 Denom = d.Value,
                                 m.SerialNumber,
                                 m.Reject
                             }).ToList();

                var data = ToDataTable(result);

                conn.Close();
                return data;// result.CopyToDataTable();

            }
            catch (OleDbException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static System.Data.DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            System.Data.DataTable table = new System.Data.DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        #endregion

        #region Inserts
        public void insertarDetailSerialData(ClasesEntidades.Root root)
        {
            try
            {
                ConnectToAccess();
                OleDbCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.Connection = conn;



                foreach (Note item in root.Detail.Notes)
                {


                    string insert = "Insert into tblSerialData(DenomiIndex,SerialNumber,SerialNumberLeft,SerialNumberRight,Reject,[Key],Category,Factor,BvResult,UnfitCode,CounterFeit,SuspResult,SuspDetail,FitDetail)Values(" + item.DenomiIndex + ",'" + item.SerialNumber + "','" + item.SerialNumberLeft + "','" + item.SerialNumberRight + "'," + item.Reject.ToString() + ",'" + item.Key.ToString() + "','" + item.Category + "','" + item.Factor + "','" + item.BvResult + "','" + item.UnfitCode + "','" + item.CounterFeit + "','" + item.SuspResult + "','" + item.SuspDetail + "','" + item.FitDetail + "')";

                    cmd.CommandText = insert;
                    cmd.ExecuteNonQuery();
                }

                //MessageBox.Show("Record Submitted", "Congrats");
                conn.Close();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void insertarHeaderSerialData(ClasesEntidades.RootTotal root)
        {
            try
            {
                ConnectToAccess();
                OleDbCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.Connection = conn;

                string insert = "Insert into tblTransacciones(TranKey,CurrencyIndex)Values('" + root.Detail.TranKey + "'," + root.Detail.CurrencyIndex + ")";

                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                insert = "update tblSerialData set TranKey = '" + root.Detail.TranKey + "' where TranKey is null";

                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                insert = "Insert into tblTransactionData(TranKey,StartDateTime,EndDateTime,TotalCount,TotalAmount)Values('" + root.Detail.TranKey + "','" + root.Detail.TransactionData.StartDateTime.ToString() + "','" + root.Detail.TransactionData.EndDateTime.ToString() + "'," + root.Detail.TransactionData.TotalCount.ToString() + "," + root.Detail.TransactionData.TotalAmount.ToString() + ")";

                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                foreach (Count item in root.Detail.TransactionData.Counts)
                {
                    insert = "Insert into tblCountData(DenomiIndex,ATM,UNFIT,TELLER,Suspect,Counterfeit)Values(" + item.DenomiIndex + "," + item.ATM + "," + item.UNFIT + "," + item.TELLER + "," + item.Suspect + "," + item.Counterfeit + ")";

                    cmd.CommandText = insert;
                    cmd.ExecuteNonQuery();
                }

                //MessageBox.Show("Record Submitted", "Congrats");
                conn.Close();

            }
            catch (OleDbException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void insertDenominationData(ClasesEntidades.RootDenominaciones root)
        {
            try
            {
                ConnectToAccess();
                OleDbCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.Connection = conn;
                string insert;

                insert = "delete from tblDenomination";
                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                foreach (Denomination item in root.Detail.Denomination)
                {
                    insert = "Insert into tblDenomination(DenomiIndex,[Value],CurrencyIndex,Version)Values(" + item.DenomiIndex + "," + item.Value + "," + item.CurrencyIndex + "," + item.Version + ")";

                    cmd.CommandText = insert;
                    cmd.ExecuteNonQuery();
                }

                //MessageBox.Show("Record Submitted", "Congrats");
                conn.Close();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Listas

        public List<Denomination> ListaDenominaciones(ref OleDbCommand cmd)
        {
            List<Denomination> listadedenominaciones = null;
            try
            {
                listadedenominaciones = new List<Denomination>();
                cmd.CommandText = "select * from tblDenomination";
                OleDbDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    listadedenominaciones.Add(new Denomination()
                    {
                        DenomiIndex = Convert.ToInt32(reader["DenomiIndex"]),
                        Value = Convert.ToInt32(reader["Value"]),
                        CurrencyIndex = Convert.ToInt32(reader["CurrencyIndex"]),
                        Version = Convert.ToInt32(reader["Version"]),
                    });


                }
                // always call Close when done reading.
                reader.Close();

            }
            catch (OleDbException ex)
            {
                throw new Exception(ex.Message);
            }
            return listadedenominaciones;
        }

        public List<Note> ListaDinerosSerial(ref OleDbCommand cmd, string where)
        {
            string query = "select * from tblSerialData " + where;
            List<Note> listamoneys = null;
            try
            {
                cmd.CommandText = query;
                OleDbDataReader reader = cmd.ExecuteReader();

                listamoneys = new List<Note>();

                while (reader.Read())
                {
                    listamoneys.Add(new Note()
                    {
                        DenomiIndex = Convert.ToInt32(reader["DenomiIndex"]),
                        SerialNumber = reader["SerialNumber"].ToString(),
                        SerialNumberLeft = reader["SerialNumberLeft"].ToString(),
                        SerialNumberRight = reader["SerialNumberRight"].ToString(),
                        Reject = Convert.ToBoolean(reader["Reject"]),
                        Category = reader["Category"].ToString(),
                        Factor = reader["Factor"].ToString(),
                        Key = reader["Key"].ToString(),
                        BvResult = reader["BvResult"].ToString(),
                        UnfitCode = reader["UnfitCode"].ToString(),
                        CounterFeit = reader["CounterFeit"].ToString(),
                        SuspResult = reader["SuspResult"].ToString(),
                        SuspDetail = reader["SuspDetail"].ToString(),
                        FitDetail = reader["FitDetail"].ToString(),
                        TranKey = reader["TranKey"].ToString(),
                    });

                }
                // always call Close when done reading.
                reader.Close();

            }
            catch (OleDbException ex)
            {
                throw new Exception(ex.Message);
            }
            return listamoneys;
        }

        public List<Detail> ListaTransacciones(ref OleDbCommand cmd, string where)
        {
            List<Detail> listatransacciones = null;
            string query = "select * from tblTransacciones " + where;
            try
            {
                cmd.CommandText = query;
                OleDbDataReader reader = cmd.ExecuteReader();

                listatransacciones = new List<Detail>();

                while (reader.Read())
                {
                    listatransacciones.Add(new Detail()
                    {
                        CurrencyIndex = Convert.ToInt32(reader["CurrencyIndex"]),
                        TranKey = reader["TranKey"].ToString()
                    });


                }
                // always call Close when done reading.
                reader.Close();
            }
            catch (OleDbException ex)
            {
                throw new Exception(ex.Message);
            }

            return listatransacciones;
        }


        public List<TransactionData> ListaTransaccionesData(OleDbCommand cmd)
        {
            List<TransactionData> listatransacciones = null;
            try
            {
                cmd.CommandText = "select * from tblTransactionData ";
                OleDbDataReader reader = cmd.ExecuteReader();

                listatransacciones = new List<TransactionData>();

                while (reader.Read())
                {
                    listatransacciones.Add(new TransactionData()
                    {
                        TranKey = reader["TranKey"].ToString(),
                        StartDateTime = Convert.ToDateTime(reader["StartDateTime"].ToString()),
                        EndDateTime = Convert.ToDateTime(reader["EndDateTime"].ToString()),
                    });
                }
                // always call Close when done reading.
                reader.Close();
            }
            catch (OleDbException ex)
            {
                throw new Exception(ex.Message);
            }

            return listatransacciones;
        }

        #endregion

        public void Deleteinformatio()
        {
            try
            {
                ConnectToAccess();
                OleDbCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.Connection = conn;

                string insert = "delete from tblCountData;";
                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                insert = " delete from  tblSerialData;";
                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                insert = " delete from  tblTransacciones;";
                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                insert = " delete from  tblTransactionData;";
                cmd.CommandText = insert;
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
