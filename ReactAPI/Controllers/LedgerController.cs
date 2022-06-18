using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using WebAPI.Models;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private string mainQuery = $@"SELECT g.TType, g.VocNo, g.Date, g.id, g.SrNo, g.PartyID, g.Description, g.NetDebit, g.NetCredit, p.PartyName FROM dbo.tbl_Ledger g INNER JOIN tbl_Party p ON p.PartyNameID = g.PartyID WHERE g.VocNo={g.VocNo} AND g.TType='{g.TType}' ORDER BY SrNo";

        public LedgerController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet("test")]
        public bool Test()
        {
            LedgerM m1 = new LedgerM();
            LedgerM m2 = m1;
            m1.VocNo = 1;
            m2.VocNo = 1;
            return m1.Equals(m2);
        }
        [HttpGet("{ttype}/{vocno}")]
        public JsonResult Get(string ttype, int vocno)
        {
            string query = mainQuery;

            DataTable mTable = new DataTable();
            DataTable dTable = new DataTable();

            mTable.Columns.Add("TType", typeof(string));
            mTable.Columns.Add("VocNo", typeof(int));
            mTable.Columns.Add("Date", typeof(DateTime));
            mTable.Columns.Add("Trans", typeof(DataTable));

            dTable.Columns.Add("id", typeof(int));
            dTable.Columns.Add("SrNo", typeof(int));
            dTable.Columns.Add("PartyID", typeof(int));
            dTable.Columns.Add("PartyName", typeof(int));
            dTable.Columns.Add("Description", typeof(string));
            dTable.Columns.Add("NetDebit", typeof(Int64));
            dTable.Columns.Add("NetCredit", typeof(Int64));

            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    if (myReader.HasRows)
                    {
                        while (myReader.Read())
                        {
                            if (mTable.Rows.Count == 0)
                            {
                                mTable.Rows.Add(
                                    myReader.GetString(0), //ttype
                                    myReader.GetInt32(1), //vocno
                                    myReader.GetDateTime(2)); //Date
                            }
                            dTable.Rows.Add(
                                myReader.GetInt32(3), //id
                                myReader.GetInt32(4), //srno
                                myReader.GetInt32(5), //partyid
                                myReader.IsDBNull(6) ? null : myReader.GetString(6),//description
                                myReader.IsDBNull(7) ? null : (Int64)myReader.GetDecimal(7),
                                myReader.IsDBNull(8) ? null : (Int64)myReader.GetDecimal(8),
                                myReader.GetString(9)); //partyname
                        }

                        mTable.Rows[0]["Trans"] = dTable;
                    }
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(mTable);
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"SELECT TOP 10 id, VocNo, SrNo, Date, Description, NetDebit, NetCredit FROM dbo.tbl_Ledger";

            DataTable mTable = new DataTable();
            DataTable dTable = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    mTable.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(mTable);
        }


        [HttpPost]
        public JsonResult Post(LedgerM g)
        {
            DataTable mTable = new DataTable();
            DataTable dTable = new DataTable();

            mTable.Columns.Add("TType", typeof(string));
            mTable.Columns.Add("VocNo", typeof(int));
            mTable.Columns.Add("Date", typeof(DateTime));
            mTable.Columns.Add("Trans", typeof(DataTable));

            dTable.Columns.Add("id", typeof(int));
            dTable.Columns.Add("SrNo", typeof(int));
            dTable.Columns.Add("PartyID", typeof(int));
            dTable.Columns.Add("Description", typeof(string));
            dTable.Columns.Add("NetDebit", typeof(Int64));
            dTable.Columns.Add("NetCredit", typeof(Int64));


            string query = $@"INSERT INTO dbo.tbl_Ledger" +
                "(TType, VocNo, Date, SrNo, PartyId, Description, NetDebit,NetCredit) VALUES";
            int i = 0;
            string? dr = "";
            string? cr = "";
            foreach (var d in g.Trans)
            {
                i += 1; //row no
                dr = d.NetDebit == null ? "null" : d.NetDebit.ToString();
                cr = d.NetCredit == null ? "null" : d.NetCredit.ToString();
                if (g.VocNo == 0)
                {
                    g.VocNo = GetNewVocNo(g.TType);

                }
                query += $"('{g.TType}',{g.VocNo},'{g.Date.ToString("yyyy-MM-dd")}',{d.SrNo},{d.PartyId},'{d.Description}',{dr},{cr})";
                if (g.Trans.Count != i)
                {
                    query += ",";
                }
            }
            //return new JsonResult(query);
            int recs;

            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    recs = myCommand.ExecuteNonQuery();
                }
                query = mainQuery;

                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();

                    if (myReader.HasRows)
                    {
                        while (myReader.Read())
                        {
                            if (mTable.Rows.Count == 0)
                            {
                                mTable.Rows.Add(
                                    myReader.GetString(0), //ttype
                                    myReader.GetInt32(1), //vocno
                                    myReader.GetDateTime(2)); //Date
                            }
                            dTable.Rows.Add(
                                myReader.GetInt32(3), //id
                                myReader.GetInt32(4), //srno
                                myReader.GetInt32(5), //partyid
                                myReader.IsDBNull(6) ? null : myReader.GetString(6),//description
                                myReader.IsDBNull(7) ? null : (Int64)myReader.GetDecimal(7),
                                myReader.IsDBNull(8) ? null : myReader.GetDecimal(8),
                                myReader.GetString(9)); //partyname
                        }

                        mTable.Rows[0]["Trans"] = dTable;
                    }
                    myReader.Close();

                    myCon.Close();
                }
            }
            return new JsonResult(mTable);
        }

        private int GetNewVocNo(string v)
        {
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                string query = $"SELECT ISNULL(MAX(VocNo),0)+1 FROM tbl_Ledger WHERE TType='{v}'";
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    int i = (int)myCommand.ExecuteScalar();
                    myCon.Close();
                    return i;
                }
            }
        }

        [HttpPut]
        public JsonResult Put(LedgerM newG)
        {
            string query = mainQuery;
            string varSql = "";
            List<string> varSqls = new List<string>();
            string? dr = "";
            string? cr = "";

            LedgerM oldG = new LedgerM();
            LedgerD od;

            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    if (myReader.HasRows)
                    {
                        while (myReader.Read())
                        {
                            varSql = "";
                            od = new LedgerD();
                            if (oldG.VocNo == 0)
                            {
                                oldG.TType = myReader.GetString(0);
                                oldG.VocNo = myReader.GetInt32(1);
                                oldG.Date = myReader.GetDateTime(2);
                            }

                            od.Id = myReader.GetInt32(3); //id
                            od.SrNo = myReader.GetInt32(4); //srno
                            od.PartyId = myReader.GetInt32(5); //partyid
                            od.Description = myReader.IsDBNull(6) ? null : myReader.GetString(6); //description
                            od.NetDebit = myReader.IsDBNull(7) ? null : (Int64)myReader.GetDecimal(7);
                            od.NetCredit = myReader.IsDBNull(8) ? null : (Int64)myReader.GetDecimal(8);
                            od.PartyName = myReader.GetString(9); //partyname


                            LedgerD check = newG.Trans.Find(x => x.Id == od.Id);
                            newG.Trans.RemoveAll(x => x.Id == od.Id);

                            if (check != null)
                            {
                                if (check.isDeleted)
                                {
                                    varSql = $"DELETE FROM tbl_Ledger WHERE TType='{newG.TType}' AND VocNo={newG.VocNo} AND Id={check.Id}";
                                    varSqls.Add(varSql);
                                }
                                else
                                {
                                    if (newG.Date != oldG.Date) varSql += $"[Date]='{newG.Date.ToString("yyyy - MM - dd")}',";
                                    if (check.SrNo != od.SrNo) varSql += $"SrNo={check.SrNo},";
                                    if (check.PartyId != od.PartyId) varSql += $"PartyId={check.PartyId},";
                                    if (check.Description != od.Description) varSql += $"Description='{check.Description}',";
                                    if (check.NetDebit != od.NetDebit) varSql += $"NetCredit={check.NetDebit},";
                                    if (check.NetCredit != od.NetCredit) varSql += $"NetCredit={check.NetCredit},";

                                    if (varSql.Length > 0)
                                        varSqls.Add($"UPDATE tbl_Ledger SET {varSql.Substring(0, varSql.Length - 1)} WHERE id ={ od.Id }");
                                }
                            }
                            //else
                            //{
                            //    dr = od.NetDebit == null ? "NULL" : od.NetDebit.ToString();
                            //    cr = od.NetCredit == null ? "NULL" : od.NetCredit.ToString();

                            //    varSql = @"INSERT INTO dbo.tbl_Ledger" +
                            //        " (TType, VocNo, Date, SrNo, PartyId, Description, NetDebit,NetCredit) VALUES";
                            //    varSql += $"('{newG.TType}',{newG.VocNo},'{newG.Date.ToString("yyyy-MM-dd")}',{od.SrNo},{od.PartyId},'{od.Description}',{dr},{cr})";

                            //    varSqls.Add(varSql);
                            //}

                        }
                    }
                    foreach (var nd in newG.Trans)
                    {
                        if (nd.isDeleted == false)
                        {
                            dr = nd.NetDebit == null ? "NULL" : nd.NetDebit.ToString();
                            cr = nd.NetCredit == null ? "NULL" : nd.NetCredit.ToString();

                            varSql = @"INSERT INTO dbo.tbl_Ledger" +
                                " (TType, VocNo, Date, SrNo, PartyId, Description, NetDebit,NetCredit) VALUES";
                            varSql += $"('{newG.TType}',{newG.VocNo},'{newG.Date.ToString("yyyy-MM-dd")}',{nd.SrNo},{nd.PartyId},'{nd.Description}',{dr},{cr})";

                            varSqls.Add(varSql);
                        }
                    }
                    myReader.Close();
                    myCon.Close();
                }
            }
            if (varSqls.Count == 0)
                return new JsonResult($"Already Updated");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    SqlTransaction t = myCon.BeginTransaction();
                    myCommand.Transaction = t;
                    foreach (var sq in varSqls)
                    {
                        myCommand.CommandText = sq;
                        myCommand.ExecuteNonQuery();
                    }
                    myCommand.Transaction.Commit();
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult($"{string.Join(";", varSqls)}");
        }


        [HttpDelete("{ttype}/{vocno}")]
        public JsonResult Delete(string ttype, int vocno)
        {
            string query = $@"DELETE FROM dbo.tbl_Ledger WHERE TType='{ttype}' AND VocNo={vocno}";
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            int cnt;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    cnt = myCommand.ExecuteNonQuery();
                    myCon.Close();
                }
            }

            return new JsonResult($"{cnt} Records Deleted Successfully");
        }

    }
}
