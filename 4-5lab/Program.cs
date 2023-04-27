using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

public static class Program
{

    public static string path = "script3.txt";
    public static string pathResult = "result.txt";

    static Dictionary<int, string> operators = new Dictionary<int, string>()
    {
        {1, "**"},
        {2, "*"},
        {3, "/"},
        {4, "%"},
        {5, "+"},
        {6, "-"},
        {7, ">"},
        {8, "<"},
        {9, "="},
        {10, "=="},
        {11, ">="},
        {12, "<="},
        {13, "!="},
        {14, "-="},
        {15, "*="},
        {16, "/="},
        {17, "%="},
        {18, "+="},
        {19, "("},
        {20, ")"},
    };

    static Dictionary<int, string> specialWordsWithParams = new Dictionary<int, string>()
    {
        {1, "while"},
        {2, "if"},
        {3, "elif"},
        {4, "for"},
        {6, "def"},
        {7, "lambda"},
        {8, "return"},
        {9, "range" },
        {10, "list(set(numpy.concatenate"}
    };

    static Dictionary<int, string> specialWordsWithoutParams = new Dictionary<int, string>()
    {
        {1, "else"},
        {2, "break"},
        {3, "continue"},
    };

    static List<Tuple<string, string>> variables = new List<Tuple<string, string>>();

    static List<Tuple<string, int>> functions = new List<Tuple<string, int>>();

    static bool IsSpecialWordWithParams(string line)
    {
        foreach(var item in specialWordsWithParams)
        {
            if(line.IndexOf(item.Value) != -1)
            {
                return true;
            }
        }
        return false;
    }

    static bool IsBool(string value)
    {
        if (value == "True" || value == "False")
        {
            return true;
        }
        return false;
    }

    static bool IsString(string value)
    {
        if (value.IndexOf('"') == value.LastIndexOf('"'))
        {
            return false;
        }
        return true;
    }

    static bool IsNumber(string num)
    {
        bool result = int.TryParse(num, out var number);
        if (result)
        {
            return true;
        }
        result = float.TryParse(num, out var number2);
        if (result)
        {
            return true;
        }
        return false;
    }

    static string GetType(string val)
    {
        string type = "none";
        if (IsNumber(val))
        {
            type = "num";
        }
        else if (IsBool(val))
        {
            type = "bool";
        }
        else if (IsString(val))
        {
            type = "string";
        }
        else if (variables.FirstOrDefault(func => func.Item1 == val) != null)
        {
            type = variables.FirstOrDefault(func => func.Item1 == val).Item2;
        } else if (val.IndexOf("[[") != -1)
        {
            type = "double array";
        }
        else if (val.IndexOf("[") != -1)
        {
            type = "array";
        }
        return type;
    }

    static List<string> GetFunctionParams(string line)
    {
        List<string> p = new List<string>();
        for (int l = 0; l < line.Length; l++)
        {
            int endOfLexeme = line.Substring(l).IndexOf(',');
            if (endOfLexeme == -1)
            {
                endOfLexeme = line.Length - l;
            }
            string s = line.Substring(l, endOfLexeme);
            if (s != " " && s.Length != 0)
            {
                p.Add(s);
            }
            l += endOfLexeme;
        }
        return p;
    }

    static int GetFunctionName(string line, int i, string tab)
    {
        int endOfName = line.Substring(i).IndexOf(' ');
        string str = line.Substring(i, endOfName);
        int bracket1 = line.IndexOf('(');
        int bracket2 = line.IndexOf(')');
        if (bracket1 == -1 || bracket2 == -1)
        {
            return -1;
        }
        line = line.Substring(bracket1 + 1, bracket2 - bracket1 - 1);
        List<string> p = GetFunctionParams(line);
        functions.Add(new Tuple<string, int>(str, p.Count));
        foreach (var item in p)
        {
            GetParams(item, tab);
        }
        //Console.WriteLine(tab + "def " + str);
        using (StreamWriter writer = new StreamWriter(pathResult, true))
        {
            writer.WriteLine(tab + "def " + str);
        }
        return endOfName;
    }

