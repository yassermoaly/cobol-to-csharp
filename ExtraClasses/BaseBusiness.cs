//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace OSS_Domain
//{
//    public class BaseBusiness
//    {
//        public long SQLCODE;
//        public BaseBusiness()
//        {
//            DBOPEARATION = new DBOPEARATION();
//            Cursors = new Cursors();
//        }

//        protected long IBDATE = 20211006;
//        protected long APPL_STATUS, APPL_EXTENDED_STATUS;
//        protected string SQL { get; set; }
//        protected Cursors Cursors { get; set; }
//        protected Dictionary<string,Object> Parameters { get; set; }
//        protected string ConnStrOracle { get; set; }
//        protected DBOPEARATION DBOPEARATION { get; set; }
//        protected DataTable DT { get; set; }       
//        protected int ZEROS
//        {
//            get
//            {
//                return 0;
//            }
//        }
//        protected int ZERO
//        {
//            get
//            {
//                return 0;
//            }
//        }
//        protected string SPACES
//        {
//            get
//            {
//                return "";
//            }
//        }
//        protected string SPACE
//        {
//            get
//            {
//                return "";
//            }
//        }
//    }
//}
