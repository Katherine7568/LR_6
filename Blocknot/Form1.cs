using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Blocknot
{
    public partial class Form1 : Form
    {
        private string currentFilePath;
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private string lastSavedText = ""; // Для отслеживания последнего сохраненного текста
        private bool isTextChanged = false; // Флаг для отслеживания изменений в тексте

        public Form1()
        {
            InitializeComponent();
        }
            

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripButton6.Click += toolStripButton6_Click;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Проверяем, есть ли несохраненные изменения
            if (isTextChanged)
            {
                var result = MessageBox.Show("Вы хотите сохранить изменения в текущем файле?", "Сохранить изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveFile(); // Сохраняем текущий файл
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // Отменяем открытие нового файла
                }
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox.Text = File.ReadAllText(openFileDialog.FileName);
                    currentFilePath = openFileDialog.FileName; // Сохраняем путь к открытому файлу
                    lastSavedText = textBox.Text; // Сохраняем текст
                    isTextChanged = false; // Сбрасываем флаг изменений
                    undoStack.Clear(); // Очищаем стек отмены
                    redoStack.Clear(); // Очищаем стек повтора
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Проверяем, есть ли несохраненные изменения
            if (isTextChanged)
            {
                var result = MessageBox.Show("Вы хотите сохранить изменения в текущем файле?", "Сохранить изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveFile(); // Сохраняем текущий файл
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // Отменяем закрытие формы
                }
            }

            base.OnFormClosing(e); // Вызываем базовый метод
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileAs(); // Если файл не был сохранен, используем "Сохранить как"
            }
            else
            {
                File.WriteAllText(currentFilePath, textBox.Text); // Сохраняем изменения в существующий файл
                lastSavedText = textBox.Text; // Обновляем последнее сохраненное состояние
                isTextChanged = false; // Сбрасываем флаг изменений
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileAs(); // Всегда открываем диалог "Сохранить как"
        }

        private void SaveFileAs()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, textBox.Text);
                    currentFilePath = saveFileDialog.FileName; // Сохраняем путь к новому файлу
                    lastSavedText = textBox.Text; // Сохраняем текст
                    isTextChanged = false; // Сбрасываем флаг изменений
                    undoStack.Clear(); // Очищаем стек отмены
                    redoStack.Clear(); // Очищаем стек повтора
                }
            }
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Проверяем, есть ли несохраненные изменения
            if (isTextChanged)
            {
                var result = MessageBox.Show("Вы хотите сохранить изменения в текущем файле?", "Сохранить изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveFile(); // Сохраняем текущий файл
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // Отменяем создание нового файла
                }
            }

            // Очищаем текстовое поле и сбрасываем путь к файлу
            textBox.Clear();
            currentFilePath = string.Empty; // Сбрасываем путь к файлу
            lastSavedText = ""; // Сбрасываем последнее сохраненное состояние
            isTextChanged = false; // Сбрасываем флаг изменений
            undoStack.Clear(); // Очищаем стек отмены
            redoStack.Clear(); // Очищаем стек повтора
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) // Кнопка "Отмена"
        {
            Undo();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e) // Кнопка "Повтор"
        {
            Redo();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, была ли нажата клавиша пробела или Enter
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                // Сохраняем текущее состояние текста в стек отмены
                undoStack.Push(textBox.Text);
                redoStack.Clear(); // Очищаем стек повтора при новом изменении
            }
        }

        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                // Сохраняем текущее состояние текста в стек повтора
                redoStack.Push(textBox.Text);
                // Восстанавливаем предыдущее состояние текста
                textBox.Text = undoStack.Pop();
                textBox.SelectionStart = textBox.Text.Length; // Устанавливаем курсор в конец текста
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                // Сохраняем текущее состояние текста в стек отмены
                undoStack.Push(textBox.Text);
                // Восстанавливаем состояние текста из стека повтора
                textBox.Text = redoStack.Pop();
                textBox.SelectionStart = textBox.Text.Length; // Устанавливаем курсор в конец текста
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText != "")
            {
                Clipboard.SetText(textBox.SelectedText);
                textBox.SelectedText = "";
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText != "")
            {
                Clipboard.SetText(textBox.SelectedText);
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                textBox.Paste();
            }
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText != "")
            {
                textBox.SelectedText = "";
                isTextChanged = true; // Устанавливаем флаг изменений
            }
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            textBox.SelectAll();
        }

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string helpFilePath = "C:\\Users\\xxxma\\OneDrive\\Рабочий стол\\НЯМ НЯМ\\Компиляторы\\РГЗ\\help2.html"; // Убедитесь, что файл находится в корне проекта или укажите полный путь
            System.Diagnostics.Process.Start(helpFilePath);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Текстовый редактор на C#\nВерсия 1.0\n\n" +
                    "Автор: Кляйншмидт Максим\n" +
                    "Описание: Это простой текстовый редактор с базовыми функциями редактирования текста.",
                    "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            Redo();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            isTextChanged = true;
        }
        private void AnalyzeButtonClick(object sender, EventArgs e)
        {
            //        (Регулярные выражения для поиска цитат)
            string inputText = textBox.Text;
            string pattern = "[«\"](.*?)[»\"]"; // фиксированный паттерн для поиска цитат

            try
            {
                List<RegexSearcher.MatchInfo> matches = RegexSearcher.FindMatches(inputText, pattern);

                if (matches.Count == 0)
                {
                    resultTextBox.Text = "Цитаты не найдены.";
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Найденные цитаты:");

                foreach (var match in matches)
                {
                    sb.AppendLine(match.ToString());
                }

                resultTextBox.Text = sb.ToString();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка в регулярном выражении:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                resultTextBox.Text = string.Empty;
            }
        }     

            /*          (Регулярные выражения для проверки слов, содержащих подстроку "кофе" и ровно 10 символов)
            string inputText = textBox.Text;

            // Регулярное выражение для поиска слов с "кофе" длиной ровно 10 символов
            string pattern = @"\b(?=\w{10}\b)\w*кофе\w*\b";

            try
            {
                List<RegexSearcher.MatchInfo> matches = RegexSearcher.FindMatches(inputText, pattern);

                if (matches.Count == 0)
                {
                    resultTextBox.Text = "Подходящих слов не найдено.";
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Найденные слова:");

                foreach (var match in matches)
                {
                    sb.AppendLine(match.ToString());
                }

                resultTextBox.Text = sb.ToString();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка в регулярном выражении:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                resultTextBox.Text = string.Empty;
            }
            */

        
            /*string inputText = textBox.Text;

            // Регулярное выражение для поиска времени в формате ЧЧ:ММ:СС с ведущими нулями
            string pattern = @"\b(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d\b";

            try
            {
                List<RegexSearcher.MatchInfo> matches = RegexSearcher.FindMatches(inputText, pattern);

                if (matches.Count == 0)
                {
                    resultTextBox.Text = "Время в заданном формате не найдено.";
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Найденное время:");

                foreach (var match in matches)
                {
                    sb.AppendLine(match.ToString());
                }

                resultTextBox.Text = sb.ToString();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка в регулярном выражении:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                resultTextBox.Text = string.Empty;
            }
          */ 


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // Проверяем, есть ли несохраненные изменения
            if (isTextChanged)
            {
                var result = MessageBox.Show("Вы хотите сохранить изменения в текущем файле?", "Сохранить изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveFile(); // Сохраняем текущий файл
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // Отменяем создание нового файла
                }
            }

            // Очищаем текстовое поле и сбрасываем путь к файлу
            textBox.Clear();
            currentFilePath = string.Empty; // Сбрасываем путь к файлу
            lastSavedText = ""; // Сбрасываем последнее сохраненное состояние
            isTextChanged = false; // Сбрасываем флаг изменений
            undoStack.Clear(); // Очищаем стек отмены
            redoStack.Clear(); // Очищаем стек повтора

        }
       
       

        private void toolStripButton5_Click_1(object sender, EventArgs e)
        {
            Redo();
        }

        private void Сохранить_Click(object sender, EventArgs e)
        {
            // Проверяем, есть ли несохраненные изменения
            if (isTextChanged)
            {
                var result = MessageBox.Show("Вы хотите сохранить изменения в текущем файле?", "Сохранить изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveFile(); // Сохраняем текущий файл
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // Отменяем открытие нового файла
                }
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox.Text = File.ReadAllText(openFileDialog.FileName);
                    currentFilePath = openFileDialog.FileName; // Сохраняем путь к открытому файлу
                    lastSavedText = textBox.Text; // Сохраняем текст
                    isTextChanged = false; // Сбрасываем флаг изменений
                    undoStack.Clear(); // Очищаем стек отмены
                    redoStack.Clear(); // Очищаем стек повтора
                }
            }
        }

        private void Сохранить_как_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void Отменить_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void Копировать_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText != "")
            {
                Clipboard.SetText(textBox.SelectedText);
            }
        }

        private void Вырезать_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText != "")
            {
                Clipboard.SetText(textBox.SelectedText);
                textBox.SelectedText = "";
            }
        }

        private void Вставить_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                textBox.Paste();
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            SaveFileAs();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            textBox.SelectAll();
            if (textBox.SelectedText != "")
            {
                textBox.SelectedText = "";
                isTextChanged = true; // Устанавливаем флаг изменений
            }
        }

        private void resultTextBox_TextChanged(object sender, EventArgs e)
        {

        }
        private void пToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnalyzeButtonClick(sender, e);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            string inputText = textBox.Text.Trim();
            resultTextBox.Text = "";

            try
            {
                var parser = new RecursiveDescentParser(inputText);
                parser.ParseE();

                if (parser.CurrentToken != null)
                {
                    throw new Exception($"Ошибка: неожиданный символ '{parser.CurrentToken}' на позиции {parser.GetPosition()}");
                }

                resultTextBox.Text = "Разбор выполнен успешно.\nПоследовательность вызовов:\n" +
                                    parser.CallSequence;
            }
            catch (Exception ex)
            {
                resultTextBox.Text = $"Ошибка: {ex.Message}";
            }
        }

        class RecursiveDescentParser
        {
            private readonly string input;
            private int position;
            public string CallSequence { get; private set; } = "";

            public RecursiveDescentParser(string input)
            {
                this.input = input;
                this.position = 0;
                SkipWhitespace();
            }

            public char? CurrentToken => position < input.Length ? input[position] : (char?)null;
            public int GetPosition() => position + 1;

            private void SkipWhitespace()
            {
                while (position < input.Length && char.IsWhiteSpace(input[position]))
                {
                    position++;
                }
            }

            private void AddToSequence(string element)
            {
                if (CallSequence.Length > 0)
                    CallSequence += "-";
                CallSequence += element;
            }

            private void Match(char expected)
            {
                if (CurrentToken == expected)
                {
                    position++;
                    SkipWhitespace();
                }
                else
                {
                    throw new Exception($"Ожидался символ '{expected}', но получен '{CurrentToken}'");
                }
            }

            private void Match(string expected)
            {
                foreach (char c in expected)
                {
                    if (CurrentToken != c)
                    {
                        throw new Exception($"Ожидалась строка '{expected}', но получен '{CurrentToken}'");
                    }
                    position++;
                }
                SkipWhitespace();
            }

            public void ParseE()
            {
                AddToSequence("E");
                ParseE1();

                if (CurrentToken == '=' || CurrentToken == '<' || CurrentToken == '>')
                {
                    char op = CurrentToken.Value;
                    AddToSequence(op.ToString());
                    Match(op);
                    ParseE1();
                }
            }

            private void ParseE1()
            {
                AddToSequence("E1");
                ParseT();

                while (CurrentToken == '+' || CurrentToken == '-' ||
                       (CurrentToken == 'o' && LookAhead("or")))
                {
                    if (CurrentToken == 'o')
                    {
                        AddToSequence("or");
                        Match("or");
                    }
                    else
                    {
                        char op = CurrentToken.Value;
                        AddToSequence(op.ToString());
                        Match(op);
                    }

                    ParseT();
                }
            }

            private void ParseT()
            {
                AddToSequence("T");
                ParseF();

                while (CurrentToken == '*' || CurrentToken == '/' ||
                       (CurrentToken == 'a' && LookAhead("and")))
                {
                    if (CurrentToken == 'a')
                    {
                        AddToSequence("and");
                        Match("and");
                    }
                    else
                    {
                        char op = CurrentToken.Value;
                        AddToSequence(op.ToString());
                        Match(op);
                    }

                    ParseF();
                }
            }

            private void ParseF()
            {
                AddToSequence("F");

                if (CurrentToken == 't' && LookAhead("true"))
                {
                    AddToSequence("true");
                    Match("true");
                }
                else if (CurrentToken == 'f' && LookAhead("false"))
                {
                    AddToSequence("false");
                    Match("false");
                }
                else if (CurrentToken == 'n' && LookAhead("not"))
                {
                    AddToSequence("not");
                    Match("not");
                    ParseF();
                }
                else if (CurrentToken == '(')
                {
                    AddToSequence("(");
                    Match('(');
                    ParseE();
                    AddToSequence(")");
                    Match(')');
                }
                else
                {
                    throw new Exception($"Ожидалось 'true', 'false', 'not' или выражение в скобках, но получен '{CurrentToken}'");
                }
            }

            private bool LookAhead(string expected)
            {
                if (position + expected.Length > input.Length)
                    return false;

                for (int i = 0; i < expected.Length; i++)
                {
                    if (input[position + i] != expected[i])
                        return false;
                }
                return true;
            }
        }


        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            string inputText = textBox.Text;

            // Строгое регулярное выражение для имен файлов:
            // 1. Начинается с буквы или цифры
            // 2. Может содержать буквы, цифры, пробелы, подчеркивания, точки, дефисы
            // 3. Заканчивается точкой и расширением (3-4 буквы)
            string pattern = @"(?!\.)\b[\w\s\-\.]+?\.(?:txt|jpg|jpeg|pdf|doc|docx|png|gif|dat)\b";

            try
            {
                List<RegexSearcher.MatchInfo> matches = RegexSearcher.FindMatches(inputText, pattern);

                // Дополнительная фильтрация
                var validFiles = new List<RegexSearcher.MatchInfo>();
                foreach (var match in matches)
                {
                    string fileName = match.Value;

                    // Проверяем, что имя файла не содержит запрещенных символов
                    if (!fileName.Any(c => Path.GetInvalidFileNameChars().Contains(c)) &&
                        !fileName.StartsWith(".") && // исключаем скрытые файлы
                        fileName.Count(c => c == '.') == 1) // ровно одна точка
                    {
                        validFiles.Add(match);
                    }
                }

                if (validFiles.Count == 0)
                {
                    resultTextBox.Text = "Корректные названия файлов не найдены.";
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Найденные корректные названия файлов:");

                foreach (var match in validFiles)
                {
                    sb.AppendLine($"{match.Value} (позиция: {match.Position + 1})");
                }

                resultTextBox.Text = sb.ToString();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка в регулярном выражении:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                resultTextBox.Text = string.Empty;
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            string inputText = textBox.Text;
            string pattern = @"\b(25[0-5]|2[0-4]\d|[01]?\d\d?)\.(25[0-5]|2[0-4]\d|[01]?\d\d?)\.(25[0-5]|2[0-4]\d|[01]?\d\d?)\.(25[0-5]|2[0-4]\d|[01]?\d\d?):(6553[0-5]|655[0-2]\d|65[0-4]\d{2}|6[0-4]\d{3}|[1-5]\d{4}|[1-9]\d{0,3}|0)\b";

            try
            {
                List<RegexSearcher.MatchInfo> matches = RegexSearcher.FindMatches(inputText, pattern);

                if (matches.Count == 0)
                {
                    resultTextBox.Text = "IP-адреса с портами не найдены.";
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Найденные IP-адреса с портами:");

                foreach (var match in matches)
                {
                    sb.AppendLine($"\"{match.Value}\" at position {match.Position} (length {match.Length})");
                }

                resultTextBox.Text = sb.ToString();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ошибка в регулярном выражении:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                resultTextBox.Text = string.Empty;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            string inputText = textBox.Text;
            var matches = IpPortParser.FindAll(inputText);

            if (matches.Count == 0)
            {
                resultTextBox.Text = "IP-адреса с портами не найдены.";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Найденные IP-адреса с портами:");

            foreach (var m in matches)
                sb.AppendLine($"\"{m.Value}\" at position {m.Index} (length {m.Length})");

            resultTextBox.Text = sb.ToString();
        }

    }
}