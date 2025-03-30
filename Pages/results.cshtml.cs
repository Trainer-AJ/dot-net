using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace tax1.Pages
{
    public class ResultsModel : PageModel
    {
        public List<TaxCalculatorModel.TaxRecord> TaxRecords { get; set; } = new List<TaxCalculatorModel.TaxRecord>();

        private readonly IConfiguration _configuration;

        public ResultsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT Id, UserInput, TaxAmount, DateCalculated, TaxRegime FROM Taxes ORDER BY DateCalculated DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TaxRecords.Add(new TaxCalculatorModel.TaxRecord
                            {
                                Id = reader.GetInt32(0),
                                UserInput = (double)reader.GetDecimal(1), // Cast Decimal to Double
                                TaxAmount = (double)reader.GetDecimal(2), // Cast Decimal to Double
                                DateCalculated = reader.GetDateTime(3),
                                TaxRegime = reader.GetString(4)
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception("An error occurred while fetching tax data from the database.", ex);
                }
            }
        }
    }
}
