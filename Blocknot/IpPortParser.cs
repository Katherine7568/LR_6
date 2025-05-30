using System;
using System.Collections.Generic;

namespace Blocknot
{
    public static class IpPortParser
    {
        public struct MatchInfo
        {
            public string Value { get; }
            public int Index { get; }
            public int Length => Value.Length;

            public MatchInfo(string value, int index)
            {
                Value = value;
                Index = index;
            }
        }

        private enum State
        {
            Start,
            Octet1, Dot1,
            Octet2, Dot2,
            Octet3, Dot3,
            Octet4,
            Colon,
            Port,
            Fail
        }

        /// <summary>Находит все подстроки-«IP:порт» и возвращает позиции.</summary>
        public static List<MatchInfo> FindAll(string text)
        {
            var results = new List<MatchInfo>();
            State state = State.Start;

            int startIndex = -1;
            int octetValue = 0, portValue = 0;
            int digitCount = 0, dotCount = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                switch (state)
                {
                    case State.Start:
                        if (char.IsDigit(c))
                        {
                            state = State.Octet1;
                            startIndex = i;
                            dotCount = 0;
                            octetValue = c - '0';
                            digitCount = 1;
                        }
                        break;

                    case State.Octet1:
                    case State.Octet2:
                    case State.Octet3:
                        if (char.IsDigit(c))
                        {
                            octetValue = octetValue * 10 + (c - '0');
                            if (++digitCount > 3 || octetValue > 255)
                                state = State.Fail;
                        }
                        else if (c == '.')
                        {
                            if (octetValue > 255) { state = State.Fail; break; }

                            dotCount++;
                            digitCount = 0;
                            octetValue = 0;

                            if (dotCount == 1) state = State.Dot1;
                            else if (dotCount == 2) state = State.Dot2;
                            else if (dotCount == 3) state = State.Dot3;
                            else state = State.Fail;
                        }
                        else state = State.Fail;
                        break;

                    case State.Dot1:
                    case State.Dot2:
                    case State.Dot3:
                        if (char.IsDigit(c))
                        {
                            octetValue = c - '0';
                            digitCount = 1;
                            state = state == State.Dot1 ? State.Octet2
                                   : state == State.Dot2 ? State.Octet3
                                   : State.Octet4;
                        }
                        else state = State.Fail;
                        break;

                    case State.Octet4:
                        if (char.IsDigit(c))
                        {
                            octetValue = octetValue * 10 + (c - '0');
                            if (++digitCount > 3 || octetValue > 255)
                                state = State.Fail;
                        }
                        else if (c == ':')
                        {
                            state = State.Colon;
                            portValue = 0;
                            digitCount = 0;
                        }
                        else state = State.Fail;
                        break;

                    case State.Colon:
                        if (char.IsDigit(c))
                        {
                            portValue = c - '0';
                            digitCount = 1;
                            state = State.Port;
                        }
                        else state = State.Fail;
                        break;

                    case State.Port:
                        if (char.IsDigit(c))
                        {
                            portValue = portValue * 10 + (c - '0');
                            if (++digitCount > 5 || portValue > 65535)
                                state = State.Fail;
                        }
                        else // конец совпадения
                        {
                            if (digitCount > 0 && portValue <= 65535)
                                AddMatch(ref state, text, results, startIndex, i - 1);
                            else
                                state = State.Fail;
                        }
                        break;
                }

                // конец текста
                if (i == text.Length - 1 && state == State.Port && portValue <= 65535)
                    AddMatch(ref state, text, results, startIndex, i);

                // сброс после ошибки
                if (state == State.Fail)
                    state = State.Start;
            }

            return results;
        }

        private static void AddMatch(ref State state, string text, List<MatchInfo> results,
                                     int start, int end)
        {
            if (start >= 0 && end >= start)
                results.Add(new MatchInfo(text.Substring(start, end - start + 1), start));

            state = State.Start;
        }
    }
}