    static string GetParams(string line, string tab)
    {
        line = line.Replace("(", "");
        line = line.Replace(")", "");
        line = line.Replace(":", "");

        while (line[0] == ' ')
        {
            line = line.Remove(0, 1);
        }
        while (line[line.Length - 1] == ' ')
        {
            line = line.Remove(line.Length - 1, 1);
        }

        List<string> val = new List<string>();
        List<string> op = new List<string>();

        for (int i = 0; i < line.Length; i++)
        {
            int endOfLexeme = line.Substring(i).IndexOf(' ');
            if (endOfLexeme == -1)
            {
                endOfLexeme = line.Length - i;
            }
            string str = line.Substring(i, endOfLexeme);

            if (operators.ContainsValue(str))
            {
                op.Add(str);
            }
            else
            {
                if (op.Count > val.Count)
                {
                    val.Add("null");
                }
                val.Add(str);
            }

            i += endOfLexeme;
        }

        List<string> opWithPriority = new List<string>();

        foreach (var item in operators)
        {
            foreach (var i in op)
            {
                if (i == item.Value)
                {
                    opWithPriority.Add(i);
                    break;
                }
            }
        }

        if (op.Count == 0)
        {
            if (GetType(line) == "none" && variables.FirstOrDefault(func => func.Item1 == line) == null)
            {
                variables.Add(new Tuple<string, string>(line, "none"));
            }
            //Console.WriteLine(tab + line);
            using (StreamWriter writer = new StreamWriter(pathResult, true))
            {
                writer.WriteLine(tab + line);
            }
        }
        else if (op.Count == 1 && op[0] == "=")
        {
            if (GetType(val[1]) == "none" && variables.FirstOrDefault(func => func.Item1 == val[1]) == null)
            {
                return $"error {val[1]} is undefined";
            }
            string type = GetType(val[1]);
            //Console.WriteLine(tab + val[1]);
            //Console.WriteLine(tab + "    " + op[0]);
            //Console.WriteLine(tab + val[0]);
            using (StreamWriter writer = new StreamWriter(pathResult, true))
            {
                writer.WriteLine(tab + val[1]);
                writer.WriteLine(tab + "    " + op[0]);
                writer.WriteLine(tab + val[0]);
            }
            if (variables.FirstOrDefault(func => func.Item1 == val[0]) == null)
            {
                variables.Add(new Tuple<string, string>(val[0], type));
            } else 
            {
                int index = variables.FindIndex(func => func.Item1 == val[0]);
                variables.RemoveAt(index);
                variables.Add(new Tuple<string, string>(val[0], type));
            }

        }
        else
        {
            if (op[0] == "=" && variables.FirstOrDefault(func => func.Item1 == val[0]) == null)
            {
                variables.Add(new Tuple<string, string>(val[0], "none"));
            }
            for (int i = 0; i < opWithPriority.Count; i++)
            {
                int index = op.IndexOf(opWithPriority[i]);
                string varType1 = "none";
                string varType2 = "none";
                if (val[index] != "null")
                {
                    if (GetType(val[index]) == "none" && variables.FirstOrDefault(func => func.Item1 == val[index]) == null)
                    {
                        return $"error {val[index]} is undefined";
                    }
                    if (variables.FirstOrDefault(func => func.Item1 == val[0]) != null)
                    {
                        string type = GetType(val[1]);
                        int j = variables.FindIndex(func => func.Item1 == val[0]);
                        variables.RemoveAt(j);
                        variables.Add(new Tuple<string, string>(val[0], type));
                    }
                    varType1 = GetType(val[index]);
                    //Console.WriteLine(tab + val[index]);
                    using (StreamWriter writer = new StreamWriter(pathResult, true))
                    {
                        writer.WriteLine(tab + val[index]);
                    }
                }
                //Console.WriteLine(tab + "    " + opWithPriority[i]);
                using (StreamWriter writer = new StreamWriter(pathResult, true))
                {
                    writer.WriteLine(tab + "    " + opWithPriority[i]);
                }
                if (index + 1 < val.Count && val[index + 1] != "null")
                {
                    if (GetType(val[index + 1]) == "none" && variables.FirstOrDefault(func => func.Item1 == val[index + 1]) == null)
                    {
                        return $"error {val[index + 1]} is undefined";
                    }
                    varType2 = GetType(val[index + 1]);
                    if(varType2 != varType1 && (varType1 != "none" && varType2 != "none"))
                    {
                        if (varType1 == "num")
                        {
                            varType1 = "int/float";
                        }

                        if (varType2 == "num")
                        {
                            varType2 = "int/float";
                        }
                        return $"error {opWithPriority[i]} is not supported for type {varType1} and {varType2}";
                    }

                    //Console.WriteLine(tab + val[index + 1]);
                    using (StreamWriter writer = new StreamWriter(pathResult, true))
                    {
                        writer.WriteLine(tab + val[index + 1]);
                    }
                    if (index > 0 && opWithPriority.Count > 1)
                    {
                        string temp = val[index - 1];
                        val[index - 1] = "null";
                        val[index] = temp;
                        tab += "    ";
                    }
                }
            }
        }
        return "0";
    }

