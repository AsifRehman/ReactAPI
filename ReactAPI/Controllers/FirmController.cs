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
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FirmController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public FirmController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }
    [Authorize]
    [HttpGet("firmlist")]
    public JsonResult FirmList()
    {
        string query = @"
                    select Id FirmId, [Name] as FirmName, 'asif.pro@gmail.com' Email, '0333993386' phone, 'st#3' Address from dbo.Firm";
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
                    select Id FirmId, [Name] as FirmName, 'asif.pro@gmail.com' Email, '0333993386' phone, 'st#3' Address from dbo.Firm WHERE id={id}";
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
                    select Id FirmId, [Name] as FirmName, 'asif.pro@gmail.com' Email, '0333993386' phone, 'st#3' Address from dbo.Firm WHERE id=1
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
}
