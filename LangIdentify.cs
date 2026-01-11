using System.Text.RegularExpressions;

namespace JPRUS_Dictionary
{
    public class LangIdentify
    {
        public int IdentifyJapanese(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // Определение символов японского языка
            // Хирагана: U+3040 - U+309F
            // Катакана: U+30A0 - U+30FF
            // Кандзи (китайские иероглифы): U+4E00 - U+9FFF
            // Расширенные кандзи: U+3400 - U+4DBF
            bool hasJapanese = false;
            bool hasNonJapanese = false;

            foreach (char c in text)
            {
                if (IsJapaneseCharacter(c))
                {
                    hasJapanese = true;
                }
                else if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !char.IsDigit(c))
                {
                    // Проверяем, не является ли символ латиницей, кириллицей или другим алфавитом
                    if (IsLatinOrCyrillic(c))
                    {
                        hasNonJapanese = true;
                    }
                    else
                    {
                        // Для других символов (корейских, арабских и т.д.) тоже считаем как не-японские
                        hasNonJapanese = true;
                    }
                }
            }

            // Возвращаем 1 только если есть японские символы И нет других языковых символов
            return hasJapanese && !hasNonJapanese ? 1 : 0;
        }

        private bool IsJapaneseCharacter(char c)
        {
            // Проверка диапазонов Unicode для японских символов
            return (c >= '\u3040' && c <= '\u309F') || // Хирагана
                   (c >= '\u30A0' && c <= '\u30FF') || // Катакана
                   (c >= '\u4E00' && c <= '\u9FFF') || // Кандзи (общие иероглифы)
                   (c >= '\u3400' && c <= '\u4DBF');   // Расширенные иероглифы A
        }

        private bool IsLatinOrCyrillic(char c)
        {
            // Латинские буквы
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                return true;

            // Кириллические буквы
            if ((c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я'))
                return true;

            return false;
        }
    }
}