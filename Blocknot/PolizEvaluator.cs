using System;
using System.Collections.Generic;

public static class PolizEvaluator
{
    public static double Evaluate(List<string> poliz)
    {
        Stack<double> stack = new Stack<double>();

        foreach (var token in poliz)
        {
            if (double.TryParse(token, out double num))
            {
                stack.Push(num);
            }
            else
            {
                if (stack.Count < 2)
                    throw new Exception("Недостаточно операндов для операции.");

                double b = stack.Pop();
                double a = stack.Pop();
                double res;
                switch (token)
                {
                    case "+":
                        res = a + b;
                        break;
                    case "-":
                        res = a - b;
                        break;
                    case "*":
                        res = a * b;
                        break;
                    case "/":
                        if (b == 0)
                            throw new DivideByZeroException();
                        res = a / b;
                        break;
                    default:
                        throw new Exception($"Неизвестная операция: {token}");
                }

                stack.Push(res);
            }
        }

        if (stack.Count != 1)
            throw new Exception("Ошибка в вычислении выражения.");

        return stack.Pop();
    }
}
