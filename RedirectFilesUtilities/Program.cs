﻿using static RedirectFilesUtilities.UsagePrinter;
using static RedirectFilesUtilities.GitUtilities;

namespace RedirectFilesUtilities
{
	public class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length <= 2)
			{
				Console.WriteLine("No arguments\n");
				PrintUsageOpenFile();
				PrintUsageCommit();
				PrintUsagePush();
				PrintUsageForcePush();
				PrintUsageUpdate();
				PrintUsageMerge();
				PrintUsageAddFile();
				Environment.Exit(-1);
			}

			string actionType = args[0];
			switch (actionType)
			{
				case "-open": //opening a file
					if (!OpenFileFromRedirect(args))
					{
						Console.WriteLine("Failed to open the file");
						Environment.Exit(-1);
					}
					break;

				case "-commit": //Commiting changes
					if (!CommitFileOrDirectory(args))
					{
						Console.WriteLine("Failed to commit");
						Environment.Exit(-1);
					}
					break;

				case "-push": //Pushing commit
					if (!PushCommit(args))
					{
						Console.WriteLine("Failed to push");
						Environment.Exit(-1);
					}
					break;
				case "-merge": //Merging to resolve conflicts
					if (!MergeSolver(args))
					{
						Console.WriteLine("Failed to resolve conflict");
						Environment.Exit(-1);
					}
					break;
				case "-update":
					if (!UpdateRepository(args))
					{
						Console.WriteLine("Failed to update repository");
						Environment.Exit(-1);
					}
					break;
				case "-forcePush":
					if (!ForcePush(args))
					{
						Console.WriteLine("Failed to force push");
						Environment.Exit(-1);
					}
					break;
				case "-add":
					if (!AddFile(args))
					{
						Console.WriteLine("Failed to add file to the repository");
						Environment.Exit(-1);
					}
					break;
				case "-comp":
					if (!OpenFileFromReal(args))
					{
						Console.WriteLine("Failed to launch the compiler process");
						Environment.Exit(-1);
					}
					break;
				case "-config":
					if (!WriteConfigFile(args))
					{
						Console.WriteLine("Failed to write the configuration file");
						Environment.Exit(-1);
					}
					break;
				default:
					PrintUsageOpenFile();
					PrintUsageCommit();
					PrintUsagePush();
					PrintUsageForcePush();
					PrintUsageUpdate();
					PrintUsageMerge();
					PrintUsageAddFile();
					PrintUsageCompiler();
					Environment.Exit(-1);
					break;
			}
		}
	}
}