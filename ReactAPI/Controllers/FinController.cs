using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

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
            public int PartyId { get; set; }
            public DateTime SDate { get; set; }
            public DateTime EDate { get; set; }
        }

        [HttpGet("acstat")]
        public JsonResult AcStat([FromQuery] QueryParameters parameters)
        {
            string query = @$"
                    SELECT VocNo,SrNo ,Date ,PartyID ,TType ,Description ,NetCredit ,NetDebit ,BAL ,PartyRef ,pVocNo FROM AcStat
                    WHERE PartyID={parameters.PartyId} AND (Date Between '{parameters.SDate.ToString("yyyy-MM-dd")}' AND '{parameters.EDate.ToString("yyyy-MM-dd")}')
                    ORDER BY Date
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
}
