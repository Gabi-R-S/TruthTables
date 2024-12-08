using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace TruthTable
{
    //note: canot use v, V and A as variable names because they will be recognised as OR or AND
    internal class Program
    {
        abstract class Node
        {
            public abstract bool IsTrue();
        }
        class VariableNode : Node
        {
            string variableName;
            public override bool IsTrue()
            {
                return variables[variableName];
            }
            public VariableNode(string variableName)
            {
                this.variableName = variableName;
                if (!variables.ContainsKey(variableName)) { variables.Add(variableName,false); }
            }

            public static Dictionary<string, bool> variables = new Dictionary<string, bool>();
        }
        class NegationNode:Node
        {
            Node child;
            public override bool IsTrue()
            {
                return !child.IsTrue();
            }
            public NegationNode(Node child) { this.child = child; } 
        }
        class AndNode : Node 
        { Node left; Node right;
            public override bool IsTrue()
            {
                return left.IsTrue()&&right.IsTrue();
            }
            public AndNode(Node left, Node right)
            {
                this.left = left;
                this.right = right;
            }
        }
        class OrNode : Node { Node left; Node right;
            public override bool IsTrue()
            {
                return left.IsTrue() || right.IsTrue();
            }
            public OrNode(Node left, Node right)
            {
                this.left = left;
                this.right = right;
            }
        }

        class ImplyNode : Node 
        { Node left;Node right;
            public override bool IsTrue()
            {
                return !(left.IsTrue() && !right.IsTrue());
            }
            public ImplyNode(Node left, Node right)
            {
                this.left = left;
                this.right = right;
            }
        }

        
       

    static Node ParseExpression(string expression)//assumes s has no spaces 
        {
            if (expression.Length == 0) 
            {
                throw new Exception("Parsing function received unexpected empty string");
            }
            //remove parenthisis, somehow

            if (expression[0] == '(')
            {//may be inside parenthisis
                int depth = 1;
                bool insideParenthisis = true;
                for (var v = 1; v < expression.Length - 1; v++)
                {
                    if ((char)expression[v] == ')')
                    {
                        depth -= 1;
                    }
                    if ((char)expression[v] == '(')
                    {
                        depth += 1;
                    }
                    if (depth == 0)
                    {
                        insideParenthisis = false;
                         break;
                    }
                }
                if (insideParenthisis)
                {
                    Console.WriteLine("WILL REMOVE PARENTHISYS " + expression);

                    expression = expression.Remove(0, 1);
                    expression = expression.Remove(expression.Length - 1, 1);
                }

                Console.WriteLine("REMOVED PARENTHISYS " + expression);
            }
            { 
            //imply
            int depth=0;
            int indexOfLast=-1;
            for (var v = 0; v < expression.Length -1; v++) 
            {
                if (expression[v] == '(')
                {
                    depth += 1;
                }
                if (expression[v] == ')')
                {
                    depth -= 1;
                }
                if (expression[v] == '-'&&expression[v+1] == '>' && depth==0) 
                {
                    indexOfLast = v;
                }
            }
            if (indexOfLast != -1) 
            {
                var s1= expression.Substring(0, indexOfLast);
                var s2 = expression.Substring( indexOfLast + 2);
                    Console.WriteLine(s1 + " IMPLIES " + s2);
                    return new ImplyNode(ParseExpression(s1),ParseExpression(s2));
            }

            
            //or
            depth = 0;
            for (var v = 0; v < expression.Length ; v++)
            {
                if (expression[v] == '(')
                {
                    depth += 1;
                }
                if (expression[v] == ')')
                {
                    depth -= 1;
                }
                if ((expression[v] == 'v'|| expression[v] == 'V'|| expression[v] == '|') && depth == 0)
                {
                    var s1 = expression.Substring(0, v );
                    var s2 = expression.Substring(v + 1);
                        Console.WriteLine(s1 + " OR " + s2);
                        return new OrNode(ParseExpression(s1), ParseExpression(s2));
                      

                    }
                }

            depth = 0;
            for (var v = 0; v < expression.Length ; v++)
            {
                if (expression[v] == '(')
                {
                    depth += 1;
                }
                if (expression[v] == ')')
                {
                    depth -= 1;
                }
                if ((expression[v] == '&' || expression[v] == 'A' || expression[v] == '^') && depth == 0)
                {
                    var s1 = expression.Substring(0, v );
                    var s2 = expression.Substring(v + 1);
                        Console.WriteLine(s1+ " AND "+s2);

                    return new AndNode(ParseExpression(s1), ParseExpression(s2));
                }
            }
            }

            if (expression[0] == '!' || expression[0] == '-') 
            {
                Console.WriteLine("NOT"+expression);
                return new NegationNode(ParseExpression(expression.Substring(1)));
          
            }
            Console.WriteLine("NEW VARIABLE:" +expression);
            return new VariableNode(expression);
           

        }

       static string GetTruthTable(string expression) 
        {
            string final="";
            for (var i = 0; i < expression.Length; i++) 
            {
                if (expression[i]==' ') { expression = expression.Remove(i,1); }
            }

            var tree = ParseExpression(expression);
            Console.WriteLine();
            var variables = VariableNode.variables.Keys.ToArray();
            foreach (var p in variables) 
            { 
                final += p+"||";
            }
            final += "\t"+expression+"\n";

            for (uint i = 0;i<Math.Pow(2,variables.Length);i++)
            {
                int c = 1;
                foreach (var v in variables) 
                {
                    VariableNode.variables[v] = (i&c)==0?false:true;
                    c*=2;
                    final += (VariableNode.variables[v]?"T":"F")+"||";
                }

                final += "\t"+(tree.IsTrue()?"T":"F")+"\n";
            }


            return final;
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Insert a logial expression:");

                var expression = Console.ReadLine();
                Console.WriteLine(GetTruthTable(expression));
                VariableNode.variables.Clear();
            }
        }
    }
}