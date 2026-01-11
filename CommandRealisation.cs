
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JPRUS_Dictionary
{
    public class CommandRealisation
    {
        LangIdentify lang = new LangIdentify();
        public string connectionString = @"Data Source=(localdb)\mssqllocaldb;
                Integrated Security=SSPI;
                Initial Catalog=JP_RU_Dict_Alphabet_Order;
                Timeout=30;
                TrustServerCertificate=True;";
        private string[] FirstLine = ["а", "и", "у", "э", "о", "я", "ю", "ё"];
        private string[] SecondLine = [
              "ка", "ки", "ку", "кэ", "ко",
    "са", "си", "су", "сэ", "со",
    "та", "ти", "цу", "тэ", "то",
    "на", "ни", "ну", "нэ", "но",
    "ха", "хи", "фу", "хэ", "хо",
    "ма", "ми", "му", "мэ", "мо",
    "ра", "ри", "ру", "рэ", "ро",
    "ва", "ву", "вэ", "во", "йо",
    "га", "ги", "гу", "гэ", "го",
    "да", "дэ", "до",
    "ба", "би", "бу", "бэ", "бо",
    "па", "пи", "пу", "пэ", "по"
          ];
        private string[] ThirdLine = ["дза", "дзи", "дзу", "дзэ", "дзо", "н"];
        public string Answer(string search)
        {
            string FinaleResult = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    conn.Open();

                    string query = "SELECT TOP 10 * FROM а";

                    return FinaleResult;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string Russian(string connectionString, string search)
        {



            string searchRefined = search.ToLower().Trim();

            string FinaleResult = "";
            string Isearch = searchRefined.Substring(0, 1);
            string WhereToSearch = "";
            for (int i = 0; i < FirstLine.Length; i++)
            {
                if (Isearch == FirstLine[i])
                {

                    WhereToSearch = FirstLine[i];
                    break;
                }
            }
            if (WhereToSearch == "")
            {
                Isearch = searchRefined.Substring(0, 2);
                for (int i = 0; i < SecondLine.Length; i++)
                {
                    if (Isearch == SecondLine[i])
                    {
                        WhereToSearch = SecondLine[i];
                        break;
                    }

                }
                if(WhereToSearch == "")
                {
                    Isearch = searchRefined.Substring(0, 3);
                    for (int i=0; i < ThirdLine.Length; i++)
                    {
                        if(Isearch == ThirdLine[i])
                        {
                            WhereToSearch = ThirdLine[i];
                            break;
                        }
                    }
                    if (WhereToSearch == "")
                    {
                        Isearch = searchRefined.Substring(0, 1);
                        if (Isearch == "н")
                            WhereToSearch = Isearch;
                    }
                }
            }
            if (WhereToSearch == "")
            {
                FinaleResult = "Ничего не найдено";
                return FinaleResult;
            }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {

                        conn.Open();

                        string query = "SELECT TOP 10 * FROM а";

                        return FinaleResult;
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
    }

