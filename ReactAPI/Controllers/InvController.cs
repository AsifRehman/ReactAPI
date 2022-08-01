using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using WebAPI.Models;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Net;
using WebAPI.Helpers;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvController : ControllerBase
{
    DataTable mTable = new();
    DataTable dTable = new();

    private readonly IConfiguration _configuration;
    private readonly SqlHelper h;

    private string mainQuery = @"SELECT g.cashAc, g.TType, g.VocNo, g.Date, g.id, g.SrNo, g.PartyID, p.PartyName, g.Description, g.NetDebit, g.NetCredit FROM dbo.tbl_Ledger g INNER JOIN tbl_Party p ON p.PartyNameID = g.PartyID WHERE g.VocNo=searchVocNo AND g.TType='searchTType' ORDER BY SrNo";

    public InvController(IConfiguration configuration)
    {
        h = new(configuration);
        _configuration = configuration;
        mTable.Columns.Add("CashAc", typeof(long));
        mTable.Columns.Add("TType", typeof(string));
        mTable.Columns.Add("VocNo", typeof(int));
        mTable.Columns.Add("Date", typeof(DateTime));
        mTable.Columns.Add("Trans", typeof(DataTable));

        dTable.Columns.Add("id", typeof(int));
        dTable.Columns.Add("SrNo", typeof(int));
        dTable.Columns.Add("PartyID", typeof(int));
        dTable.Columns.Add("PartyName", typeof(string));
        dTable.Columns.Add("Description", typeof(string));
        dTable.Columns.Add("NetDebit", typeof(long));
        dTable.Columns.Add("NetCredit", typeof(long));
        dTable.Columns.Add("IsDeleted", typeof(bool));

    }

    [HttpGet("{ttype}/{vocno}")]
    public async Task<JsonResult> Get(string ttype, int vocno)
    {
        await GetData(ttype, vocno);
        return new JsonResult(mTable);
    }

    [HttpGet]
    public async Task<JsonResult> Get()
    {
        string query = @"SELECT TOP 10 id, VocNo, SrNo, Date, Description, NetDebit, NetCredit FROM dbo.tbl_Ledger";

        DataTable mTable = new();
        mTable.Load(await h.GetReaderBySQL(query));
        return new JsonResult(mTable);
    }


    [HttpPost]
    public async Task<JsonResult> Post(LedgerM g)
    {
        if (g.VocNo > 0)
        {
            var jsonResult = new JsonResult("Post Data Only Accepts VocNo equals 0");
            jsonResult.StatusCode = ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return jsonResult;
        }

        try
        {
            string query = $@"INSERT INTO dbo.tbl_Ledger" +
                "(CashAc, TType, VocNo, Date, SrNo, PartyId, Description, NetDebit,NetCredit) VALUES";
            int i = 0;
            string? dr = "";
            string? cr = "";
            string? cashAc = "";
            cashAc = g.CashAc == null ? "null" : g.CashAc.ToString();
            foreach (var d in g.Trans)
            {
                i += 1; //row no
                dr = d.NetDebit == null ? "null" : d.NetDebit.ToString();
                cr = d.NetCredit == null ? "null" : d.NetCredit.ToString();
                if (g.VocNo == 0)
                {
                    g.VocNo = await GetNewVocNo(g.TType);

                }
                query += $"({cashAc},'{g.TType}',{g.VocNo},'{g.Date.ToString("yyyy-MM-dd")}',{i},{d.PartyId},'{d.Description}',{dr},{cr})";
                if (g.Trans.Count != i)
                {
                    query += ",";
                }
            }
            //return new JsonResult(query);
            int recs;

            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
                {
                    recs = myCommand.ExecuteNonQuery();
                }
                query = mainQuery.Replace("searchVocNo", g.VocNo.ToString()).Replace("searchTType", g.TType);

                using (SqlCommand myCommand = new(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    myReader.Close();
                    myCon.Close();
                }
            }
            mTable = await GetData(g.TType, g.VocNo);
            return new JsonResult(mTable);
        }
        catch (Exception ex)
        {
            var jsonResult = new JsonResult(ex.Message);
            jsonResult.StatusCode = ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return jsonResult;
        }


    }
    #region Methods
    private async Task<int> GetNewVocNo(string v)
    {
        string query = $"SELECT ISNULL(MAX(VocNo),0)+1 FROM tbl_Ledger WHERE TType='{v}'";
        int i = await h.GetExecuteScalarByStr(query);
        return i;
    }

    private async Task<DataTable> GetData(string ttype, int vocno)
    {
        string query = mainQuery.Replace("searchVocNo", vocno.ToString()).Replace("searchTType", ttype);

        SqlDataReader myReader;
        myReader = await h.GetReaderBySQL(query);
        if (myReader.HasRows)
        {
            while (myReader.Read())
            {
                if (mTable.Rows.Count == 0)
                {
                    mTable.Rows.Add(
                        myReader.IsDBNull(0) ? null : (long)myReader.GetInt32(0), //cashAc
                        myReader.GetString(1), //ttype
                        myReader.GetInt32(2), //vocno
                        myReader.GetDateTime(3)); //Date
                }
                dTable.Rows.Add(
                    myReader.GetInt32(4), //id
                    myReader.GetInt32(5), //srno
                    myReader.GetInt32(6), //partyid
                    myReader.GetString(7), //partyname
                    myReader.IsDBNull(8) ? null : myReader.GetString(8), //description
                    myReader.IsDBNull(9) ? null : (long)myReader.GetDecimal(9),
                    myReader.IsDBNull(10) ? null : (long)myReader.GetDecimal(10),
                    false);
            }

            mTable.Rows[0]["Trans"] = dTable;
        }

        return mTable;
    }
    #endregion

    [HttpPut]
    public async Task<JsonResult> Put(LedgerM newG)
    {
        if (newG.VocNo == 0)
        {
            var jsonResult = new JsonResult("Put(Update) Data Only Accepts VocNo greater than 0");
            jsonResult.StatusCode = ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return jsonResult;
        }

        try
        {
            string query = mainQuery.Replace("searchVocNo", newG.VocNo.ToString()).Replace("searchTType", newG.TType);
            string varSql = "";
            var varSqls = new List<string>();
            string? dr = "";
            string? cr = "";
            string? cashAc = "";

            LedgerM oldM = new();
            LedgerD oldD;

            SqlDataReader myReader;
            int i = 1;
            myReader = await h.GetReaderBySQL(query);
            if (myReader.HasRows)
            {
                while (myReader.Read())
                {
                    varSql = "";
                    oldD = new LedgerD();
                    if (oldM.VocNo == 0)
                    {
                        oldM.CashAc = myReader.IsDBNull(0) ? null : myReader.GetInt32(0);
                        oldM.TType = myReader.GetString(1);
                        oldM.VocNo = myReader.GetInt32(2);
                        oldM.Date = myReader.GetDateTime(3);
                    }

                    oldD.Id = myReader.GetInt32(4); //id
                    oldD.SrNo = myReader.GetInt32(5); //srno
                    oldD.PartyId = myReader.GetInt32(6); //partyid
                    oldD.PartyName = myReader.GetString(7); //partyname
                    oldD.Description = myReader.IsDBNull(8) ? null : myReader.GetString(8); //description
                    oldD.NetDebit = myReader.IsDBNull(9) ? null : (long)myReader.GetDecimal(9);
                    oldD.NetCredit = myReader.IsDBNull(10) ? null : (long)myReader.GetDecimal(10);


                    LedgerD? newCur = newG.Trans.Find(x => x.Id == oldD.Id);
                    newG.Trans.RemoveAll(x => x.Id == oldD.Id);

                    if (newCur != null)
                    {
                        if (newCur.isDeleted)
                        {
                            varSql = $"DELETE FROM tbl_Ledger WHERE TType='{newG.TType}' AND VocNo={newG.VocNo} AND Id={newCur.Id}";
                            varSqls.Add(varSql);
                        }
                        else
                        {
                            if (newG.CashAc != oldM.CashAc) varSql += $"CashAc={newG.CashAc},";
                            if (newG.Date != oldM.Date) varSql += $"[Date]='{newG.Date.ToString("yyyy-MM-dd")}',";
                            if (newCur.SrNo != i) varSql += $"SrNo={i++},";
                            //                                    if (newCur.SrNo != oldD.SrNo) varSql += $"SrNo={newCur.SrNo},";
                            if (newCur.PartyId != oldD.PartyId) varSql += $"PartyId={newCur.PartyId},";
                            if (newCur.Description != oldD.Description) varSql += $"Description='{newCur.Description}',";
                            if (newCur.NetDebit == null)
                                varSql += $"NetDebit=NULL,";
                            else
                                varSql += $"NetDebit={newCur.NetDebit},";
                            if (newCur.NetCredit != oldD.NetCredit)
                                if (newCur.NetCredit == null)
                                    varSql += $"NetCredit=NULL,";
                                else
                                    varSql += $"NetCredit={newCur.NetCredit},";

                            if (varSql.Length > 0)
                                varSqls.Add($"UPDATE tbl_Ledger SET {varSql.Substring(0, varSql.Length - 1)} WHERE id ={ oldD.Id }");
                        }
                    }
                }
            }
            foreach (var nd in newG.Trans)
            {
                if (nd.isDeleted == false)
                {
                    dr = nd.NetDebit == null ? "NULL" : nd.NetDebit.ToString();
                    cr = nd.NetCredit == null ? "NULL" : nd.NetCredit.ToString();
                    cashAc = newG.CashAc == null ? "null" : newG.CashAc.ToString();

                    varSql = @"INSERT INTO dbo.tbl_Ledger" +
                        " (TType, VocNo, Date, SrNo, PartyId, Description, NetDebit,NetCredit, cashAc) VALUES";
                    varSql += $"('{newG.TType}',{newG.VocNo},'{newG.Date.ToString("yyyy-MM-dd")}',{i++},{nd.PartyId},'{nd.Description}',{dr},{cr},{cashAc})";

                    varSqls.Add(varSql);
                }
            }

            if (varSqls.Count == 0)
                return new JsonResult($"Already Updated");

            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
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

            mTable = await GetData(newG.TType, newG.VocNo);
            return new JsonResult(mTable);
        }
        catch (Exception ex)
        {

            var jsonResult = new JsonResult(ex.Message);
            jsonResult.StatusCode = ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return jsonResult;
        }


    }


    [HttpDelete("{ttype}/{vocno}")]
    public JsonResult Delete(string ttype, int vocno)
    {
        string query = $@"DELETE FROM dbo.tbl_Ledger WHERE TType='{ttype}' AND VocNo={vocno}";
        string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
        int cnt;
        using (SqlConnection myCon = new(sqlDataSource))
        {
            myCon.Open();
            using (SqlCommand myCommand = new(query, myCon))
            {
                cnt = myCommand.ExecuteNonQuery();
                myCon.Close();
            }
        }

        return new JsonResult($"{cnt} Records Deleted Successfully");
    }

}
