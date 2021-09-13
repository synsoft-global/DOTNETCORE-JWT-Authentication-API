using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace NetCoreDAL
{
    public class SQLManager : IDisposable
    {
        private readonly IConfiguration _configuration;
        private string g_ConStr = string.Empty;
        private SqlConnection g_ObjCon;
        private SqlCommand g_ObjSQLCmd;
        private int g_Timeout = 60;

        public SQLManager(IConfiguration configuration, string p_ConStr = "", int p_Timeout = 0)
        {
            _configuration = configuration;
            try
            {
                //Prod
                if (p_ConStr.Trim().Length == 0)
                {
                    p_ConStr = _configuration.GetConnectionString("DefaultConnection").ToString();
                }
                if ((p_ConStr == null ? string.Empty : p_ConStr.Trim()).Length != 0)
                {
                    g_ConStr = (p_ConStr.Trim().Length == 0 ? p_ConStr : p_ConStr);
                    //g_ConStr = @"Data Source=DESKTOP-HAVJT89\SQLNEW;Initial Catalog=BuzzMate_Prod;User ID=testconnect;Password=12345678";
                }

                //Make Sure Connection Info ends properly.
                if (g_ConStr.Trim().Length > 0)
                {
                    g_ConStr = (g_ConStr + ";").Replace(";;", ";");
                }

                #region Custom Settings
                //Apply Custom Connection Timeout
                if (g_ConStr.Length > 0 && p_Timeout > 0)
                {
                    g_Timeout = p_Timeout;
                    if (g_ConStr.Contains("Connection Timeout=120;") == true)
                    {
                        g_ConStr = g_ConStr.Replace("Connection Timeout=120;", "");
                    }
                    if (g_ConStr.Contains("Connection Timeout=60;") == true)
                    {
                        g_ConStr = g_ConStr.Replace("Connection Timeout=60;", "");
                    }
                    g_ConStr += "Connection Timeout=" + p_Timeout.ToString() + ";";
                }

                //Apply default connection timeout if not exists
                if (g_ConStr.Length > 0 && g_ConStr.Contains("Connection Timeout") == false)
                {
                    g_ConStr += "Connection Timeout=60;";
                }
                #endregion

                //initiate connection
                DBCon();
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-DBMGR: " + ex.Message);
            }
        }//end of Constructor

        private void DBCon()
        {
            try
            {
                if (g_ConStr.Trim().Length == 0)
                {
                    throw new Exception("Err-DBM: Connection string is not available.");
                }
                if (g_ObjCon == null)
                {
                    g_ObjCon = new SqlConnection(g_ConStr);
                }//end of if
                if (g_ObjCon.State == System.Data.ConnectionState.Open)
                {
                    WaitIfConnectionBusy();
                }
                if (g_ObjCon.State == System.Data.ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }//end of if
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-001: " + ex);
            }
        }//end of function 

        private void WaitIfConnectionBusy()
        {
            int i = 0;
            //opening connection and wait if connection is still busy
            while (g_ObjCon.State != ConnectionState.Closed)
            {
                i += 1;
                if (i == 50000)
                {
                    g_ObjCon.Dispose(); g_ObjCon = null;
                }
            }//end of while
        }//end of function

        protected virtual void Dispose(bool p_disposing)
        {
            if (p_disposing == true)
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                }
                if (g_ObjCon != null)
                {
                    g_ObjCon.Dispose();
                }
                GC.SuppressFinalize(this);
            }
        }//end of function

        public void DBClose()
        {
            if (g_ObjCon == null) { return; }
            try
            {
                if (g_ObjCon.State == System.Data.ConnectionState.Open)
                {
                    g_ObjCon.Close();
                    g_ObjCon.Dispose();
                    GC.SuppressFinalize(this);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }//end of function

        public void Dispose()
        {
            try
            {
                if (g_ObjCon == null)
                {
                    return;
                }//end of if
                if (g_ObjCon.State == System.Data.ConnectionState.Open)
                {
                    g_ObjCon.Close();
                }//end of if
                Dispose(true);
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-002: " + ex);
            }
            finally
            {
                g_ObjCon.Dispose();
                g_ObjCon = null;
            }
        }//end of function

        public DataSet GetDS(string p_StrQry) //Specifically for Database Restore, Backups & Migration Stored Procedures
        {
            DataSet DS = new DataSet();
            try
            {
                SqlDataAdapter ObjSDA = new SqlDataAdapter(p_StrQry, g_ObjCon);
                ObjSDA.SelectCommand.CommandTimeout = 300;
                ObjSDA.Fill(DS);
                ObjSDA.Dispose(); ObjSDA = null;
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-004: " + ex);
            }
            return DS;
        }//end of function

        public void UpdateDB(string p_StrQry)
        {
            try
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand(p_StrQry, g_ObjCon);
                g_ObjSQLCmd.CommandType = CommandType.Text;
                g_ObjSQLCmd.Transaction = g_ObjCon.BeginTransaction();
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                g_ObjSQLCmd.ExecuteNonQuery();
                g_ObjSQLCmd.Transaction.Commit();

            }
            catch (System.Exception ex)
            {
                if (g_ObjSQLCmd != null && g_ObjSQLCmd.Transaction != null)
                {
                    g_ObjSQLCmd.Transaction.Rollback();
                }
                throw new Exception("DBM-003: " + ex);
            }
            finally
            {
                g_ObjSQLCmd.Dispose();
                g_ObjSQLCmd = null;
            }
        }//end of function

        public void UpdateDB(SqlCommand p_objSQLCmd, bool p_IsStoredProcedure = false)
        {
            try
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand();
                p_objSQLCmd.Connection = g_ObjCon;
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                g_ObjSQLCmd = p_objSQLCmd;
                if (p_IsStoredProcedure == true)
                {
                    g_ObjSQLCmd.CommandType = CommandType.StoredProcedure;
                }
                if (g_ObjCon.State == ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }
                g_ObjSQLCmd.Transaction = g_ObjCon.BeginTransaction();
                g_ObjSQLCmd.ExecuteNonQuery();
                g_ObjSQLCmd.Transaction.Commit();

            }
            catch (System.Exception ex)
            {
                if (g_ObjSQLCmd != null && g_ObjSQLCmd.Transaction != null)
                {
                    g_ObjSQLCmd.Transaction.Rollback();
                }
                throw new Exception("DBM-004i: " + ex);
            }
            finally
            {
                g_ObjSQLCmd.Dispose();
                g_ObjSQLCmd = null;
            }
        }//end of function

        public void UpdateDB(SqlCommand[] p_objSQLCmd)
        {
            if (p_objSQLCmd.Length == 0)
            {
                throw new Exception("Batch execution aborted! List of SQL commands are not available.");
            }
            try
            {
                //Verify sqlCommand Array Commands Text
                try
                {
                    for (int i = 0; i < p_objSQLCmd.Length; i++)
                    {
                        if (p_objSQLCmd[i].CommandText.Trim().Length == 0)
                        {
                            throw new Exception("Batch execution aborted! SQL command is not available at position " + i.ToString());
                        }
                    }//end of loop
                }
                catch (System.Exception ex)
                {
                    throw new Exception("DBM-004iiv: " + ex);
                }

                //If array of sql commands are valid
                if (g_ObjCon.State == ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }

                SqlTransaction l_sqlTransactions;
                l_sqlTransactions = g_ObjCon.BeginTransaction();

                for (int i = 0; i < p_objSQLCmd.Length; i++)
                {
                    if (p_objSQLCmd[i].CommandText.Trim().Length > 0)
                    {
                        //Below code is helpful for debuging purpose
                        //string l_Qry = p_objSQLCmd[i].CommandText.Trim();
                        //for (int ii = 0; ii < p_objSQLCmd[i].Parameters.Count; ii++)
                        //{
                        //    l_Qry = l_Qry.Replace(p_objSQLCmd[i].Parameters[ii].ParameterName, p_objSQLCmd[i].Parameters[ii].Value.ToString());
                        //}
                        //Console.WriteLine(l_Qry);
                        p_objSQLCmd[i].Connection = g_ObjCon;
                        p_objSQLCmd[i].CommandTimeout = g_Timeout;
                        p_objSQLCmd[i].Transaction = l_sqlTransactions;
                        p_objSQLCmd[i].ExecuteScalar();
                    }
                }//end of loop
                l_sqlTransactions.Commit();

            }
            catch (System.Exception ex)
            {
                if (p_objSQLCmd != null && p_objSQLCmd[0].Transaction != null)
                {
                    p_objSQLCmd[0].Transaction.Rollback();
                }
                throw new Exception("DBM-004ii: " + ex);
            }
            finally
            {
                p_objSQLCmd = null;
            }
        }//end of function

        public DataSet FetchDB(string p_StrQry)
        {
            DataSet DS = new DataSet();
            try
            {
                if (g_ObjCon.State == ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }
                SqlDataAdapter ObjSDA = new SqlDataAdapter(p_StrQry, g_ObjCon);
                ObjSDA.SelectCommand.CommandTimeout = g_Timeout;
                ObjSDA.Fill(DS);
                ObjSDA.Dispose(); ObjSDA = null;
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-004: " + ex);
            }
            return DS;
        }//end of function

        public DataTable FetchDT(string p_StrQry)
        {
            DataTable l_DT = new DataTable();
            try
            {
                if (g_ObjCon.State != ConnectionState.Open)
                {
                    g_ObjCon.Open();
                }
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand(p_StrQry, g_ObjCon);
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                SqlDataReader l_DR = g_ObjSQLCmd.ExecuteReader(CommandBehavior.CloseConnection);
                l_DT.Load(l_DR);
                l_DR.Close();
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-004i: " + ex);
            }
            return l_DT;
        }//end of function

        //Synsoft Global (Vaibhav)
        //Start
        public DataTable FetchDT(SqlCommand p_objSQLCmd)
        {
            DataTable l_DT = new DataTable();
            try
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand();
                g_ObjSQLCmd = p_objSQLCmd;
                g_ObjSQLCmd.CommandType = CommandType.StoredProcedure;
                p_objSQLCmd.Connection = g_ObjCon;
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                if (g_ObjCon.State == ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }
                SqlDataReader l_DR = g_ObjSQLCmd.ExecuteReader(CommandBehavior.CloseConnection);
                l_DT.Load(l_DR);
                l_DR.Close();
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-004i: " + ex);
            }
            return l_DT;
        }//end of function

        public string FetchXML(SqlCommand p_objSQLCmd)
        {
            string xml = "";
            try
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand();
                g_ObjSQLCmd = p_objSQLCmd;
                g_ObjSQLCmd.CommandType = CommandType.StoredProcedure;
                p_objSQLCmd.Connection = g_ObjCon;
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                if (g_ObjCon.State == ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }
                xml = g_ObjSQLCmd.ExecuteScalar().ToString();
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-004i: " + ex);
            }
            return xml;
        }//end of function

        public DataSet FetchDB(SqlCommand p_objSQLCmd)
        {
            DataSet DS = new DataSet();
            try
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand();
                g_ObjSQLCmd = p_objSQLCmd;
                g_ObjSQLCmd.CommandType = CommandType.StoredProcedure;
                p_objSQLCmd.Connection = g_ObjCon;
                if (g_ObjCon.State == ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }
                SqlDataAdapter ObjSDA = new SqlDataAdapter(g_ObjSQLCmd);
                ObjSDA.SelectCommand.CommandTimeout = g_Timeout;
                ObjSDA.Fill(DS);
                ObjSDA.Dispose(); ObjSDA = null;
            }
            catch (System.Exception ex)
            {
                throw new Exception("DBM-004: " + ex);
            }
            return DS;
        }//end of function

        public void UpdateDB(DataTable p_DT, string p_StrQry)
        {
            try
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand(p_StrQry, g_ObjCon);
                SqlDataAdapter l_DA = new SqlDataAdapter(g_ObjSQLCmd);
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                l_DA.AcceptChangesDuringFill = l_DA.AcceptChangesDuringUpdate = true;
                SqlCommandBuilder l_SCB = new SqlCommandBuilder(l_DA);
                l_DA.UpdateCommand = l_SCB.GetUpdateCommand();
                l_DA.Update(p_DT);
                p_DT.AcceptChanges();
                l_DA.Dispose();
                l_DA = null;
                l_SCB.Dispose();
                l_SCB = null;
            }
            catch (System.Exception ex)
            {
                if (g_ObjSQLCmd != null && g_ObjSQLCmd.Transaction != null)
                {
                    g_ObjSQLCmd.Transaction.Rollback();
                }
                throw new Exception("DBM-005: " + ex);
            }
            finally
            {
                g_ObjSQLCmd.Dispose();
                g_ObjSQLCmd = null;
            }
        }//end of function

        public void UpdateDB(DataTable p_DT, string p_StrQry, bool p_insert)
        {
            try
            {
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand(p_StrQry, g_ObjCon);
                SqlDataAdapter l_DA = new SqlDataAdapter(g_ObjSQLCmd);
                l_DA.AcceptChangesDuringFill = l_DA.AcceptChangesDuringUpdate = true;
                SqlCommandBuilder l_SCB = new SqlCommandBuilder(l_DA);
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                if (p_insert == true)
                    l_DA.UpdateCommand = l_SCB.GetInsertCommand();
                else
                    l_DA.UpdateCommand = l_SCB.GetUpdateCommand();

                l_DA.Update(p_DT);
                p_DT.AcceptChanges();
                l_DA.Dispose(); l_DA = null;
                l_SCB.Dispose(); l_SCB = null;
            }
            catch (System.Exception ex)
            {
                if (g_ObjSQLCmd != null && g_ObjSQLCmd.Transaction != null)
                {
                    g_ObjSQLCmd.Transaction.Rollback();
                }
                throw new Exception("DBM-005i: " + ex);
            }
            finally
            {
                g_ObjSQLCmd.Dispose();
                g_ObjSQLCmd = null;
            }
        }//end of function

        public void PUpdateDB(SqlCommand p_objSQLCmd, bool p_IsStoredProcedure = false)
        {
            try
            {
                g_ObjCon.Close();
                g_ObjCon.Open();
                if (g_ObjSQLCmd != null)
                {
                    g_ObjSQLCmd.Dispose();
                    g_ObjSQLCmd = null;
                }
                g_ObjSQLCmd = new SqlCommand();
                p_objSQLCmd.Connection = g_ObjCon;
                g_ObjSQLCmd.CommandTimeout = g_Timeout;
                g_ObjSQLCmd = p_objSQLCmd;
                if (p_IsStoredProcedure == true)
                {
                    g_ObjSQLCmd.CommandType = CommandType.StoredProcedure;
                }
                if (g_ObjCon.State == ConnectionState.Closed)
                {
                    g_ObjCon.Open();
                }
                g_ObjSQLCmd.Transaction = g_ObjCon.BeginTransaction();
                g_ObjSQLCmd.ExecuteNonQuery();
                g_ObjSQLCmd.Transaction.Commit();

            }
            catch (System.Exception ex)
            {
                if (g_ObjSQLCmd != null && g_ObjSQLCmd.Transaction != null)
                {
                    g_ObjSQLCmd.Transaction.Rollback();
                }
                throw new Exception("DBM-004i: " + ex);
            }
            finally
            {
                g_ObjSQLCmd.Dispose();
                g_ObjSQLCmd = null;
            }
        }//end of function
    }
}
