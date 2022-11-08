using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using static RedirectFilesUtilities.UsagePrinter;
using static RedirectFilesUtilities.GitUtilities;
using static System.Net.WebRequestMethods;

namespace RedirectFilesUtilities
{
	public class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length <= 2)
			{
				Console.WriteLine("No arguments");
				PrintUsageOpenFile();
				PrintUsageCommit();
				PrintUsagePush();
				return;
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
				default:
					PrintUsageOpenFile();
					PrintUsageCommit();
					PrintUsagePush();
					PrintUsageUpdate();
					PrintUsageMerge();
					Environment.Exit(-1);
					break;
			}
		}
	}
}