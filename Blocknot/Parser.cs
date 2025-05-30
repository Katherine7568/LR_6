using System;
using System.Collections.Generic;
using System.Linq;

namespace TextCompiler
{
    public class SyntaxParser
    {
        public class SyntaxError
        {
            public string Message { get; set; }
            public int Position { get; set; }
            public int Line { get; set; }
        }

        private static readonly HashSet<string> validTypes = new HashSet<string>
        {
            "String", "u64", "i32", "f64", "bool"
        };

        private static readonly HashSet<char> validIdentChars =
            new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_");

        public List<SyntaxError> Parse(string input)
        {
            var errors = new List<SyntaxError>();
            if (string.IsNullOrEmpty(input)) return errors;

            int pos = 0;
            int line = 1;

            // 1. Проверяем ключевое слово 'struct'
            ExpectKeyword("struct", ref pos, ref line, input, errors);

            // 2. Проверяем пробел после 'struct'
            ExpectWhitespace(ref pos, ref line, input, errors);

            // 3. Проверяем имя структуры
            ExpectIdentifier(ref pos, ref line, input, errors, "имя структуры");

            // 4. Проверяем открывающую скобку '{'
            ExpectChar('{', ref pos, ref line, input, errors);

            // 5. Проверяем поля структуры
            while (pos < input.Length && input[pos] != '}')
            {
                SkipWhitespace(ref pos, ref line, input);
                if (pos >= input.Length || input[pos] == '}') break;

                // Запоминаем начало поля для восстановления
                int fieldStart = pos;
                int fieldLine = line;

                // 5.1. Имя поля
                ExpectIdentifier(ref pos, ref line, input, errors, "имя поля");

                // 5.2. Двоеточие
                if (pos < input.Length) ExpectChar(':', ref pos, ref line, input, errors);

                // 5.3. Тип поля
                if (pos < input.Length) ExpectType(ref pos, ref line, input, errors);

                // Пропускаем пробелы перед проверкой разделителя
                SkipWhitespace(ref pos, ref line, input);

                // Проверяем разделитель
                if (pos < input.Length && input[pos] != ',' && input[pos] != '}')
                {
                    AddError($"Ожидается ',' или '}}'", pos, line,
                            pos < input.Length ? input[pos].ToString() : "EOF", errors);

                    // Пытаемся найти следующий разделитель
                    while (pos < input.Length && input[pos] != ',' && input[pos] != '}')
                    {
                        if (input[pos] == '\n') line++;
                        pos++;
                    }
                }

                if (pos < input.Length && input[pos] == ',')
                {
                    pos++; // Переходим к следующему полю
                }
            }

            // 6. Проверяем закрывающую скобку '}'
            ExpectChar('}', ref pos, ref line, input, errors);

            // 7. Проверяем точку с запятой ';'
            ExpectChar(';', ref pos, ref line, input, errors);

            return errors;
        }

        // Методы проверки теперь только добавляют ошибки и продвигают позицию
        private void ExpectKeyword(string keyword, ref int pos, ref int line,
                                 string input, List<SyntaxError> errors)
        {
            SkipWhitespace(ref pos, ref line, input);

            int start = pos;
            while (pos < input.Length && char.IsLetter(input[pos]))
                pos++;

            string word = pos <= input.Length ? input.Substring(start, pos - start) : "";
            if (word != keyword)
            {
                AddError($"Ожидается ключевое слово '{keyword}'", start, line, word, errors);
            }
        }

        private void ExpectWhitespace(ref int pos, ref int line,
                                    string input, List<SyntaxError> errors)
        {
            if (pos >= input.Length || !char.IsWhiteSpace(input[pos]))
            {
                AddError("Ожидается пробел", pos, line,
                        pos < input.Length ? input[pos].ToString() : "EOF", errors);
            }
            else
            {
                pos++;
            }
        }

        private void ExpectIdentifier(ref int pos, ref int line,
                                   string input, List<SyntaxError> errors,
                                   string expected)
        {
            SkipWhitespace(ref pos, ref line, input);

            int start = pos;
            while (pos < input.Length && validIdentChars.Contains(input[pos]))
                pos++;

            if (start == pos)
            {
                AddError($"Ожидается {expected}", pos, line,
                        pos < input.Length ? input[pos].ToString() : "EOF", errors);
            }
            else if (pos < input.Length && !char.IsWhiteSpace(input[pos]) &&
                     input[pos] != ':' && input[pos] != '{' && input[pos] != '}')
            {
                AddError("Недопустимый символ в идентификаторе", pos, line,
                        input[pos].ToString(), errors);
            }
        }

        private void ExpectType(ref int pos, ref int line,
                              string input, List<SyntaxError> errors)
        {
            SkipWhitespace(ref pos, ref line, input);

            int start = pos;
            while (pos < input.Length && char.IsLetterOrDigit(input[pos]))
                pos++;

            string type = pos <= input.Length ? input.Substring(start, pos - start) : "";
            if (!validTypes.Contains(type))
            {
                AddError("Ожидается тип поля (String, u64, i32, f64, bool)",
                        start, line, type, errors);
            }
        }

        private void ExpectChar(char expected, ref int pos, ref int line,
                              string input, List<SyntaxError> errors)
        {
            SkipWhitespace(ref pos, ref line, input);

            if (pos >= input.Length || input[pos] != expected)
            {
                AddError($"Ожидается символ '{expected}'", pos, line,
                        pos < input.Length ? input[pos].ToString() : "EOF", errors);
            }
            else
            {
                pos++;
            }
        }

        private void SkipWhitespace(ref int pos, ref int line, string input)
        {
            while (pos < input.Length)
            {
                if (input[pos] == '\n')
                {
                    line++;
                    pos++;
                }
                else if (char.IsWhiteSpace(input[pos]))
                {
                    pos++;
                }
                else
                {
                    break;
                }
            }
        }

        private void RecoverTo(ref int pos, ref int line, string input, char[] stopChars)
        {
            while (pos < input.Length && !stopChars.Contains(input[pos]))
            {
                if (input[pos] == '\n') line++;
                pos++;
            }
        }

        private void RecoverToEnd(ref int pos, ref int line, string input)
        {
            pos = input.Length;
        }

        private void AddError(string message, int position, int line,
                            string found, List<SyntaxError> errors)
        {
            errors.Add(new SyntaxError
            {
                Message = message,
                Position = position,
                Line = line,
            });
        }
    }
}