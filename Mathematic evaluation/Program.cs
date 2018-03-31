using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathematic_evaluation
{
    class Program
    {
        static string numerics = "1234567890,.";
        static string operators = "()^*/+-";
        static void Main(string[] args)
        {
            Console.WriteLine("Enter calculation:");
            string calculation = Console.ReadLine();
            try
            {
                List<string> split = SplitCalc(calculation);
                List<string> RPN = ShuntingYard(split);
                double result = SolveRPN(RPN);
                Console.WriteLine("Result: "+result);
            }
            catch (SyntaxException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        static List<string> SplitCalc(string calc)
        {
            List<string> output = new List<string>();
            string temp = "";
            foreach (char c in calc)
            {
                if (numerics.Contains(c))
                {
                    temp += c;
                }
                else if (operators.Contains(c))
                {
                    if (temp.Length > 0)
                    {
                        double t;
                        if (double.TryParse(temp, out t))
                        {
                            output.Add(temp);
                            temp = "";
                        }
                        else
                        {
                            throw new SyntaxException("Unparsable number: " + temp);
                        }
                    }
                    output.Add(c.ToString());
                }
                else
                {
                    throw new SyntaxException("Unrecognized character: " + c);
                }
            }
            if (temp.Length > 0)
            {
                double t;
                if (double.TryParse(temp, out t))
                {
                    output.Add(temp);
                    temp = "";
                }
                else
                {
                    throw new SyntaxException("Unparsable number: " + temp);
                }
            }
            return output;
        }
        static List<string> ShuntingYard(List<string> calc)
        {
            List<string> output = new List<string>();
            Stack<string> stack = new Stack<string>();
            foreach (string s in calc)
            {
                if (numerics.Contains(s[0]))
                {
                    output.Add(s);
                }
                else if (s == "^")
                {
                    stack.Push(s);
                }
                else if (s == "/")
                {
                    while (stack.Count > 0 && "^/*".Contains(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(s);
                }
                else if (s == "*")
                {
                    while (stack.Count > 0 && "^/*".Contains(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(s);
                }
                else if (s == "-")
                {
                    while(stack.Count > 0 && "^/*+-".Contains(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(s);
                }
                else if (s == "+")
                {
                    while (stack.Count > 0 && "^/*+-".Contains(stack.Peek()))
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(s);
                }
                else if (s == "(")
                {
                    stack.Push(s);
                }
                else if (s == ")")
                {
                    try
                    {
                        while (stack.Peek() != "(")
                        {
                            output.Add(stack.Pop());
                        }
                        stack.Pop();
                    }
                    catch (InvalidOperationException)
                    {
                        throw new SyntaxException("Mismatched parentheses");
                    }
                }
                else
                {
                    throw new SyntaxException("Unrecognized character: " + s);
                }
            }
            while (stack.Count > 0)
            {
                string s = stack.Pop();
                if (s == "(")
                {
                    throw new SyntaxException("Mismatched parentheses");
                }
                else
                {
                    output.Add(s);
                }
            }
            return output;
        }
        static double SolveRPN(List<string> RPN)
        {
            Stack<double> operands = new Stack<double>();
            foreach (string s in RPN)
            {
                if (operators.Contains(s))
                {
                    if (operands.Count >= 2)
                    {
                        double b = operands.Pop();
                        double a = operands.Pop();
                        double result = new double();
                        switch (s)
                        {
                            case "^":
                                 result = Math.Pow(a, b);
                                 break;
                            case "*":
                                result = a * b;
                                break;
                            case "/":
                                result = a / b;
                                break;
                            case "+":
                                result = a + b;
                                break;
                            case "-":
                                result = a - b;
                                break;    
                        }
                        operands.Push(result);
                    }
                    else
                    {
                        throw new SyntaxException("Syntax error: not enough operands");
                    }
                }
                else
                {
                    double n = double.Parse(s);
                    operands.Push(n);
                }
            }
            if (operands.Count > 1)
            {
                throw new SyntaxException("Syntax error: not enough operators");
            }
            else
            {
                return operands.Pop();
            }
        }
    }
    public class SyntaxException : Exception
    {
        public SyntaxException()
        {

        }
        public SyntaxException(string message)
            : base(message)
        {

        }
        public SyntaxException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
