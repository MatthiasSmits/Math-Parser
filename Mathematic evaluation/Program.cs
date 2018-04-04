using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathematic_evaluation
{
    class Program
    {
        static string numeric = "1234567890,.";
        static string alphabetic = "abcdefghijklmnopqrstuvwxyz";
        static string operators = "^*/+-";
        static string brackets = "()";
        static void Main(string[] args)
        {
            TakeInput();
        }
        static void TakeInput()
        {
            while (true)
            {
                Console.WriteLine("Enter calculation:");
                string calculation = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(calculation))
                {
                    try
                    {
                        List<MathToken> split = Tokenizer(calculation);
                        List<MathToken> RPN = ShuntingYard(split);
                        double result = SolveRPN(RPN);
                        Console.WriteLine("Result: " + result);
                    }
                    catch (ParseException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (SyntaxException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("An unexpected error occurred.");
                    }
                }
                else
                {
                    Console.WriteLine("No input.");
                }
            }
        }
        static List<MathToken> Tokenizer(string calc)
        {
            calc = calc.ToLower();
            calc = calc.Replace(" ", "");
            List<MathToken> result = new List<MathToken>();
            while (true)
            {
                string temp = "";
                while (calc.Length > 0 && numeric.Contains(calc[0]))
                {
                    temp += calc[0];
                    calc = calc.Substring(1);
                }
                if (temp.Length > 0)
                {
                    result.Add(new NumberToken(temp));
                    temp = "";
                }
                if (calc.Length == 0) break;
                while (calc.Length > 0 && alphabetic.Contains(calc[0]))
                {
                    temp += calc[0];
                    calc = calc.Substring(1);
                }
                if (temp.Length > 0)
                {
                    if (temp == "pi")
                    {
                        result.Add(new NumberToken(temp));
                        temp = "";
                    }
                    else
                    {
                        result.Add(new FunctionToken(temp));
                        temp = "";
                    }
                }
                if (calc.Length == 0) break;
                if (operators.Contains(calc[0]))
                {
                    temp += calc[0];
                    result.Add(new OperatorToken(temp));
                    temp = "";
                    calc = calc.Substring(1);
                }
                if (calc.Length == 0) break;
                if (brackets.Contains(calc[0]))
                {
                    temp += calc[0];
                    result.Add(new BracketToken(temp));
                    temp = "";
                    calc = calc.Substring(1);
                }
                if (calc.Length == 0) break;
                if (!(brackets + operators + numeric + alphabetic).Contains(calc[0]))
                {
                    throw new ParseException("Unrecognized character: "+ calc[0]);
                }
            }
            return result;
        }
        static List<MathToken> ShuntingYard(List<MathToken> calc)
        {
            List<MathToken> output = new List<MathToken>();
            Stack<MathToken> stack = new Stack<MathToken>();
            foreach (MathToken t in calc)
            {
                if (t.GetType() == typeof(NumberToken))
                {
                    output.Add(t);
                }
                else if (t.GetType() == typeof(FunctionToken))
                {
                    stack.Push(t);
                }
                else if (t.GetType() == typeof(OperatorToken))
                {
                    while (stack.Count > 0)
                    {
                        if (stack.Peek().GetType() == typeof(FunctionToken))
                        {
                            output.Add(stack.Pop());
                        }
                        else if (stack.Peek().GetType() == typeof(OperatorToken))
                        {
                            OperatorToken stackoper = (OperatorToken)stack.Peek();
                            OperatorToken inputoper = (OperatorToken)t;
                            if (stackoper.Precedence >= inputoper.Precedence)
                            {
                                output.Add(stack.Pop());
                            }
                            else break;
                        }
                        else break;
                    }
                    stack.Push(t);
                }
                else if (t.GetType() == typeof(BracketToken))
                {
                    BracketToken b = (BracketToken) t;
                    if (b.IsLeft)
                    {
                        stack.Push(t);
                    }
                    else
                    {
                        try
                        {
                            while (true)
                            {
                                if (stack.Peek().GetType() == typeof(BracketToken))
                                {
                                    BracketToken bracket = (BracketToken)stack.Peek();
                                    if (bracket.IsLeft)
                                    {
                                        stack.Pop();
                                        break;
                                    }
                                }
                                output.Add(stack.Pop());
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            throw new SyntaxException("Mismatched parentheses");
                        }
                    }
                }
                else
                {
                    throw new SyntaxException("Something went horribly wrong");
                }
            }
            while (stack.Count > 0)
            {
                MathToken t = stack.Pop();
                if (t.GetType() == typeof(BracketToken))
                {
                    BracketToken b = (BracketToken) t;
                    if(b.IsLeft)
                        throw new SyntaxException("Mismatched parentheses");
                }
                else
                {
                    output.Add(t);
                }
            }
            if (output.Count == 0)
            {
                throw new SyntaxException("Nothing but parentheses?");
            }
            return output;
        }
        static double SolveRPN(List<MathToken> RPN)
        {
            Stack<double> operands = new Stack<double>();
            foreach (MathToken t in RPN)
            {
                if (t.GetType() == typeof(OperatorToken))
                {
                    OperatorToken o = (OperatorToken)t;
                    if (operands.Count >= 2)
                    {
                        double b = operands.Pop();
                        double a = operands.Pop();
                        double result = o.Operator(a, b);
                        operands.Push(result);
                    }
                    else
                    {
                        throw new SyntaxException("Syntax error: not enough operands");
                    }
                }
                else if (t.GetType() == typeof(NumberToken))
                {
                    NumberToken n = (NumberToken)t;
                    operands.Push(n.NumberValue);
                }
                else if (t.GetType() == typeof(FunctionToken))
                {
                    FunctionToken f = (FunctionToken)t;
                    if (operands.Count >= 1)
                    {
                        double a = operands.Pop();
                        double result = f.Function(a);
                        operands.Push(result);
                    }
                    else
                    {
                        throw new SyntaxException("Syntax error: not enough operands");
                    }
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
    public class MathToken
    {
        public MathToken()
        {

        }
        public MathToken(string s)
        {
            SourceString = s;
        }
        public string SourceString;
    }
    public class NumberToken : MathToken
    {
        public NumberToken(string s) : base(s)
        {
            if (s == "pi")
            {
                numberValue = Math.PI;
            }
            else if (!double.TryParse(s, out numberValue))
            {
                throw new ParseException("Unparseable number: " + s);
            }
        }
        private double numberValue;
        public double NumberValue
        {
            get { return numberValue; }
        }
    }
    public class FunctionToken : MathToken
    {
        public FunctionToken(string s) : base(s)
        {
            switch (s)
            {
                case "sin":
                    Function = Math.Sin;
                    break;
                case "cos":
                    Function = Math.Cos;
                    break;
                case "tan":
                    Function = Math.Tan;
                    break;
                case "sqrt":
                    Function = Math.Sqrt;
                    break;
                default:
                    throw new ParseException("Unrecognized function: " + s);
            }
        }
        public delegate double Del(double x);
        public Del Function;
    }
    public class OperatorToken : MathToken
    {
        public OperatorToken(string s) : base(s)
        {
            switch (s)
            {
                case "^":
                    Operator = Math.Pow;
                    precedence = 2;
                    break;
                case "/":
                    Operator = delegate (double x, double y) { return x / y; };
                    precedence = 1;
                    break;
                case "*":
                    Operator = delegate (double x, double y) { return x * y; };
                    precedence = 1;
                    break;
                case "-":
                    Operator = delegate (double x, double y) { return x - y; };
                    precedence = 0;
                    break;
                case "+":
                    Operator = delegate (double x, double y) { return x + y; };
                    precedence = 0;
                    break;
                default:
                    throw new ParseException("Unrecognized operator: " + s);

            }
        }
        private int precedence;
        public int Precedence
        {
            get { return precedence; }
        }
        public delegate double Del(double x, double y);
        public Del Operator;
    }
    public class BracketToken : MathToken
    {
        public BracketToken (string s) : base(s)
        {
            if (s == "(") isLeft = true;
            else if (s == ")") isLeft = false;
            else throw new ParseException("Unrecognized bracket: " + s);
        }
        private bool isLeft;
        public bool IsLeft
        {
            get { return isLeft; }
        }
    }
    public class ParseException : Exception
    {
        public ParseException() : base()
        {

        }
        public ParseException(string message)
            : base(message)
        {

        }
        public ParseException(string message, Exception inner)
            : base(message, inner)
        {

        }

    }
    public class SyntaxException : Exception
    {
        public SyntaxException() : base()
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
