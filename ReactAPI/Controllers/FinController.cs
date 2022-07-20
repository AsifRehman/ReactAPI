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

        public class QueryParameters
        {
            public int? PartyId { get; set; } = null;
            public DateTime SDate { get; set; }
            public DateTime EDate { get; set; }
            public bool isTrans { get; set; } = true;
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
            table.TableName = "data";
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
            DataTable opening = new();
            opening.TableName = "opening";
            opening.Columns.Add("opbal", typeof(Int32));

            SqlHelper h = new(_configuration);
            Int32 opVal = await h.GetExecuteScalarByStr($"SELECT ISNULL(Sum(Bal),0) Bal FROM acstat WHERE PartyID={parameters.PartyId} AND Date<'{parameters.SDate.ToString("yyyy-MM-dd")}'");
            opening.Rows.Add(opVal);

            DataSet ds = new();
            ds.Tables.AddRange(new DataTable[] { opening, table });

            return new JsonResult(ds);
        }

        [HttpGet("trial")]
        public JsonResult Trial([FromQuery] QueryParameters parameters)
        {
            try
            {

                SqlHelper s = new(_configuration);
                SqlParameter sDate = new SqlParameter("sDate", SqlDbType.SmallDateTime);
                sDate.Value = parameters.SDate;
                SqlParameter eDate = new SqlParameter("eDate", SqlDbType.SmallDateTime);
                eDate.Value = parameters.EDate;
                SqlParameter isTrans = new SqlParameter("isTrans", SqlDbType.Bit);
                isTrans.Value = parameters.isTrans;

                DataSet ds = s.GetDatasetByCommand("Trial", new SqlParameter[] { sDate, eDate, isTrans });

                return new JsonResult(ds);
            }
            catch (Exception ex)
            {

                return new JsonResult(ex.Message);
            }


        }

    }
}
