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
                        calculation = calculation.ToLower();
                        calculation = calculation.Replace(" ", "");
                        calculation = MinusToPlusNegative(calculation);
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
        static string MinusToPlusNegative(string calc) //changes every binary operation "minus" to a binary "plus" and a unary "negative" to deal with the ambiguity of "-"
        {
            string result = "";
            result += calc[0];
            for (int i = 1; i < calc.Length; i++)
            {
                if ( calc[i] == '-' & (result.Last() == ')' | numeric.Contains(result.Last()))) //when reading a "-" and the preceding symbol indicates an operand (number or subexpression)
                {
                    result += '+';
                }
                result += calc[i];
            }
            return result;
        }
        static List<MathToken> Tokenizer(string calc) //reads through a string, combining every consecutive symbol of one type into one token, then adds the token to the list
        {
            List<MathToken> result = new List<MathToken>();
            while (true)
            {
                string temp = "";
                while (calc.Length > 0 && numeric.Contains(calc[0])) //this actually accepts too much (more than one decimal point), but the NumberToken deals with the actual parsing
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
                while (calc.Length > 0 && alphabetic.Contains(calc[0])) //reading function words
                {
                    temp += calc[0];
                    calc = calc.Substring(1);
                }
                if (temp.Length > 0)
                {
                    if (temp == "pi")           //special case for the constant pi (probably shouldn't try to use "sqrtpi" without brackets)
                    {
                        result.Add(new NumberToken(temp));
                    }
                    else
                    {
                        result.Add(new FunctionToken(temp));
                    }
                    temp = "";
                }
                if (calc.Length == 0) break;
                if (operators.Contains(calc[0])) //operators are single symbols
                {
                    temp += calc[0];
                    if (temp == "-")
                    {
                        result.Add(new FunctionToken(temp)); //special case, because I treat minus as a unary operator (this actually results in 5---5 being accepted as 5+-(-(-(5))), which is slightly weird)
                    }
                    else
                    {
                        result.Add(new OperatorToken(temp));
                    }
                    temp = "";
                    calc = calc.Substring(1);
                }
                if (calc.Length == 0) break;
                if (brackets.Contains(calc[0])) //brackets are operators in some interpretation, but they need a different treatment, so a different identity.
                {
                    temp += calc[0];
                    result.Add(new BracketToken(temp));
                    temp = "";
                    calc = calc.Substring(1);
                }
                if (calc.Length == 0) break;
                if (!(brackets + operators + numeric + alphabetic).Contains(calc[0])) //if the tokenizer encounters a symbol that doesn't fit any of the above
                {
                    throw new ParseException("Unrecognized character: "+ calc[0]);
                }
            }
            return result;
        }
        static List<MathToken> ShuntingYard(List<MathToken> calc) //algorithm by Edsger Dijkstra to turn (possibly) ambiguous infix notation into (much easier to parse) reverse Polish notation
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
                        catch (InvalidOperationException) //if it runs out of stack without finding an opening bracket
                        {
                            throw new SyntaxException("Mismatched parentheses");
                        }
                    }
                }
                else //this should probably go, since every type of token is accounted for
                {
                    throw new SyntaxException("Something went horribly wrong");
                }
            }
            while (stack.Count > 0) //when all input is read, move the stack to the output
            {
                MathToken t = stack.Pop();
                if (t.GetType() == typeof(BracketToken)) //if it finds an opening bracket that was never closed
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
            if (output.Count == 0) //it looks like acceptable input, but once you take the brackets out, you're left with nothing
            {
                throw new SyntaxException("Nothing but parentheses?");
            }
            return output;
        }
        static double SolveRPN(List<MathToken> RPN)     //pushes numbers onto the stack, resolves functions and operators with those numbers, pushes the result back on, and so forth,
        {                                               // until there's (hopefully) one number and no operators left
            Stack<double> operands = new Stack<double>();
            foreach (MathToken t in RPN)
            {
                if (t.GetType() == typeof(OperatorToken))
                {
                    OperatorToken o = (OperatorToken)t;
                    if (operands.Count >= 2) //binary operators need at least 2 numbers
                    {
                        double b = operands.Pop();
                        double a = operands.Pop();
                        double result = o.Operator(a, b);
                        operands.Push(result);
                    }
                    else
                    {
                        throw new SyntaxException("Not enough operands");
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
                    if (operands.Count >= 1) //this only rules out an empty stack
                    {
                        double a = operands.Pop();
                        double result = f.Function(a);
                        operands.Push(result);
                    }
                    else
                    {
                        throw new SyntaxException("Not enough operands");
                    }
                }
            }
            if (operands.Count > 1) //if we're left with more than one number and no operators
            {
                throw new SyntaxException("Not enough operators");
            }
            else
            {
                return operands.Pop();
            }
        }
    }
    public class MathToken //this is the base class. It only stores the source string, but that has no current function. I used it for some diagnostics to print the RPN.
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
            if (s == "pi") //special case for the letter combination pi
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
    public class FunctionToken : MathToken //a class for unary operators
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
                case "-":
                    Function = delegate (double x) { return -x; };
                    break;
                default:
                    throw new ParseException("Unrecognized function: " + s);
            }
        }
        public delegate double Del(double x);
        public Del Function;
    }
    public class OperatorToken : MathToken //a class for binary operators. I've thought about adding a property for higher arity, but there aren't any such operators in common use (especially with infix notation).
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
            else throw new ParseException("Unrecognized bracket: " + s); //does this ever happen? No, but you should be prepared for anything.
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
