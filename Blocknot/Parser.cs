using System;
using System.Collections.Generic;

public class Parser
{
    private readonly List<Token> tokens;
    private int pos;
    private List<string> poliz;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        pos = 0;
        poliz = new List<string>();
    }

    private Token Current => pos < tokens.Count ? tokens[pos] : tokens[tokens.Count - 1];

    private void Eat(TokenType type)
    {
        if (Current.Type == type)
        {
            pos++;
        }
        else
        {
            string msg;
            switch (type)
            {
                case TokenType.Number:
                    msg = "число или '('";
                    break;
                case TokenType.LParen:
                    msg = "'('";
                    break;
                case TokenType.RParen:
                    msg = "закрывающаяся скобка";
                    break;
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Multiply:
                case TokenType.Divide:
                    msg = "оператор";
                    break;
                default:
                    msg = type.ToString();
                    break;
            }

            throw new Exception($"Ошибка: ожидался {msg}. Позиция: [{Current.StartPos}]");
        }
    }

    public List<string> Parse()
    {
        poliz.Clear();
        ParseE();
        if (Current.Type != TokenType.EOF)
            throw new Exception($"Ошибка: лишние символы после выражения. Позиция: [{Current.StartPos}]");
        return poliz;
    }

    // E -> T A
    private void ParseE()
    {
        ParseT();
        ParseA();
    }

    // A -> (+|-) T A | ε
    private void ParseA()
    {
        while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
        {
            var op = Current.Type;
            Eat(op);
            ParseT();
            poliz.Add(op == TokenType.Plus ? "+" : "-");
        }
    }

    // T -> O B
    private void ParseT()
    {
        ParseO();
        ParseB();
    }

    // B -> (*|/) O B | ε
    private void ParseB()
    {
        while (Current.Type == TokenType.Multiply || Current.Type == TokenType.Divide)
        {
            var op = Current.Type;
            Eat(op);
            ParseO();
            poliz.Add(op == TokenType.Multiply ? "*" : "/");
        }
    }

    // O -> Number | (E)
    private void ParseO()
    {
        if (Current.Type == TokenType.Number)
        {
            poliz.Add(Current.Value);
            Eat(TokenType.Number);
        }
        else if (Current.Type == TokenType.LParen)
        {
            int startPos = Current.StartPos;
            Eat(TokenType.LParen);
            ParseE();
            if (Current.Type != TokenType.RParen)
            {
                throw new Exception($"Ошибка: ожидалась закрывающаяся скобка. Позиция: [{Current.StartPos}]");
            }
            Eat(TokenType.RParen);

            // Проверка на пропущенный оператор между скобками/числами
            if (pos < tokens.Count)
            {
                var next = tokens[pos];
                if (next.Type == TokenType.Number || next.Type == TokenType.LParen)
                {
                    throw new Exception($"Ошибка: пропущен оператор. Позиция: [{next.StartPos}]");
                }
            }
        }
        else
        {
            throw new Exception($"Ошибка: ожидалось число или '('. Позиция: [{Current.StartPos}]");
        }
    }
}

