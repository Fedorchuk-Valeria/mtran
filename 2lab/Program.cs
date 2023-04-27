using System;

public static class Program
{
    static Dictionary<int, string> operators = new Dictionary<int, string>()
    {
        {1, "+"},
        {2, "-"},
        {3, "/"},
        {4, "*"},
        {5, "**"},
        {6, "%"},
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

    static Dictionary<int, string> specialWords = new Dictionary<int, string>()
    {
        {1, "while"},
        {2, "if"},
        {3, "elif"},
        {4, "else"},
        {5, "break"},
        {6, "continue"},
        {7, "for"},
        {8, "range"},
        {9, "def"},
        {10, "lambda"},
        {11, "return"}
    };

    static List<string> variables = new List<string>();

    static List<string> functions = new List<string>();

    public static string path = "script.txt";

    static int IndentationСheck(string line)
    {
        int i = 0;
        while (i < line.Length)
        {
            if (line[i] == ' ')
            {
                i++;
            }
            else
            {
                break;
            }
        }
        return i;
    }

    static string GetDictionaryKey(string value, Dictionary<int, string> dict)
    {
        foreach (var item in dict)
        {
            if(Math.Abs(item.Value.Length - value.Length) > 2)
            {
                continue;
            }
            int count = item.Value.Length > value.Length ? item.Value.Length : value.Length;
            int len = count;
            for (int i = 0; i < len; i++)
            {
                if (i < item.Value.Length && i < value.Length && item.Value[i] == value[i])
                {
                    count--;
                }
            }
            if (count == 0)
            {
                return item.Key.ToString();
            }
            else if(count < len - 1)
            {
                return value + "? may be " + item.Value;
            }
        }
        return "-1";
    }

    static string IsBool(string value)
    {
        if (value == "True" || value == "False")
        {
            return value + "\tconst bool";
        }
        return "not bool"; 
    }

    static string IsString(string value)
    {
        if(value.IndexOf('"') == value.LastIndexOf('"'))
        {
            return "not string";
        }
        return value + "\tconst string";
    }

    static string GetNumberType(string num)
    {
        bool result = int.TryParse(num, out var number);
        if (result)
        {
            return "int";
        }
        result = float.TryParse(num, out var number2);
        if (result)
        {
            return "float";
        }
        return "not num";
    }

    static void GetFunctionName(string line, int i)
    {
        int endOfName = line.Substring(i).IndexOf(' ');
        string str = line.Substring(i, endOfName);
        functions.Add(str);
    }

    static string IsFunc(string name)
    {
        foreach(var item in functions)
        {
            if(item == name)
            {
                return item + "\t function name";
            }
        }
        return "not function";
    }

    static string GetLexemeType(string lexeme)
    {
        if(lexeme.IndexOf(',') != -1)
        {
            lexeme = lexeme.Remove(lexeme.IndexOf(','));
        }

        if (lexeme.IndexOf(':') != -1)
        {
            lexeme = lexeme.Remove(lexeme.IndexOf(':'));
        }

        if(lexeme == "")
        {
            return "";
        }

        string key = GetDictionaryKey(lexeme, operators);
        if (key != "-1" && int.TryParse(key, out var number))
        {
            string result = lexeme + "      " + operators[number] + " operator";
            return result;
        }
        else if(key != "-1")
        {
            return key;
        }

        key = GetDictionaryKey(lexeme, specialWords);
        if (key != "-1" && int.TryParse(key, out number))
        {
            string result = lexeme + "\t" + specialWords[number] + " special word";
            return result;
        }
        else if (key != "-1")
        {
            return key;
        }

        string numberType = GetNumberType(lexeme);
        if(numberType != "not num")
        {
            string result = lexeme + "\t" + "const " + numberType ;
            return result;
        }

        string isbool = IsBool(lexeme);
        if (isbool != "not bool")
        {
            return isbool;
        }

        string isString = IsString(lexeme);
        if (isString != "not string")
        {
            return isString;
        }

        string isFunc = IsFunc(lexeme);
        if (isFunc != "not function")
        {
            return isFunc;
        }

        variables.Add(lexeme);
        return lexeme + "\tvariable";
    }


    public static async Task Main(string[] args)
    {
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (i == 0 && line[i] == ' ')
                        {
                            i = IndentationСheck(line);
                            if (i < 4)
                            {
                                return;
                            }
                        }
                        int endOfLexeme = line.Substring(i).IndexOf(' ');
                        if (endOfLexeme == -1)
                        {
                            endOfLexeme = line.Length - i;
                        }
                        string str = line.Substring(i, endOfLexeme);
                        if (str == "def")
                        {
                            GetFunctionName(line, i + endOfLexeme + 1);
                        }
                        Console.WriteLine(GetLexemeType(str));
                        i += endOfLexeme;
                    }
                }
            }
        }
        catch
        {
            Console.WriteLine("Wrong input");
        }
        
    }
}


