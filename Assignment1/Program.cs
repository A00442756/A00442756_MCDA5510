using System;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using log4net;

namespace Assignment1
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("Info message");
            var watch = System.Diagnostics.Stopwatch.StartNew(); //begin timer
            
            DirWalker fw = new DirWalker();
            fw.walk(@"C:\Users\tempSomto\OneDrive - Saint Marys University\MCDA\5510\my_projects\MCDA5510_Assignments\MCDA5510_Assignments\Sample Data");
            
            watch.Stop(); //stop timer

            log.Info($"Execution Time: {watch.ElapsedMilliseconds} ms");
            
            var validRows = File.ReadAllLines(@".\Output\completeRecords.csv");
            int validRowsCount = validRows.Length;
            log.Info("Total valid rows: " + validRowsCount);

            var skippedRows = File.ReadAllLines(@".\Output\skippedRows.csv");
            int skippedRowsCount = skippedRows.Length;
            log.Info("Total skipped rows: " + skippedRowsCount);

        }
    }
   
    public class DirWalker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void walk(String path)
        {

            string[] list = Directory.GetDirectories(path);


            if (list == null) return;

            foreach (string dirpath in list)
            // foreach (String f : list)
            {
                //log.Info("Directory: " + dirpath);
                if (Directory.Exists(dirpath))
                {
                    walk(dirpath);
                    log.Info("Directory: " + dirpath);
                }
            }
            string[] fileList = Directory.GetFiles(path);
            foreach (string filepath in fileList)
            {
                log.Info("File: " + filepath);
                using (TextFieldParser parser = new TextFieldParser(filepath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    int rowCount = 1;
                    while (!parser.EndOfData)
                    {
                        //Process row
                        string[] fields = parser.ReadFields();
                        if (rowCount == 1)
                        {
                            rowCount++;
                            continue;  //skip header
                        }

                        List<string> completeRecordsList = new List<string>();
                        //string[] fields = parser.ReadFields();
                        foreach (string field in fields)
                        {
                            //TODO: Process field
                            if (field.Length < 1)
                            {
                                log.Info($"Incomplete records. Skipped row {rowCount++} in file {filepath}");
                                break;
                            }
                            //if (field.StartsWith("First Name")) break;
                            completeRecordsList.Add(field);
                            rowCount++;
                        }

                        string[] completeRecordsArray = completeRecordsList.ToArray();
                        Exceptions ex = new Exceptions();
                        if (completeRecordsArray.Length == 10)
                        {
                            var sw = ex.OpenStream(@".\Output\completeRecords.csv");

                            if (sw is null)
                            {
                                log.Info("sw is null");
                                return;
                            }
                            string completeRecordString = string.Join(',', completeRecordsArray);
                            sw.WriteLine(completeRecordString);
                            sw.Close();
                        }
                        else
                        {
                            var sw = ex.OpenStream(@".\Output\skippedRows.csv");

                            if (sw is null)
                            {
                                log.Info("sw is null");
                                return;
                            }
                            string skippedRows = string.Join(',', completeRecordsArray);
                            sw.WriteLine(skippedRows);
                            sw.Close();
                        }
                    }
                }
            }
        }

        

    }

    public class Exceptions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public StreamWriter OpenStream(string path)
        {
            if (path is null)
            {
                log.Error("You did not supply a file path.");
                return null;
            }

            try
            {
                var fs = new FileStream(path, FileMode.Append);
                return new StreamWriter(fs);
            }
            catch (FileNotFoundException)
            {
                log.Error("The file or directory cannot be found.");
            }
            catch (DirectoryNotFoundException)
            {
                log.Error("The file or directory cannot be found.");
            }
            catch (DriveNotFoundException)
            {
                log.Error("The drive specified in 'path' is invalid.");
            }
            catch (PathTooLongException)
            {
                log.Error("'path' exceeds the maxium supported path length.");
            }
            catch (UnauthorizedAccessException)
            {
                log.Error("You do not have permission to create this file.");
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32)
            {
                log.Error("There is a sharing violation.");
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 80)
            {
                log.Error("The file already exists.");
            }
            catch (IOException e)
            {
                log.Error($"An exception occurred:\nError code: " +
                                  $"{e.HResult & 0x0000FFFF}\nMessage: {e.Message}");
            }
            return null;
        }

    }

}
