//using Oracle.ManagedDataAccess.Client;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace OSS_Domain
//{
//    public class DBOPEARATION
//    {
//        public DataTable DBExecuteDT(string SQL, string ConnStrOracle,DataTable DT,Dictionary<string,object> Parameters,out long SQLCODE)
//        {
//            try
//            {
//                SQLCODE = 0;
//                string connectionString = "DATA SOURCE=10.19.109.16/arnt;PASSWORD=user5;PERSIST SECURITY INFO=True;USER ID=USER5";
//                //string oradb = "User Id=" + UserName + ";Password=" + Password + ";provider=msdaora;Data Source=(DESCRIPTION =" + "(ADDRESS = (PROTOCOL = TCP)(HOST = 10.20.102.64)(PORT = 1536))" + "(CONNECT_DATA =" + "(SERVER = DEDICATED)" + "(SERVICE_NAME = crmdb)));";
//                DT = new DataTable();

//                using (OracleConnection connection = new OracleConnection(connectionString))
//                using (OracleCommand command = new OracleCommand(SQL, connection))
//                {

//                    foreach (var Key in Parameters.Keys)
//                    {
//                        OracleParameter Parameter = new OracleParameter(Key, Parameters[Key]);
//                        Parameter.OracleDbType = OracleDbType.Char;
//                        command.Parameters.Add(Parameter);
//                    }
//                    command.BindByName = true;
//                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);
//                    dataAdapter.Fill(DT);
//                }
//                if (SQL.ToLower().Trim().StartsWith("select"))
//                {
//                    if (DT.Rows.Count == 0)
//                        SQLCODE = 1403;
//                    else if (DT.Rows.Count > 1)
//                        SQLCODE = 2112;
//                    else if (DT.Columns.Count == 1 && DT.Rows[0][0] == DBNull.Value)
//                        SQLCODE = 1405;


//                }
                

//                return DT;
//            }
//            catch (OracleException e)
//            {
//                SQLCODE = e.ErrorCode;                
//                return null;
//            }

                     
//        }
//    }
//}
