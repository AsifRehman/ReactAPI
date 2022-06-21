using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using WebAPI.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Cors;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public PartyController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet("partieslist")]
        public JsonResult PartiesList()
        {
            string query = @"
                    select PartyNameId as PartyId, PartyName from dbo.tbl_Party ORDER BY PartyName";
            DataTable table = new();
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            string query = $@"
                    select PartyNameId as PartyId, PartyName, Debit,
                    Credit, PartyTypeId
                    from dbo.tbl_Party WHERE PartyNameID={id}";
            DataTable table = new();
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"
                    select PartyNameId as PartyId, PartyName, Debit,
                    Credit, PartyTypeId
                    from dbo.tbl_Party
                    ";
            DataTable table = new();
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }


        [HttpPost]
        public JsonResult Post(Party p)
        {
            string? dr = p.Debit == null ? "NULL" : p.Debit.ToString();
            string? cr = p.Credit== null ? "NULL" : p.Credit.ToString();

            string query = $@"
                    insert into dbo.tbl_Party 
                    (PartyNameId,PartyName, PartyTypeId, Debit,Credit)
                    values({p.PartyId},'{p.PartyName}', {p.PartyTypeId}, {dr}, {cr})";
            DataTable table = new();
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            try
            {
                using (SqlConnection myCon = new(sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new(query, myCon))
                    {
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader); ;

                        myReader.Close();
                        myCon.Close();
                    }
                }
                return new JsonResult(p);
            }
            catch (Exception ex)
            {
                var jsonResult = new JsonResult(ex.Message);
                jsonResult.StatusCode = ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return jsonResult;
            }


            
        }


        [HttpPut]
        public JsonResult Put(Party p)
        {
            string query = @"
                    update dbo.Party set 
                    PartyName = '" + p.PartyId + @"'
                    ,Department = '" + p.PartyName + @"'
                    ,DateOfJoining = '" + p.Debit + @"'
                    where PartyId = " + p.Credit + @" 
                    ";
            DataTable table = new();
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Updated Successfully");
        }


        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string query = @"
                    delete from dbo.tbl_Party
                    where PartyNameId = " + id + @" 
                    ";
            DataTable table = new();
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Deleted Successfully");
        }

    }
}
