using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace tax1.Pages
{
    public class TaxCalculatorModel : PageModel
    {
        [BindProperty]
        public double Income { get; set; }
        
        [BindProperty]
        public string TaxRegime { get; set; }
        
        public double? TaxAmount { get; set; }

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
    }
}
