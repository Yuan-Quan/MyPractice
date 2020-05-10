﻿using System;
using System.IO;
using CommandDotNet;
using ConsoleTables;
using GoPractice.MyUtil;
using System.Collections.Generic;

namespace GoPracticeCli
{
    class Program
    {
        static void Startup()
        {
            System.Console.WriteLine("gpcli: ");
            System.Console.WriteLine();
        }
        static int Main(string[] args)
        {
            Startup();
            return new AppRunner<MainEntry>()
                .UseDefaultMiddleware()
                .Run(args);
            
            //return 0;
        }
    }



    public class MainEntry
    {
        //to store the console color when color changes
        ConsoleColor preForegroundColor;

        //format of date time
        const string dataFmt = "{0,-30}{1}";

        [Command(Name = "new",
        Usage = "new [date]",
        Description = "creat a new report",
        ExtendedHelpText = "creat a new report,\nspecify [date] to customize the file name.\ncustom template or file path not implemented")]
        [Obsolete]
        public void CreatNewReport(
            [Option(ShortName = "d")]string date = null
            )
        {
            if (date == null)
            {
                Console.WriteLine("No date specifyed, using current date");
                //maybe print the current timezone 
                date = MyUtil.GetDateString(DateTime.Now);
            }
            
            Console.WriteLine($"new record will be named {date}.md");
            Console.WriteLine();
            // Get the local time zone and the current local time and year.
            var localZone = TimeZone.CurrentTimeZone;
            DateTime currentDate = DateTime.Now;
            Console.WriteLine("Your current time zone set to:");
            Console.WriteLine(dataFmt, "UTC offset:", localZone.GetUtcOffset(currentDate));
            Console.WriteLine();

            if (File.Exists(@$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/{date}.md"))
            {
                //file already exists

                Console.Write($"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/templates.md already exitsts, ");

                Console.WriteLine("\n");
                preForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[1]SKIP:");
                Console.ForegroundColor = preForegroundColor;
                Console.Write(" break this opreation.");
                Console.WriteLine();

                preForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[2]DELETE:");
                Console.ForegroundColor = preForegroundColor;
                Console.Write(" will delete it.");
                Console.WriteLine();

                preForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[3]OVERWRITE:");
                Console.ForegroundColor = preForegroundColor;
                Console.Write(" will delete it, then creat a new one.");
                Console.WriteLine();

                while (true)
                {

                    Console.Write("Select a option -> ");

                    if (Int32.TryParse(Console.ReadLine(), out int result))
                    {
                        switch (result)
                        {
                            case 2:
                                DeleteExist();
                                return;
                            case 1:
                                Console.WriteLine("will stop opreation 'new'.");
                                return;
                            case 3:
                                DeleteExist();
                                GenerateReport();
                                return;
                            default:
                                Console.WriteLine("No this option!!");
                                continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Not a valid input!!");
                        continue;
                    }

                }
            }
            else
            {
                //no exist file

                GenerateReport();

                return;
            }            

            //Delete duplicate file
            void DeleteExist()
            {
                Console.Write($"Will ");
                preForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("DELETE");
                Console.ForegroundColor = preForegroundColor;
                Console.Write(@$" {MyUtil.ReadSetting("path").Split(',')[0]}/src/records/{date}.md");
                Console.WriteLine();

                File.Delete(@$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/{date}.md");

                preForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Done!");
                Console.ForegroundColor = preForegroundColor;
                Console.WriteLine();
            }

            //New a report
            void GenerateReport()
            {
                try
                {
                    File.Copy(@$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/templates/DailyReport.md", @$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/{date}.md");
                }
                catch (DirectoryNotFoundException)
                {
                    preForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("WRONG PATH!! Have you set the right path in app.config???");
                    Console.ForegroundColor = preForegroundColor;
                    Console.WriteLine();
                    throw;
                }


                ConsoleColor preConsoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"File {date}.md generated successfully");
                Console.ForegroundColor = preConsoleColor;
                Console.WriteLine();
            }
        }

        [Command(Name = "config",
        Usage = "config [opration] [key] [value]\nexample: config set path ~/GoPractice",
        Description = "view change/add settings",
        ExtendedHelpText = "opration: view set add remove\nformat: [opration] key value,description")]
        public void Config(
            string opration = null,
            string k = null,
            string v = null
            )
        {
            switch (opration)
            {
                case "v":
                case "view":
                    ConfigView(k);
                    break;
                case "set":
                case "s":
                    ConfigSet(k, v);
                    break;
                case "add":
                case "a":
                    ConfigAdd(k, v);
                    break;
                case "remove":
                case "rm":
                    ConfigRemove(k, v);
                    break;
                default:
                    Console.WriteLine("Usage: config [opration] [key] [value]\nexample: config set path ~/GoPractice");
                    break;
            }

            static void ConfigRemove(string k, string v)
            {
                Console.WriteLine("Too lazy to write it. Just goto App.config and delete it.");
                throw new NotImplementedException();
            }

            void ConfigAdd(string k, string v)
            {
                MyUtil.AddUpdateAppSettings(k, v);
                Console.WriteLine();
                preForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration added successfully!!");
                Console.ForegroundColor = preForegroundColor;
            }

            void ConfigSet(string k, string v)
            {
                MyUtil.AddUpdateAppSettings(k, v);
                Console.WriteLine();
                preForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration modified successfully!!");
                Console.ForegroundColor = preForegroundColor;
            }

            static void ConfigView(string k)
            {
                if (k == null)
                {
                    //view all appsettings
                    var table = new ConsoleTable("key", "value", "description");
                    foreach (var setting in MyUtil.GetAllSettings())
                    {
                        table.AddRow(setting.Key, setting.Value, setting.Description);
                    }

                    Console.WriteLine();
                    Console.WriteLine(" Here all your configurations in App.config:");
                    table.Write();
                    Console.WriteLine();
                }
            }
        }

        [Command(Name = "select",
        Usage = "select [file name]",
        Description = "select working file",
        ExtendedHelpText = "select working file")]
        public void Select(
            string fileName = null
            )
        {
            switch (fileName)
            {
                case "today":
                case null:
                    Console.WriteLine("Selecting today's record");
                    fileName = MyUtil.GetDateString(DateTime.Now);
                    break;
                default:
                    if (File.Exists($@"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/{fileName}.md"))
                    {
                        break;
                    }

                    Console.WriteLine($"No record named {fileName}.md. Have you type the right name?");
                    return;
            }

            fileName += ".md";
            Console.WriteLine($"\nWorking on file: {fileName}\n");

            MyUtil.AddUpdateAppSettings("WorkingOn", fileName + ",Current working file");
        }

        [Command(Name = "edit",
        Usage = "edit [option] [paramter]",
        Description = "todo add delete",
        ExtendedHelpText = "edit file")]
        public void EditFile(
        [Operand(Name = "opration",
        Description = "What do you want to do? editTODO, add, delete, attach")]
        string opr,
        [Option(LongName = "file", ShortName = "f",
        Description = "file you want to attach")]
        string fileAtch = null,
        [Option(LongName = "addline", ShortName = "l",
        Description = "string you want to attach")]
        string addStr = null
            )
        {
            Console.WriteLine("Will edit current file");
            var file = MyUtil.ReadSetting("WorkingOn").Split(',')[0];
            switch (opr)
            {
                case "TODO":
                case "todo":
                    CheckTODO(file);
                    break;
                case "a":
                case "add":
                case "ADD":
                case "Add":
                    AddAString(addStr);
                    break;
                case "d":
                case "delete":
                case "DELETE":
                case "Delete":
                    DeleteLastString();
                    break;
                case "attach":
                case "ATTACH":
                case "Attach":
                    if (fileAtch == null)
                    {
                        Console.WriteLine();
                        var preForegroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("You must specify a file path when attach a file");
                        Console.ForegroundColor = preForegroundColor;
                        System.Console.WriteLine(
                            "\nYou can add '-f ' then drag and dorp the file into your terminal!!"
                        );
                    }
                    if(!File.Exists(fileAtch))
                    {
                        var preForegroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No file found in given path");
                        Console.ForegroundColor = preForegroundColor;
                        return;
                    }
                    AttachFile(fileAtch);
                    break;
                default:
                System.Console.WriteLine();
                System.Console.WriteLine("unknow opration");
                break;
            }

            void AttachFile(string path)
            {
                var fileType = MyUtil.GetFileType(path);
                switch (fileType)
                {
                    case FileType.image:
                        AttachImage(path, @$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/images/{MyUtil.GetSHA1Hash(path)}{path.Substring(path.LastIndexOf('.'), path.Length - path.LastIndexOf('.'))}");
                        break;
                    case FileType.audio:
                        AttachAudio(path, @$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/audio/{MyUtil.GetSHA1Hash(path)}{path.Substring(path.LastIndexOf('.'), path.Length - path.LastIndexOf('.'))}");
                        break;
                    case FileType.video:
                        throw new NotImplementedException("");
                        AttachVideo(path, @$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/video/{MyUtil.GetSHA1Hash(path)}{path.Substring(path.LastIndexOf('.'), path.Length - path.LastIndexOf('.'))}");
                        break;
                    default:
                        return;
                }
            }

            void AttachAudio(string pathOrg, string pathDst)
            {
                if (File.Exists(pathDst))
                {
                    var preForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("File already existed!! will abort this action");
                    Console.ForegroundColor = preForegroundColor;
                    return;
                }
                File.Copy(pathOrg, pathDst);
                AddAString("  ");
                AddAString($"[__AUDIO__](../audio/{pathDst.Substring(pathDst.IndexOf("src")+3)})");
            }

            void AttachImage(string pathOrg, string pathDst)
            {
                if (File.Exists(pathDst))
                {
                    var preForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("File already existed!! will abort this action");
                    Console.ForegroundColor = preForegroundColor;
                    return;
                }
                File.Copy(pathOrg, pathDst);
                AddAString("  ");
                AddAString($"![altText](../audio/{pathDst.Substring(pathDst.IndexOf("src")+3)})");
            }

            void AttachVideo(string pathOrg, string pathDst)
            {
                if (File.Exists(pathDst))
                {
                    var preForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("File already existed!! will abort this action");
                    Console.ForegroundColor = preForegroundColor;
                    return;
                }
                File.Copy(pathOrg, pathDst);
                AddAString("  ");
                AddAString($"[__AUDIO__](../audio/{pathDst.Substring(pathDst.IndexOf("src")+3)})");
            }
            
            void DeleteLastString()
            {
                var s = new List<string>(MyUtil.ReadFrom($@"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/" + file));
                s.RemoveAt(s.Count - 1);
                MyUtil.WriteAFile(s, @$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/", file);
            }

            void AddAString(string str)
            {
                var s = new List<string>(MyUtil.ReadFrom($@"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/"+file));
                str = str.Replace("\n", "  ");
                s.Add(str);
                MyUtil.WriteAFile(s, @$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/", file);
            }

            void CheckTODO(string fileName)
            {
                Console.WriteLine();
                Console.WriteLine($"Working on {fileName}...");
                Console.WriteLine();
                var s = new List<string>();
                foreach (var item in MyUtil.TODOEdit(fileName))
                {
                    s.Add(item);
                }
                
                //foreach (var item in s)
                //{
                //    System.Console.WriteLine(item);
                //}                
                MyUtil.WriteAFile(s, @$"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/", fileName);
            }

        }

        [Command(Name = "cat",
        Usage = "cat [file name]",
        Description = "print a report",
        ExtendedHelpText = "just like cat command in bash")]
        public void Cat(
            string file = null   
            )
        {
            if (file == null)
            {
                Console.WriteLine();
                Console.WriteLine("No file specified, Will read current file");
                file = MyUtil.ReadSetting("WorkingOn").Split(',')[0];
            }
            Console.WriteLine();
            
            var fl = new List<string>(); 
            foreach (var line in MyUtil.ReadFrom($@"{MyUtil.ReadSetting("path").Split(',')[0]}/src/records/"+file))
            {
                System.Console.WriteLine(line);
            }

        }

        [Command(Name = "link",
        Usage = "link [file name] [date]",
        Description = "generate link to report in README",
        ExtendedHelpText = "-d to set date")]
        public void LinkAReport(
            [Option(LongName = "file", ShortName = "f",
            Description = "file you want to link")]
            string file = null,
            [Option(LongName = "date", ShortName = "d",
            Description = "coreesponding date of record file")]
            string date = null
        )
            {
                DateTime dt = DateTime.Today;
                System.Console.WriteLine();
                if (file == null)
                {
                    System.Console.WriteLine("No file specified, will use selected file:");
                    var preForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine(MyUtil.ReadSetting("WorkingOn").Split(',')[0]);
                    Console.ForegroundColor = preForegroundColor;
                    file = MyUtil.ReadSetting("WorkingOn").Split(',')[0];
                }
                System.Console.WriteLine();
                bool isGetDateFailed = false;
                System.Console.WriteLine("Try to get date automaticly");
                try
                {
                    dt = MyUtil.DtStrToDt(file);
                }catch{
                    var preForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Cannot get date,");
                    Console.ForegroundColor = preForegroundColor;
                    isGetDateFailed = true;
                }

                if(isGetDateFailed)
                {
                    bool f1, f2, f3;
                    bool isDateSetFaild;
                    //bool isTryPraseFaild;
                    while (true)
                    {
                        isDateSetFaild= false;
                        System.Console.WriteLine();
                        System.Console.WriteLine("Enter the date, yyyy/mm/dd");
                        System.Console.Write("> ");
                        var entry = Console.ReadLine();
                        try
                        {
                            Int32.TryParse(entry.Split('/')[0], out int ty);
                            Int32.TryParse(entry.Split('/')[1], out int tm);
                            Int32.TryParse(entry.Split('/')[2], out int td);
                        }
                        catch
                        {
                            System.Console.WriteLine("Format err, try again");
                            continue;
                        }

                        f1 = Int32.TryParse(entry.Split('/')[0], out int y);
                        f2 = Int32.TryParse(entry.Split('/')[1], out int m);
                        f3 = Int32.TryParse(entry.Split('/')[2], out int d);
                        
                        if (f1&&f2&&f3)
                        {
                            try
                            {
                                dt = new DateTime(y,m,d);
                            }catch
                            {
                                isDateSetFaild= true;
                            }  
                        }else
                        {
                            System.Console.WriteLine("Format err, try again");
                            continue;
                        }

                        if (isDateSetFaild)
                        {
                            System.Console.WriteLine("Date incorrect!!");
                            continue;
                        }

                        break;
                    }
                }

                System.Console.WriteLine(dt.ToShortDateString());
                throw new NotImplementedException();
            }
    }
}
