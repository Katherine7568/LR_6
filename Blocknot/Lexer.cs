using System.Collections.Generic;
using System;

public class Lexer
{
    private readonly string _input;
    private int _pos;

    public Lexer(string input)
    {
        _input = input;
        _pos = 0;
    }

    private char Current => _pos < _input.Length ? _input[_pos] : '\0';

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_pos < _input.Length)
        {
            if (char.IsWhiteSpace(Current))
            {
                _pos++;
                continue;
            }

            if (char.IsDigit(Current))
            {
                int start = _pos;
                var sb = new System.Text.StringBuilder();
                while (_pos < _input.Length && char.IsDigit(_input[_pos]))
                {
                    sb.Append(_input[_pos]);
                    _pos++;
                }
                int end = _pos - 1;
                tokens.Add(new Token(TokenType.Number, sb.ToString(), start, end));
                continue;
            }

            int singlePos = _pos;
            switch (Current)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+", singlePos, singlePos));
                    _pos++;
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-", singlePos, singlePos));
                    _pos++;
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Multiply, "*", singlePos, singlePos));
                    _pos++;
                    break;
                case '/':
                    tokens.Add(new Token(TokenType.Divide, "/", singlePos, singlePos));
                    _pos++;
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LParen, "(", singlePos, singlePos));
                    _pos++;
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RParen, ")", singlePos, singlePos));
                    _pos++;
                    break;
                default:
                    throw new Exception($"Недопустимый символ '{Current}' на позиции {_pos}");
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", _pos, _pos));
        return tokens;
    }
}