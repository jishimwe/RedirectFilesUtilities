﻿using LibGit2Sharp;

using static RedirectFilesUtilities.GitUtilities;
using static RedirectFilesUtilities.UsagePrinter;

namespace RedirectFilesUtilities
{
	internal class CompilerUtilities
	{
		public static readonly string RedirectRepositoryUrl = "RedirectRepositoryUrl",
			RedirectDirectoryPath = "RedirectDirectoryPath",
			RealRepositoryUrl = "RealRepositoryUrl",
			RealRepositoryPath = "RealRepositoryPath",
			Username = "Username",
			Mail = "Mail",
			TokenPath = "TokenPath",
			BranchName = "BranchName",
			RemoteName = "RemoteName",
			RefSpecs = "RefSpecs",
			Filepath = "Filepath",
			Message = "Message",
			RedirectFile = "RedirectFile",
			MergeOptions = "MergeOptions";

		public static IDictionary<string, string> ReadConfig(string configPath)
		{
			IDictionary<string, string> config = new Dictionary<string, string>();
			if (File.Exists(configPath))
			{
				StreamReader file = new StreamReader(configPath);
				string? ln = file.ReadLine();
				while ((ln = file.ReadLine()) != null)
				{
					if (string.IsNullOrEmpty(ln) || string.IsNullOrWhiteSpace(ln))
					{
						continue;
					}
					string[] stabs = ln.Split(' ');
					if (stabs.Length == 3)
					{
						config.Add(stabs[0], stabs[2]);
					}
				}
				file.Close();
				return config;
			}

			return null;
		}
		 
		public static bool CompilerCall(string[] arguments)
		{
			IDictionary<string, string> args = ReadConfig(arguments[2]);
			Repository repository = new Repository(args[RealRepositoryPath]);
			using StreamReader sr = new StreamReader(arguments[4]);
			try
			{
				string? path = sr.ReadLine();
				if (path == null)
				{
					Console.WriteLine("File not found");
					return false;
				};
				String realPath = Path.Combine(args[RealRepositoryPath], path);
				if (File.Exists(realPath))
				{
					Console.WriteLine(realPath);
					return true;
				}

				List<string> ls = new List<string>() { path };

				CheckoutOptions co = new CheckoutOptions();
				co.CheckoutModifiers = CheckoutModifiers.Force;
				repository.CheckoutPaths(args[BranchName], ls, co);

				Console.WriteLine(realPath);
				
				return true;
			}
			catch (Exception e)
			{
				ConflictCollection cc = repository.Index.Conflicts;
				if (cc.Any())
				{
					Console.WriteLine(cc.Names + "\t " + cc.First().Ours + "\t " + cc.First().Theirs);
					PrintUsageConflicts(cc);
					PrintUsageMerge();
					Environment.Exit(-2);
				}
				Console.WriteLine(e.Message);
			}
			return false;
		}

	}
}