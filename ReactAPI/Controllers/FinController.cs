using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public FinController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }
        [HttpGet("test")]
        public JsonResult Test()
        {
            DataTable table1 = new();
            table1.Columns.Add("a", typeof(int));
            table1.Columns.Add("b", typeof(string));

            table1.Rows.Add(1,"abc");

            DataTable table2 = new();
            table2.TableName = "t";
            table2.Columns.Add("a", typeof(int));
            table2.Columns.Add("b", typeof(string));

            table2.Rows.Add(1, "abc");

            DataSet ds = new();
            ds.Tables.AddRange(new DataTable[] { table1, table2 });
            return new JsonResult(ds);

        }

        public class QueryParameters
        {
            public int PartyId { get; set; }
            public DateTime SDate { get; set; }
            public DateTime EDate { get; set; }
        }

        [HttpGet("acstat")]
        public async Task<JsonResult> AcStat([FromQuery] QueryParameters parameters)
        {
            string query = @$"
                    SELECT VocNo,SrNo ,Date ,PartyID ,TType ,Description ,NetCredit ,NetDebit ,BAL ,PartyRef ,pVocNo FROM AcStat
                    WHERE PartyID={parameters.PartyId} AND (Date Between '{parameters.SDate.ToString("yyyy-MM-dd")}' AND '{parameters.EDate.ToString("yyyy-MM-dd")}')
                    ORDER BY Date
                    ";
            DataTable table = new();
            table.TableName= "data";
            string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new(query, myCon))
                {
                    myReader = await myCommand.ExecuteReaderAsync();
                    table.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }
            DataTable opening= new();
            opening.TableName = "opening";
            opening.Columns.Add("opening", typeof(Int32));

            SqlHelper h = new(_configuration);
            Int32 opVal = await h.GetExecuteScalarByStr($"SELECT ISNULL(Sum(Bal),0) Bal FROM acstat WHERE PartyID={parameters.PartyId} AND Date<'{parameters.SDate.ToString("yyyy-MM-dd")}'");
            opening.Rows.Add(opVal);

            DataSet ds= new();
            ds.Tables.AddRange(new DataTable[] {opening, table});

            return new JsonResult(ds);
        }
    }
}
