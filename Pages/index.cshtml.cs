using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient


namespace tax1.Pages
{
    public class TaxCalculatorModel : PageModel
    {
        [BindProperty]
        public double Income { get; set; }

        [BindProperty]
        public string TaxRegime { get; set; }

        public double? TaxAmount { get; set; }

        // Property to store tax calculation records from the database
        public List<TaxRecord> TaxRecords { get; set; } = new List<TaxRecord>();

        private readonly IConfiguration _configuration;

        // Constructor to inject IConfiguration for database connection string
        public TaxCalculatorModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // OnPost method to calculate tax and save data to the database
        public void OnPost()
        {
            if (Income > 0 && !string.IsNullOrEmpty(TaxRegime))
            {
                // Calculate tax based on the selected tax regime
                TaxAmount = TaxRegime.ToLower() == "new" ? CalculateTaxNewRegime(Income) : CalculateTaxOldRegime(Income);

                // Save the calculated data to the database
                SaveTaxData(Income, TaxAmount.Value, TaxRegime);
            }
        }

        // Method to calculate tax under the Old Tax Regime
        private double CalculateTaxOldRegime(double income)
        {
            double tax = 0;

            if (income <= 250000)
            {
                tax = 0;
            }
            else if (income <= 500000)
            {
                tax = (income - 250000) * 0.05;
            }
            else if (income <= 1000000)
            {
                tax = (250000 * 0.05) + ((income - 500000) * 0.20);
            }
            else
            {
                tax = (250000 * 0.05) + (500000 * 0.20) + ((income - 1000000) * 0.30);
            }

            return tax + (tax * 0.04); // Adding 4% Cess
        }

        // Method to calculate tax under the New Tax Regime
        private double CalculateTaxNewRegime(double income)
        {
            double tax = 0;

            if (income <= 250000)
            {
                tax = 0;
            }
            else if (income <= 500000)
            {
                tax = (income - 250000) * 0.05;
            }
            else if (income <= 750000)
            {
                tax = (250000 * 0.05) + ((income - 500000) * 0.10);
            }
            else
            {
                tax = (250000 * 0.05) + (250000 * 0.10) + ((income - 750000) * 0.15);
            }

            return tax + (tax * 0.04); // Adding 4% Cess
        }

        // Method to save the user's input, calculated tax, and date to the database
        private void SaveTaxData(double userInput, double taxAmount, string taxRegime)
        {
            // Retrieve the connection string from appsettings.json
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Create and open a connection to the database
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    // SQL query to insert the data into the Taxes table
                    string query = @"
                        INSERT INTO Taxes (UserInput, TaxAmount, DateCalculated, TaxRegime)
                        VALUES (@UserInput, @TaxAmount, DATEADD(HOUR, 5, DATEADD(MINUTE, 30, GETUTCDATE())), @TaxRegime)";
                    
                    // Prepare the command with the SQL query and connection
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserInput", userInput);
                    cmd.Parameters.AddWithValue("@TaxAmount", taxAmount);
                    cmd.Parameters.AddWithValue("@TaxRegime", taxRegime);

                    // Open the connection and execute the query
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // Log or handle the exception as needed
                    throw new Exception("An error occurred while saving tax data to the database.", ex);
                }
            }
        }

        // New method to fetch and display all tax records from the database
        public void OnGetResults()
        {
            // Retrieve the connection string from appsettings.json
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    // SQL query to fetch all tax records
                    string query = "SELECT Id, UserInput, TaxAmount, DateCalculated, TaxRegime FROM Taxes ORDER BY DateCalculated DESC";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var taxRecord = new TaxRecord
                            {
                                Id = reader.GetInt32(0),
                                UserInput = reader.GetDouble(1),
                                TaxAmount = reader.GetDouble(2),
                                DateCalculated = reader.GetDateTime(3),
                                TaxRegime = reader.GetString(4)
                            };

                            TaxRecords.Add(taxRecord);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Log or handle the exception as needed
                    throw new Exception("An error occurred while fetching tax data from the database.", ex);
                }
            }
        }

        // A model class to represent the tax record from the database
        public class TaxRecord
        {
            public int Id { get; set; }
            public double UserInput { get; set; }
            public double TaxAmount { get; set; }
            public DateTime DateCalculated { get; set; }
            public string TaxRegime { get; set; }
        }
    }
}
