using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using CsvHelper;
using woanware;

namespace javaidx
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                AssemblyName assemblyName = assembly.GetName();

                Console.WriteLine(Environment.NewLine + "javaidx v" + assemblyName.Version.ToString(3) + Environment.NewLine);

                Options options = new Options();
                if (CommandLineParser.Default.ParseArguments(args, options) == false)
                {
                    return;
                }

                if (options.Directory.Length == 0 & options.File.Length == 0)
                {
                    Console.WriteLine("Either the file (-f) or directory (-d) parameter value must be set");
                    return;
                }

                if (options.Directory.Length > 0 & options.File.Length > 0)
                {
                    Console.WriteLine("The file (-f) and directory (-d) parameters cannot be set at the same time");
                    return;
                }

                if (options.File.Length > 0)
                {
                    if (File.Exists(options.File) == false)
                    {
                        Console.WriteLine("The IDX file does not exist");
                        return;
                    }

                    CacheEntry cacheEntry = new CacheEntry(options.File);
                    if (cacheEntry.Error == true)
                    {
                        Console.WriteLine("Unable to parse IDX file");
                        return;
                    }

                    PrintCacheEntries(options, new List<CacheEntry>{cacheEntry});
                }
                else
                {
                    List<CacheEntry> cacheEntries = new List<CacheEntry>();
                    foreach (string file in Directory.EnumerateFiles(options.Directory, "*.idx", SearchOption.AllDirectories))
                    {
                        CacheEntry cacheEntry = new CacheEntry(file);
                        if (cacheEntry.Error == true)
                        {
                            Console.WriteLine("Unable to parse IDX file: " + file);
                            continue;
                        }

                        cacheEntries.Add(cacheEntry);
                    }

                    PrintCacheEntries(options, cacheEntries);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static CsvWriterOptions SetCsvWriterConfig(Options options)
        {
            CsvHelper.CsvWriterOptions csvWriterOptions = new CsvWriterOptions();

            try
            {
                switch (options.Delimiter)
                {
                    case "'\\t'":
                        csvWriterOptions.Delimiter = '\t';
                        break;
                    case "\\t":
                        csvWriterOptions.Delimiter = '\t';
                        break;
                    default:
                        csvWriterOptions.Delimiter = char.Parse(options.Delimiter);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to set delimiter. Defaulting to comma \",\": " + ex.Message);
                csvWriterOptions.Delimiter = ',';
            }

            return csvWriterOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cacheEntries"></param>
        /// <returns></returns>
        private static IEnumerable<CacheEntry> GetSortedCacheEntries(Options options, List<CacheEntry> cacheEntries)
        {
            switch (options.Sort.ToLower())
            {
                case "expiration":
                    return from c in cacheEntries orderby c.ExpirationDate select c;
                case "url":
                    return from c in cacheEntries orderby c.Url select c;
                case "validation":
                    return from c in cacheEntries orderby c.ValidationTimestamp select c;
                case "modified":
                    return from c in cacheEntries orderby c.LastModified select c;
                case "length":
                    return from c in cacheEntries orderby c.ContentLength select c;
                default:
                    return from c in cacheEntries orderby c.Url select c;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="hits"></param>
        /// <returns></returns>
        private static bool IsValidSortValue(Options options)
        {
            switch (options.Sort.ToLower())
            {
                case "expiration":
                    return true;
                case "url":
                    return true;
                case "validation":
                    return true;
                case "modified":
                    return true;
                case "length":
                    return true;
                case "":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cacheEntries"></param>
        private static void PrintCacheEntries(Options options, List<CacheEntry> cacheEntries)
        {
            try
            {
                CsvHelper.CsvWriterOptions csvWriterOptions = SetCsvWriterConfig(options);

                using (MemoryStream memoryStream = new MemoryStream())
                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                using (CsvHelper.CsvWriter csvWriter = new CsvHelper.CsvWriter(streamWriter, csvWriterOptions))
                {
                    // Write out the file headers
                    csvWriter.WriteField("File Path");
                    csvWriter.WriteField("File Name");
                    csvWriter.WriteField("URL");
                    csvWriter.WriteField("Headers");
                    csvWriter.WriteField("Content Length");
                    csvWriter.WriteField("Modified");
                    csvWriter.WriteField("Expiration");
                    csvWriter.WriteField("Validation");
                    csvWriter.NextRecord();

                    var sorted = GetSortedCacheEntries(options, cacheEntries);

                    StringBuilder text = new StringBuilder();

                    foreach (CacheEntry cacheEntry in sorted)
                    {
                        text.AppendFormat("File Path: {0}{1}", cacheEntry.FilePath, Environment.NewLine);
                        text.AppendFormat("File Name: {0}{1}", cacheEntry.FileName, Environment.NewLine);
                        text.AppendFormat("URL: {0}{1}", cacheEntry.Url, Environment.NewLine);
                        text.AppendFormat("Headers: {1}{0}", cacheEntry.HeadersText, Environment.NewLine);
                        text.AppendFormat("Content Length: {0}{1}", cacheEntry.ContentLength, Environment.NewLine);
                        text.AppendFormat("Modified: {0}{1}", cacheEntry.LastModified.ToShortDateString() + " " + cacheEntry.LastModified.ToShortTimeString(), Environment.NewLine);
                        text.AppendFormat("Expiration: {0}{1}", cacheEntry.ExpirationDate.ToShortDateString() + " " + cacheEntry.ExpirationDate.ToShortTimeString(), Environment.NewLine);
                        text.AppendFormat("Validation: {0}{1}", cacheEntry.ValidationTimestamp.ToShortDateString() + " " + cacheEntry.ValidationTimestamp.ToShortTimeString(), Environment.NewLine);
                        text.AppendLine(string.Empty);

                        csvWriter.WriteField(cacheEntry.FilePath);
                        csvWriter.WriteField(cacheEntry.FileName);
                        csvWriter.WriteField(cacheEntry.Url);
                        csvWriter.WriteField(cacheEntry.HeadersText);
                        csvWriter.WriteField(cacheEntry.ContentLength);
                        csvWriter.WriteField(cacheEntry.LastModified.ToShortDateString() + " " + cacheEntry.LastModified.ToShortTimeString());
                        csvWriter.WriteField(cacheEntry.ExpirationDate.ToShortDateString() + " " + cacheEntry.ExpirationDate.ToShortTimeString());
                        csvWriter.WriteField(cacheEntry.ValidationTimestamp.ToShortDateString() + " " + cacheEntry.ValidationTimestamp.ToShortTimeString());
                        csvWriter.NextRecord();
                    }

                    string output = string.Empty;
                    memoryStream.Position = 0;
                    using (StreamReader streamReader = new StreamReader(memoryStream))
                    {
                        output = streamReader.ReadToEnd();
                    }

                    Console.Write(text);

                    if (options.Output.Length > 0)
                    {
                        string ret = IO.WriteTextToFile(output, options.Output, false);
                        if (ret.Length > 0)
                        {
                            Console.Write("An error occurred whilst outputting the CSV/TSV file: " + ret);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
