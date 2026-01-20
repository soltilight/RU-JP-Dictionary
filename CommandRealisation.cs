

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JPRUS_Dictionary
{
    public class CommandRealisation
    {//возвращает через int 1 если Японский,Альтернатива Русский,Другой язык-0
        LangIdentify lang = new LangIdentify();
        public string connetionString { get; set; }
        public CommandRealisation(string connectionString)
        {
            this.connetionString = connetionString;
        }
       
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
        public string Answer(string search,string connectionString)
        {
            connetionString = this.connetionString;
            string FinaleResult = "";
            int Is = lang.IdentifyJapanese(search);
            bool ISJP = (Is == 1);
            if (ISJP == true)
            {
                //feel me later
                FinaleResult = JapaneseWithRelevance(connectionString, search);
                return FinaleResult;
            }
            else {
                FinaleResult = Russian(connectionString, search);
                return FinaleResult;
            }
        }
            public string JapaneseWithRelevance(string connectionString, string search)
        {
            string searchRefined = search.Trim();

            if (string.IsNullOrWhiteSpace(searchRefined))
                return "Пустой поисковый запрос";

            StringBuilder finalResult = new StringBuilder();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Получаем список таблиц
                    List<string> tables = new List<string>();
                    string getTablesQuery = @"
                SELECT 
                    QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) AS FullTableName
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE COLUMN_NAME = N'Японский'
                AND TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";

                    using (SqlCommand cmdTables = new SqlCommand(getTablesQuery, conn))
                    using (SqlDataReader readerTables = cmdTables.ExecuteReader())
                    {
                        while (readerTables.Read())
                        {
                            tables.Add(readerTables["FullTableName"].ToString());
                        }
                    }

                    if (tables.Count == 0)
                        return "Таблицы с колонкой 'Японский' не найдены";

                    // Формируем запрос с вычислением релевантности
                    StringBuilder unionQuery = new StringBuilder();
                    for (int i = 0; i < tables.Count; i++)
                    {
                        unionQuery.AppendLine($@"
                    SELECT 
                        '{tables[i].Replace("'", "''")}' AS TableName,
                        Японский,
                        Русский_Произношение,
                        Русский_Значение,
                        -- Позиция начала совпадения (чем меньше, тем лучше)
                        CASE 
                            WHEN CHARINDEX(@Search, Японский) > 0 
                            THEN CHARINDEX(@Search, Японский)
                            ELSE 9999 
                        END AS MatchPosition,
                        -- Длина совпадения относительно длины строки
                        CASE 
                            WHEN CHARINDEX(@Search, Японский) > 0 
                            THEN CAST(LEN(@Search) AS FLOAT) / NULLIF(LEN(Японский), 0)
                            ELSE 0 
                        END AS MatchRatio
                    FROM {tables[i]}
                    WHERE Японский LIKE @SearchPattern");

                        if (i < tables.Count - 1)
                            unionQuery.AppendLine("UNION ALL");
                    }

                    // Добавляем сортировку и ограничение TOP 5
                    string query = $@"
                WITH AllResults AS (
                    {unionQuery}
                )
                SELECT TOP 5
                    TableName,
                    Японский,
                    Русский_Произношение,
                    Русский_Значение,
                    MatchPosition,
                    MatchRatio
                FROM AllResults
                ORDER BY 
                    CASE WHEN MatchPosition = 1 THEN 0 ELSE 1 END, -- Сначала те, где совпадение с начала
                    MatchPosition, -- Затем по позиции совпадения
                    MatchRatio DESC -- Затем по доле совпадения
            ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Search", searchRefined);
                        cmd.Parameters.AddWithValue("@SearchPattern", $"%{searchRefined}%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int count = 0;
                            StringBuilder resultText = new StringBuilder();

                            while (reader.Read())
                            {
                                count++;

                                string japanese = reader["Японский"]?.ToString() ?? "";
                                string pronunciation = reader["Русский_Произношение"]?.ToString() ?? "";
                                string russianMeaning = reader["Русский_Значение"]?.ToString() ?? "";
                                string tableName = reader["TableName"]?.ToString() ?? "";

                                resultText.AppendLine($"{count}. Таблица: {tableName}");
                                resultText.AppendLine($"   Японский: {japanese}");
                                resultText.AppendLine($"   Произношение: {pronunciation}");
                                resultText.AppendLine($"   Значение: {russianMeaning}");
                                resultText.AppendLine($"   -----------------");
                            }

                            if (count == 0)
                                return $"Совпадений для '{search}' не найдено";

                            //finalResult.AppendLine($"Найдено {count} наиболее релевантных совпадений:");
                            finalResult.AppendLine();
                            finalResult.Append(resultText);

                            if (count == 5)
                            {
                                //finalResult.AppendLine("\n(показано 5 наиболее релевантных результатов)");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }

            return finalResult.ToString();
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

                   string query = $@"
                   SELECT Японский, Русский_Значение, Русский_Произношение
                   FROM [{WhereToSearch}]
                   WHERE Русский_Произношение LIKE @searchPattern
                   OR Русский_Значение LIKE @searchPattern
                   ORDER BY Русский_Произношение";


                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@searchPattern", "%" + searchRefined + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            string resultText = "";
                            int count = 0;

                            while (reader.Read())
                            {
                                count++;
                                string japanese = reader["Японский"]?.ToString() ?? "";
                                string russianMeaning = reader["Русский_Значение"]?.ToString() ?? "";
                                string pronunciation = reader["Русский_Произношение"]?.ToString() ?? "";

                                resultText += $"{count}. Японский: {japanese}\n";
                                resultText += $"   Произношение: {pronunciation}\n";
                                resultText += $"   Значение: {russianMeaning}\n";
                                resultText += $"   -----------------\n";
                            }

                            if (count == 0)
                            {
                                return $"Совпадений для '{search}' не найдено";
                            }

                            FinaleResult = $"Найдено {count} совпадений:\n\n" + resultText;
                        }
                    }

                    return FinaleResult;
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }
        }
    }