    static string SyntaxLineAnalize(string line, string tab, bool recurs = false)
    {
        if (!recurs)
        {
            int k = 0;
            string currTab = "";
            while (line[k] == ' ')
            {
                currTab += ' ';
                k++;
            }
            if (currTab.Length % 4 != 0)
            {
                return $"error wrong spacing";
            }
            tab = currTab;
            line = line.Substring(k);
        }
        string check = "";
        for (int i = 0; i < line.Length; i++)
        {
            int endOfLexeme = line.Substring(i).IndexOf(' ');
            if (endOfLexeme == -1)
            {
                endOfLexeme = line.Length - i;
            }
            string str = line.Substring(i, endOfLexeme);
            if (str == "def")
            {
                //if (line[line.Length - 1] != ':')
                //{
                //    return $"error colon not found";
                //}
                int temp = GetFunctionName(line, i + endOfLexeme + 1, tab) + endOfLexeme;
                if (temp - endOfLexeme == -1)
                {
                    return $"error bracket not found";
                }
                i += temp;
                //tab += "   ";
                return tab;
            }
            else if (specialWordsWithParams.ContainsValue(str))
            {
                //if (str != "return" && line[line.Length - 1] != ':')
                //{
                //    return $"error colon not found";
                //}
                check = SyntaxLineAnalize(line.Substring(i + endOfLexeme + 1), tab, true);
                if (check.Length > 5 && check.Substring(0, 5) == "error")
                {
                    return check;
                }
                if (str == "for")
                {
                    string forVar = line.Substring(line.IndexOf(str) + str.Length + 1, 1);
                    //Console.WriteLine(tab + "    " + "in");
                    //Console.WriteLine(tab + forVar);
                    using (StreamWriter writer = new StreamWriter(pathResult, true))
                    {
                        writer.WriteLine(tab + "    " + "in");
                        writer.WriteLine(tab + forVar);
                    }
                }
                //Console.WriteLine(tab + str);
                using (StreamWriter writer = new StreamWriter(pathResult, true))
                {
                    writer.WriteLine(tab + str);
                }
                return tab;
            }
            else if (functions.FirstOrDefault(func => func.Item1 == str) != null)
            {
                int bracket1 = line.IndexOf('(');
                if (bracket1 == -1)
                {
                    return "error bracket not found";
                }
                int bracket2 = line.IndexOf(')');
                if (bracket2 == -1)
                {
                    return "error bracket not found";
                }
                string temp = line.Substring(bracket1 + 1, bracket2 - bracket1 - 1);
                List<string> p = GetFunctionParams(temp);
                Tuple<string, int> f = functions.FirstOrDefault(func => func.Item1 == str);
                if (p.Count < f.Item2)
                {
                    return $"error insufficiency of function parameters: {f.Item1} requires {f.Item2} parameters";
                }
                check = SyntaxLineAnalize(temp, tab, true);
                if (check.Length > 5 && check.Substring(0, 5) == "error")
                {
                    return check;
                }
                //Console.WriteLine(tab + str);
                using (StreamWriter writer = new StreamWriter(pathResult, true))
                {
                    writer.WriteLine(tab + str);
                }
                if (bracket2 != line.Length - 1)
                {
                    SyntaxLineAnalize(line.Substring(bracket2 + 1), tab, true);
                }
                return tab;
            }
            else if (specialWordsWithoutParams.ContainsValue(str))
            {
                //Console.WriteLine(tab + str);
                using (StreamWriter writer = new StreamWriter(pathResult, true))
                {
                    writer.WriteLine(tab + str);
                }
                return tab;
            }
        }
        check = GetParams(line, tab);
        if (check != "0")
        {
            return check;
        }
        return tab;
    }

