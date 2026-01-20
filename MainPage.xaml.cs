// MainPage.xaml.cs
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Linq.Expressions;


namespace JPRUS_Dictionary
{
    public partial class MainPage : ContentPage
    {

        LangIdentify lang = new LangIdentify();
        
        public MainPage()
        {
            InitializeComponent();
            

        }
         void OnButtonClicked(object sender, EventArgs args)
        {
            string connectionString = @"Data Source=(localdb)\mssqllocaldb;
                Integrated Security=SSPI;
                Initial Catalog=JP_RU_Dict_Alphabet_Order;
                Timeout=30;
                TrustServerCertificate=True;";
            CommandRealisation Cr = new CommandRealisation(connectionString);
            string search = SimpleSearch.Text;
            string res=Cr.Answer(search,connectionString);
            Result.Text = res;
            
            
        }
    }

}


//try
//{
//    using (SqlConnection conn = new SqlConnection(connectionString))
//    {
//        conn.Open();
//        Result.Text = "Подключение установлено";
//        CommandRealisation CR = new CommandRealisation(connectionString);
//        CR.Answer()

//        } 




