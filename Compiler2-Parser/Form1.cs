using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace Compiler2_Parser
{
    public partial class Form1 : Form
    {
        // Scanner Start
        static StreamWriter outputStream = new StreamWriter("output.txt");
        static int tokenNo = 0;
        enum ReservedWords
        {
            WRITE,
            READ,
            IF,
            ELSE,
            RETURN,
            BEGIN,
            END,
            MAIN,
            STRING,
            INT,
            REAL,
            THEN,
            REPEAT,
            UNTIL
        }
        enum tokenType
        {
            reservedWord,
            number,
            identifier,
            assign,
            equal,
            notEqual,
            add,
            multiply,
            subtract,
            divide
        }
        static char[] separator = { ' ', ';', ',', '(', ')' };
        static char[] comments = { '{', '}' };
        static char[] operators = { '+', '-', '*', '/', '<', '>' };
        static char[] multiOperators = { ':', '=', '!' };
        static char multiOperators1 = '=';
        static string identifier = @"[a-z A-Z]([a-z A-Z]|[0-9])*";
        static string digits = @"[0-9]*";
        static string TokenType(string line)
        {

            foreach (string x in Enum.GetNames(typeof(ReservedWords)))
            {
                if (line == x || line == x.ToLower())
                {
                    return "reserved word";
                }
            }

            Match match = Regex.Match(line, identifier);
            if (match.Value.Length == line.Length)
            {
                return "identifier";
            }
            match = Regex.Match(line, digits);
            if (match.Value.Length == line.Length)
            {
                return "number";
            }
            return "not identified";
        }
        static void Token(string expres)
        {

            string temp = "";
            bool cont = true;
            for (int i = 0; i < expres.Length; i++)
            {
                if (expres[i] == '{')
                {
                    outputStream.WriteLine(expres[i] + " ," + "Comment start");
                    tokenNo++;
                    int x = expres.IndexOf('}');
                    if (x != -1)
                    {

                        i = x;
                        outputStream.WriteLine(expres[i] + " ," + "Comment end");
                        tokenNo++;
                    }
                    else
                    {
                        i = expres.Length;
                        cont = false;
                    }

                }
                else if (expres[i] == '}')
                {
                    outputStream.WriteLine(expres[i] + " ," + "Comment end");
                    tokenNo++;
                }
                else
                {

                    for (int j = 0; j < separator.Length && cont; j++)
                    {

                        if (expres[i] == separator[j])
                        {
                            if (temp != " " && temp != "")
                            {

                                outputStream.WriteLine(temp + " ," + TokenType(temp));
                                tokenNo++;
                            }
                            temp = "";
                            if (expres[i] != ' ')
                            {
                                outputStream.WriteLine(expres[i] + " ," + "special Characters");
                                tokenNo++;
                            }
                            cont = false;

                        }
                    }
                    for (int j = 0; j < operators.Length && cont; j++)
                    {
                        if (expres[i] == operators[j])
                        {
                            cont = false;
                            if (temp != " " && temp != "")
                            {

                                outputStream.WriteLine(temp + " ," + TokenType(temp));
                                tokenNo++;
                            }
                            temp = "";
                            switch (expres[i])
                            {
                                case '+':
                                    outputStream.WriteLine(expres[i] + " ," + "add");
                                    tokenNo++;
                                    break;
                                case '-':
                                    outputStream.WriteLine(expres[i] + " ," + "subtract");
                                    tokenNo++;
                                    break;
                                case '*':
                                    outputStream.WriteLine(expres[i] + " ," + "multiply");
                                    tokenNo++;
                                    break;
                                case '/':
                                    outputStream.WriteLine(expres[i] + " ," + "divide");
                                    tokenNo++;
                                    break;
                                case '>':
                                    outputStream.WriteLine(expres[i] + " ," + "greater than");
                                    tokenNo++;
                                    break;
                                case '<':
                                    outputStream.WriteLine(expres[i] + " ," + "smaller than");
                                    tokenNo++;
                                    break;
                            }
                        }
                    }
                    for (int j = 0; j < multiOperators.Length && cont; j++)
                    {


                        if (expres[i] == multiOperators[j])
                        {
                            cont = false;
                            //e3ml temp 3bara 3n eh
                            if (temp != " " && temp != "")
                            {

                                outputStream.WriteLine(temp + " ," + TokenType(temp));
                                tokenNo++;
                            }
                            temp = "";
                            switch (expres[i])
                            {
                                case ':':
                                    outputStream.WriteLine(expres[i] + "" + expres[i + 1] + " ," + "assign");
                                    tokenNo++;
                                    break;
                                case '=':
                                    outputStream.WriteLine(expres[i] + "" + " ," + "equal");
                                    tokenNo++;
                                    break;
                                case '!':
                                    outputStream.WriteLine(expres[i] + "" + expres[i + 1] + " ," + "notequal");
                                    tokenNo++;
                                    break;
                            }
                            i++;
                        }
                    }
                    if (i + 1 == expres.Length && cont)
                    {
                        temp = temp + expres[i];
                        if (temp != " " && temp != "")
                        {

                            outputStream.WriteLine(temp + " ," + TokenType(temp));
                            tokenNo++;
                        }
                        temp = "";
                    }
                }
                if (cont)
                {
                    temp = temp + expres[i];
                }
                else
                {
                    cont = true;
                }
            }
        }
        //Scanner End

        //Parse Start
        struct tokens
        {
            public string value;
            public string type;
        }
        Stack<int> tempId = new Stack<int>(100);
        static tokens[] token;
        static int tokNo = 0;
        static tokens currentToken;
        
        TreeNode currentNode;
        Stack<TreeNode> lastNode = new Stack<TreeNode>(100) ;
        void Parse()
        {
            
            currentNode = treeView1.Nodes.Add("Node"+tokNo,"Start");
            prog();
        }
        void prog()
        {
            stmt_Sequence();
            return;
        }
        void stmt_Sequence()
        {
            statement();
            while(currentToken.value == ";")
            {
                Match(";");
                //currentNode = lastNode.Pop();
                statement();
            }
        }
        void statement()
        {
            switch (currentToken.value)
            {
                case "if":
                    if_stmt();
                    break;
                case "repeat":
                    repeat_stmt();
                    break;
                case "read":
                    read_stmt();
                    break;
                case "write":
                    write_stmt();
                    break;
                default: if(currentToken.type == "identifier")
                    {
                        assign_stmt();
                    }
                    break;
            }
        }
        void if_stmt()
        {
            if(currentToken.value == "if")
            {
                lastNode.Push(currentNode);
                currentNode = currentNode.Nodes.Add("Node" + (tokNo+1), "if");
                Match("if");
                exp();
                currentNode = lastNode.Pop();
                Match("then");
                stmt_Sequence();
                switch (currentToken.value)
                {
                    case "end":
                        Match("end");
                        break;
                    case "else":
                        Match("else");
                        stmt_Sequence();
                        Match("end");
                        break;
                    default:
                        Error();
                        break;
                }
            }
        }
        void repeat_stmt()
        {
            if(currentToken.value == "repeat")
            {
                lastNode.Push(currentNode);
                currentNode = currentNode.Nodes.Add("Node" + (tokNo + 1), "repeat");
                Match("repeat");
                stmt_Sequence();
                currentNode = lastNode.Pop();
                Match("until");
                exp();
                currentNode = lastNode.Pop();
            }
        }

        void assign_stmt()
        {
            if(currentToken.type == "identifier")
            {
                lastNode.Push(currentNode);
                currentNode = currentNode.Nodes.Add("Node" + (tokNo + 1), "assign " +currentToken.value);
                Match("identifier");
                Match(":=");
                exp();
                currentNode = lastNode.Pop();
            }
        }
        void read_stmt()
        {
            if (currentToken.value == "read")
            {
                lastNode.Push(currentNode);
                Match("read");
                currentNode = currentNode.Nodes.Add("Node" + (tokNo + 1), "read " + currentToken.value);
                Match("identifier");
            }
        }
        void write_stmt()
        {
            if(currentToken.value == "write")
            {
                lastNode.Push(currentNode);
                currentNode = currentNode.Nodes.Add("Node" + (tokNo + 1), "write");
                Match("write");
                exp();
                currentNode = lastNode.Pop();
            }
        }
        void exp()
        {
            simple_exp();
            if(currentToken.value == "=" || currentToken.value == "<" || currentToken.value == ">" || currentToken.value == "!=")
            {
                lastNode.Push(currentNode);
                currentNode = currentNode.Nodes.Add("Node" + (tokNo + 1), "op " + currentToken.value);
                switch (currentToken.value)
                {
                    case "=":
                        Match("=");
                        break;
                    case "<":
                        Match("<");
                        break;
                    case ">":
                        Match(">");
                        break;
                    case "!=":
                        Match("!=");
                        break;
                }
                simple_exp();
                //currentNode = lastNode.Pop();
            }
        }
        void simple_exp()
        {
            term();
            while (currentToken.value == "+"|| currentToken.value == "-")
            {
                lastNode.Push(currentNode);
                currentNode = currentNode.Nodes.Add("Node" + (tokNo + 1), "op " + currentToken.value);
                switch (currentToken.value)
                {
                    case "+":
                        Match("+");
                        break;
                    case "-":
                        Match("-");
                        break;
                }
                term();

                currentNode.Nodes.Add("Node" + (tokNo + 1), token[tempId.Pop()].value);
                //currentNode = lastNode.Pop();
            }
            if (tempId.Count != 0)
            {
                currentNode.Nodes.Add("Node" + (tokNo + 1), token[tempId.Pop()].value);
            }
        }
        void term()
        {
            factor();
            while (currentToken.value == "*")
            {
                lastNode.Push(currentNode);
                currentNode = currentNode.Nodes.Add("Node" + (tokNo + 1), "op " + currentToken.value);
                //currentNode.Nodes.Add("Node" + (tokNo + 1), token[tokNo-1].value);
                Match("*");
                factor();
                //currentNode = lastNode.Pop();
            }
            
        }
        void factor()
        {
            switch (currentToken.value)
            {
                case "(":
                    lastNode.Push(currentNode);
                    currentNode.Nodes.Add("Node" + (tokNo + 1), "(");
                    Match("(");
                    exp();
                    currentNode = lastNode.Pop();
                    currentNode.Nodes.Add("Node" + (tokNo + 1), ")");
                    Match(")");
                    break;
                default: switch (currentToken.type)
                    {
                        case "number":
                            tempId.Push(tokNo);
                            //tempId = currentToken;
                           // currentNode.Nodes.Add("Node" + (tokNo + 1), currentToken.value);
                            Match("number");
                            break;
                        case "identifier":
                            tempId.Push(tokNo);
                            //tempId = currentToken;
                            // currentNode.Nodes.Add("Node" + (tokNo + 1), currentToken.value);
                            Match("identifier");
                            break;
                        default:
                            Error();
                            break;
                    }
                    break;
            }
        }
        void Match(string expectedToken)
        {
            if(currentToken.value == expectedToken || currentToken.type == expectedToken)
            {
                tokNo++;
                if (tokNo < token.Length)
                {
                    currentToken = token[tokNo];
                    if(currentToken.value == "{")
                    {
                        tokNo += 2;
                        currentToken = token[tokNo];
                    }
                }
                else
                {
                    currentToken.value = null;
                    currentToken.type = null;
                }
            }
            else
            {
                Error();
            }

        }
        void Error()
        {
            MessageBox.Show("Error at "+currentToken.value, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        static tokens divideValueType(string line)
        {
            tokens output;
            output.type = "none";
            output.value = "none";
            string[] temp = new string[2];
            temp = line.Split(',');
            output.value = temp[0];
            for(int i = 0; i < output.value.Length; i++)
            {
                if(output.value[i]==' ')
                {
                    output.value = output.value.Remove(i, 1);
                }
            }
            output.type = temp[1];
            for (int i = 0; i < output.type.Length; i++)
            {
                if (output.type[i] == ' ')
                {
                    output.type = output.type.Remove(i, 1);
                }
            }
            return output; 
        }
        //Parse End
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            StreamWriter tempStream = new StreamWriter("input.txt");
            tempStream.WriteLine(textBox1.Text);
            tempStream.Close();
            String line;
            try
            {
                //Pass the file path and file name 
                StreamReader inputStream = new StreamReader("input.txt");
                //Read the first line of text
                line = inputStream.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    //Token function
                    Token(line);
                    //Read the next line
                    line = inputStream.ReadLine();
                }
                //close the file
                inputStream.Close();
                outputStream.Close();
                tokens[] tokena = new tokens[tokenNo];
                StreamReader parseinput = new StreamReader("output.txt");
                int currenttok = 0; // to enter values into array
                line = parseinput.ReadLine();
                tokena[currenttok] = divideValueType(line);
                while (line != null)
                {
                    line = parseinput.ReadLine();
                    currenttok++;
                    if(line != null)
                    {
                        tokena[currenttok] = divideValueType(line);

                    }
                }
                token = tokena;
                currentToken = token[tokNo];
                Parse();
                parseinput.Close();
                MessageBox.Show("Done!", "Parsing Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception er)
            {
                MessageBox.Show("Exception: " + er.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
