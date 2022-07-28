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
        readonly SqlHelper h;

        public FinController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
            h = new(_configuration);
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

            /*----Data------------------------------------------------------------------------------------------------------------------------*/
            DataTable table = new();
            table.TableName = "data";
            table.Load(await h.GetReaderBySQL(query));
            /*End Data------------------------------------------------------------------------------------------------------------------------*/

            /*----Opening---------------------------------------------------------------------------------------------------------------------*/
            query = $"SELECT ISNULL(Sum(Bal),0) Bal FROM acstat WHERE PartyID={parameters.PartyId} AND Date<'{parameters.SDate.ToString("yyyy-MM-dd")}'";
            DataTable opening = new();
            opening.TableName = "opening";
            opening.Columns.Add("opbal", typeof(Int32));
            Int32 opVal = await h.GetExecuteScalarByStr(query);
            opening.Rows.Add(opVal);
            /*End Opening---------------------------------------------------------------------------------------------------------------------*/

            DataSet ds = new();
            ds.Tables.AddRange(new DataTable[] { opening, table });

            return new JsonResult(ds);
        }

        [HttpGet("trial")]
        public JsonResult Trial([FromQuery] QueryParameters parameters)
        {
            try
            {

                SqlParameter sDate = new SqlParameter("sDate", SqlDbType.SmallDateTime);
                sDate.Value = parameters.SDate;
                SqlParameter eDate = new SqlParameter("eDate", SqlDbType.SmallDateTime);
                eDate.Value = parameters.EDate;
                SqlParameter isTrans = new SqlParameter("isTrans", SqlDbType.Bit);
                isTrans.Value = parameters.isTrans;

                DataSet ds = h.GetDatasetByCommand("Trial", new SqlParameter[] { sDate, eDate, isTrans });
                ds.Tables[0].TableName = "L1";
                ds.Tables[1].TableName = "L2";
                ds.Tables[2].TableName = "L3";
                ds.Tables[3].TableName = "L4";
                ds.Tables[4].TableName = "L5";
                return new JsonResult(ds);
            }
            catch (Exception ex)
            {

                return new JsonResult(ex.Message);
            }


        }

    }
}
