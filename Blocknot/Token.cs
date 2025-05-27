using System.Collections.Generic;
using System.Text;

public enum TokenType
{
    Number,
    Plus,
    Minus,
    Multiply,
    Divide,
    LParen,
    RParen,
    EOF
}

public class Token
{
    public int TypeCode { get; }
    public TokenType Type { get; }
    public string Value { get; }
    public int StartPos { get; }
    public int EndPos { get; }

    public Token(TokenType type, string value, int startPos, int endPos)
    {
        Type = type;
        Value = value;
        StartPos = startPos;
        EndPos = endPos;

        switch (type)
        {
            case TokenType.LParen:
            case TokenType.RParen:
                TypeCode = 1; // Скобка
                break;
            case TokenType.Plus:
            case TokenType.Minus:
                TypeCode = 2; // Оператор
                break;
            case TokenType.Multiply:
            case TokenType.Divide:
                TypeCode = 3; // Оператор
                break;
            case TokenType.Number:
                TypeCode = 7; // Целое число
                break;
            case TokenType.EOF:
                TypeCode = 0; // Конец файла
                break;
            default:
                TypeCode = -1;
                break;
        }
    }

    public static string GetTypeName(TokenType type)
    {
        switch (type)
        {
            case TokenType.LParen:
            case TokenType.RParen:
                return "скобка";
            case TokenType.Plus:
            case TokenType.Minus:
            case TokenType.Multiply:
            case TokenType.Divide:
                return "оператор";
            case TokenType.Number:
                return "целое число";
            case TokenType.EOF:
                return "конец";
            default:
                return "неизвестно";
        }
    }

    public override string ToString()
    {
        int widthTypeCode = 6;
        int widthDescription = 18;
        int widthValue = 12;
        int widthPosition = 10;

        string description = GetTypeName(Type);

        string formatted = $"{TypeCode.ToString().PadRight(widthTypeCode)} | " +
                           $"{description.PadRight(widthDescription)} | " +
                           $"{Value.PadRight(widthValue)} | " +
                           $"[{StartPos}-{EndPos}]".PadRight(widthPosition);

        return formatted;
    }
}