    static string SearchForVariable(string line)
    {
        int index = line.IndexOf("for(");
        if (index == -1)
        {
            return "";
        }
        for(int i = index + "for(".Length; i < line.Length; i++)
        {
            if (line[i] != ' ')
            {
                int endOfLexeme = line.Substring(i).IndexOf(' ');
                return line.Substring(i, endOfLexeme);
            }
        }
        return "";
    }

    static string SearchRangeVariable(string line)
    {
        int index = line.IndexOf("range(");
        if (index == -1)
        {
            return "";
        }
        for (int i = index + "range(".Length; i < line.Length; i++)
        {
            if (line[i] != ' ')
            {
                int endOfLexeme = line.Substring(i).IndexOf(')');
                return line.Substring(i, endOfLexeme);
            }
        }
        return "";
    }

    public static async Task Main(string[] args)
    {
        functions.Add(new Tuple<string, int>("print", 1));
        variables.Add(new Tuple<string, string>("list(set(numpy.concatenate ([a,b])))", "array"));
        variables.Add(new Tuple<string, string>("lena", "num"));
        //functions.Add(new Tuple<string, int>("list(set(numpy.concatenate", 1));
        //functions.Add(new Tuple<string, int>("range", 1));
        if (File.Exists(pathResult))
        {
            File.Delete(pathResult);
        }
        using (StreamReader reader = new StreamReader(path))
        {
            string? line;
            string tab = "";
            int i = 0;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line != "")
                {
                    tab = SyntaxLineAnalize(line, tab);
                    if (tab.Length > 5 && tab.Substring(0, 5) == "error")
                    {
                        Console.WriteLine($"ERROR {tab} in {i + 1} line");
                        return;
                    }
                }
                i++;
            }
        }

        List<string> stringCode = new List<string>();
        List<string> existsVar = new List<string>();
        int scopes = 0;
        using (StreamReader reader = new StreamReader(pathResult))
        {
            string? line;
            string parentNode = "";
            string childNode = "";
            bool ch = false;
            string newStringCode = "";
            string tab = "";
            bool closeScope = false;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.IndexOf(tab) == -1)
                {
                    tab = tab.Substring(0, tab.Length - 4);
                    closeScope = true;
                }
                if (ch && line.Length > 4 && line.IndexOf(tab + "    ") != -1)
                {
                    childNode = newStringCode;
                } else if (ch)
                {
                    stringCode.Add(newStringCode);
                    ch = false;
                    childNode = "";
                    parentNode = "";
                }
                //if (line.IndexOf("for") != -1)
                //{
                //    int a = 5;
                //}
                if (specialWordsWithParams.ContainsValue(line.Replace(" ", "")))
                {
                    if (childNode != "" && parentNode == "")
                    {
                        stringCode.Add(childNode);
                    }
                    newStringCode = line + "(" + stringCode[stringCode.Count - 1] + ")";
                    stringCode.Remove(stringCode[stringCode.Count - 1]);

                    if (line.IndexOf("range") == -1 && line.IndexOf("list(set(numpy.concatenate") == -1)
                    {
                        stringCode.Add(newStringCode);
                        stringCode.Add("{");
                        scopes++;
                        tab += "    ";
                    } else
                    {
                        childNode = newStringCode;
                    }

                    continue;
                } else if (functions.FirstOrDefault(func => func.Item1 == line.Replace(" ", "")) != null)
                {
                    if (childNode != "" && parentNode == "")
                    {
                        stringCode.Add(childNode);
                    }
                    newStringCode = line + "(" + stringCode[stringCode.Count - 1] + ")";
                    stringCode.Remove(stringCode[stringCode.Count - 1]);
                    stringCode.Add(newStringCode);
                    continue;
                } else if (variables.FirstOrDefault(var => var.Item1 == line.Replace(" ", "")) != null && !existsVar.Contains(line.Replace(" ", "")))
                {
                    existsVar.Add(line.Replace(" ", ""));
                    line = variables.FirstOrDefault(var => var.Item1 == line.Replace(" ", "")).Item2 + " " + line;
                }
                if (childNode == "")
                {
                    childNode = line;
                } else if (parentNode == "")
                {
                    parentNode = line;
                }
                else
                {
                    if (parentNode.IndexOf('=') != -1 || parentNode.IndexOf('<') != -1 || parentNode.IndexOf("in") != -1)
                    {
                        string temp = line;
                        line = childNode;
                        childNode = temp;
                    }
                    newStringCode = $"{childNode} {parentNode} {line}";
                    childNode = "";
                    parentNode = "";
                    ch = true;
                }
                if (closeScope)
                {
                    while (scopes != 0)
                    {
                        stringCode.Add("}");
                        scopes--;
                    }

                    closeScope = false;
                }
            }
        }
        while (scopes != 0)
        {
            stringCode.Add("}");
            scopes--;
        }
        string code = "";
        int f = 0;
        for (int i = 0; i < stringCode.Count; i++)
        {
            if (stringCode[i].IndexOf("for") != -1 && f == 2)
            {
                code += "}" + "\r\n";
                f = -100;
            } else if (stringCode[i].IndexOf("for") != -1)
            {
                f += 1;
            }
            stringCode[i] = stringCode[i].Replace("print", "Console.WriteLine");

            stringCode[i] = stringCode[i].Replace("num", "float");

            stringCode[i] = stringCode[i].Replace("none", "");
            
            if (stringCode[i].IndexOf("array") != -1)
            {
                stringCode[i] = stringCode[i].Replace("[", "{");
                stringCode[i] = stringCode[i].Replace("]", "}");
            }
            stringCode[i] = stringCode[i].Replace("double array", "float[,]");
            stringCode[i] = stringCode[i].Replace("array", "float[]");
            stringCode[i] = stringCode[i].Replace("][", ",");
            stringCode[i] = stringCode[i].Replace("float lena", "a.Length - 1");
            stringCode[i] = stringCode[i].Replace("listsetfloatpy.concatenate{a,b}", "c.Concat(b).ToArray().Distinct().ToArray()");
            if (i == stringCode.Count - 1)
            {
                code += "foreach( var item in a)" + "\r\n";
                code += "{\r\n";
                stringCode[i] = stringCode[i].Replace("result", "item");
                code += stringCode[i] + ";\r\n";
                code += "}\r\n";
                continue;
            }
            string forVar = SearchForVariable(stringCode[i]);
            string rangeVar = SearchRangeVariable(stringCode[i]);

            if (forVar != "" && rangeVar != "")
            {
                stringCode[i] = stringCode[i].Replace("in", "");
                stringCode[i] = stringCode[i].Replace("range(", "");
                stringCode[i] = stringCode[i].Replace(forVar, "");
                stringCode[i] = stringCode[i].Replace(rangeVar, "");
                stringCode[i] = stringCode[i].Replace("))", ")");
            }
            if (stringCode[i].IndexOf("temp") != -1 || stringCode[i].IndexOf("temp1") != -1)
            {
                stringCode[i] = stringCode[i].Replace("float[]", "float");
            }
            stringCode[i] = stringCode[i].Replace("for(", $"for(int {forVar} = 0; {forVar} < {rangeVar}; {forVar}++");
            if (!IsSpecialWordWithParams(stringCode[i]) && stringCode[i] != "{" && stringCode[i] != "}")
            {
                stringCode[i] += ";";
            }
            if (stringCode[i].IndexOf("if") != -1)
            {
                stringCode[i] = stringCode[i].Replace("float[]", "");
                stringCode[i] = stringCode[i].Replace("{", "[");
                stringCode[i] = stringCode[i].Replace("}", "]");
            }
            stringCode[i] = stringCode[i].Replace("{j}", "[j]");
            code += stringCode[i] + "\r\n";
        }
        //Console.WriteLine(code);
        Console.WriteLine("RESULT");

        var options = ScriptOptions.Default
            .AddImports("System", "System.IO", "System.Collections.Generic",
                "System.Console", "System.Diagnostics", "System.Dynamic",
                "System.Linq", "System.Text",
                "System.Threading.Tasks")
            .AddReferences("System", "System.Core", "Microsoft.CSharp");



        var script = CSharpScript.Create(code, options);
        script.Compile();
        await script.RunAsync();



    }
}

