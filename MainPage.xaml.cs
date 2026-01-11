// MainPage.xaml.cs
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Linq.Expressions;


namespace JPRUS_Dictionary
{
    public partial class MainPage : ContentPage
    {
        
        LangIdentify lang=new LangIdentify();
        public MainPage()
        {
            InitializeComponent();
            string connectionString = @"Data Source=(localdb)\mssqllocaldb;
                Integrated Security=SSPI;
                Initial Catalog=JP_RU_Dict_Alphabet_Order;
                Timeout=30;
                TrustServerCertificate=True;";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    Result.Text = "Подключение установлено";
                    string query = "SELECT TOP 10 * FROM а";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            string resultText = "";
                            while (reader.Read())
                            {
                                // Обработка каждой строки
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    resultText += reader[i]?.ToString() + "\t";
                                }
                                resultText += "\n";
                            }
                            Result.Text = resultText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Text = ex.Message;
            }

        }
            }
         

        }
    
