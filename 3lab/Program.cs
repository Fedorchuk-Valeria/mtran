using System;
using System.Reflection;

public static class Program
{
    public static string path = "script2.txt";

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
        {20, ")"}
    };

    static Dictionary<int, string> specialWordsWithParams = new Dictionary<int, string>()
    {
        {1, "while"},
        {2, "if"},
        {3, "elif"},
        {4, "for"},
        {5, "range"},
        {6, "def"},
        {7, "lambda"},
        {8, "return"}
    };

    static Dictionary<int, string> specialWordsWithoutParams = new Dictionary<int, string>()
    {
        {1, "else"},
        {2, "break"},
        {3, "continue"},
    };

    static List<string> variables = new List<string>();

    static List<string> functions = new List<string>();

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

    static int GetFunctionName(string line, int i, string tab)
    {
        int endOfName = line.Substring(i).IndexOf(' ');
        string str = line.Substring(i, endOfName);
        functions.Add(str);
        int bracket1 = line.IndexOf('(');
        int bracket2 = line.IndexOf(')');
        if (bracket1 == -1 || bracket2 == -1)
        {
            return -1;
        }
        GetParams(line.Substring(bracket1, bracket2 - bracket1), tab);
        Console.WriteLine(tab + "def " + str);
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
            } else
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
                if(i == item.Value)
                {
                    opWithPriority.Add(i); 
                    break;
                }
            }
        }

        if (op.Count == 0)
        {
            if (!IsNumber(line) && !variables.Contains(line))
            {
                variables.Add(line);
            }
            Console.WriteLine(tab + line);
        }
        else if (op.Count == 1 && op[0] == "=")
        {
            if (!IsNumber(val[1]) && !variables.Contains(val[1]))
            {
                return $"error {val[1]} is undefined";
            }
            Console.WriteLine(tab + val[1]);
            Console.WriteLine(tab + "    " + op[0]);
            Console.WriteLine(tab + val[0]);
            if (!variables.Contains(val[0]))
            {
                variables.Add(val[0]);
            }

        } else
        {
            if (op[0] == "=" && !variables.Contains(val[0]))
            {
                variables.Add(val[0]);
            }
            for (int i = 0; i < opWithPriority.Count; i++)
            {
                int index = op.IndexOf(opWithPriority[i]);
                if (val[index] != "null")
                {
                    if (!IsNumber(val[index]) && !variables.Contains(val[index]))
                    {
                        return $"error {val[index]} is undefined";
                    }
                    Console.WriteLine(tab + val[index]);
                }
                Console.WriteLine(tab + "    " + opWithPriority[i]);
                if (index + 1 < val.Count && val[index + 1] != "null")
                {
                    if (!IsNumber(val[index + 1]) && !variables.Contains(val[index + 1]))
                    {
                        return $"error {val[index + 1]} is undefined";
                    }
                    Console.WriteLine(tab + val[index + 1]);
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
                if (line[line.Length - 1] != ':')
                {
                    return $"error colon not found";
                }
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
                if (str != "return" && line[line.Length - 1] != ':')
                {
                    return $"error colon not found";
                }
                check = SyntaxLineAnalize(line.Substring(i + endOfLexeme + 1), tab, true);
                if (check.Length > 5 && check.Substring(0, 5) == "error")
                {
                    return check;
                }
                Console.WriteLine(tab + str);
                return tab;
            } else if (functions.Contains(str))
            {
                int bracket = line.IndexOf(')');
                if (bracket == -1)
                {
                    return $"error bracket not found";
                }
                check = SyntaxLineAnalize(line.Substring(i + endOfLexeme, bracket - i - endOfLexeme), tab, true);
                if (check.Length > 5 && check.Substring(0, 5) == "error")
                {
                    return check;
                }
                Console.WriteLine(tab + str);
                if (bracket != line.Length - 1)
                {
                    SyntaxLineAnalize(line.Substring(bracket + 1), tab, true);
                }
                return tab;
            } else if (specialWordsWithoutParams.ContainsValue(str))
            {
                Console.WriteLine(tab + str);
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

    public static async Task Main(string[] args)
    {
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
    }
}
