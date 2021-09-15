//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace OSS_Domain
//{
//    public class Cursor
//    {
//        private long SQLCODE = 0;
//        public Cursor(string Name, string Query, Dictionary<string, Object> Parameters)
//        {
//            this.Name = Name;
//            this.Query = Query;
//            this.Parameters = Parameters;
//        }
//        public string Name { get; set; }
//        private string Query { get; set; }
//        private Dictionary<string, Object> Parameters { get; set; }
//        public void Open()
//        {
            
//            Table = new DBOPEARATION().DBExecuteDT(Query,string.Empty, Table, Parameters, out SQLCODE);
//            //Fill Table
//        }
//        private DataTable Table { get; set; }

//        private int Index = 0;

//        public DataRow Fetch(out long SQLCODE)
//        {
//            if (Index < Table.Rows.Count)
//            {
//                Index++;
//                SQLCODE = 0;
//                return Table.Rows[Index-1];
//            }
//            SQLCODE = 1;
//            return null;
//        }
//    }
//}
