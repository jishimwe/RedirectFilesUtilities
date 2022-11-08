﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using static RedirectFilesUtilities.UsagePrinter;

namespace RedirectFilesUtilities
{
    public static class GitUtilities
    {
		public static bool PushCommit(string[] args, bool force = false)
		{
			string dir = "", username = "", tokenFile = "",
				mail = "jeanpaulishimwe@gmail.com",
				refSpecs = @"refs/heads/master";
			if (force)
				refSpecs = "+" + refSpecs; 
			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-d":
						dir = args[i + 1];
						break;
					case "-t":
						tokenFile = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					case "-e":
						mail = args[i + 1];
						break;
					default:
						PrintUsagePush();
						return false;
				}
			}

			if (InvalidArguments(dir, username, tokenFile))
			{
				PrintUsagePush();
				return false;
			}

			Repository repository = new(dir);

			PushOptions opt = new PushOptions
			{
				CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials { Username = username, Password = GetToken(tokenFile) }
			};

			// TODO: Careful with hardcoded values
			Remote remote = FetchRemote("", repository);

			try
			{
				repository.Network.Push(remote, refSpecs, opt);
			}
			catch (NonFastForwardException nfe)
			{
				Console.WriteLine(nfe.Message);
				Console.WriteLine("Some files may need updating, try updating before pushing");
			}

			return true;
		}

		public static bool ForcePush(string[] args)
		{
			return PushCommit(args, true);
		}

		public static bool CommitFileOrDirectory(string[] args)
		{
			string filepath = "", rootPath = "", username = "", message = "", email = "";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-f":
						filepath = args[i + 1];
						break;
					case "-d":
						rootPath = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					case "-e":
						email = args[i + 1];
						break;
					case "-m":
						message = args[i + 1];
						break;
					default:
						PrintUsageCommit();
						return false;
				}
			}

			if (InvalidArguments(filepath, rootPath, username, message, email))
			{
				PrintUsageCommit();
				return false;
			}

			if (File.Exists(Path.Combine(rootPath, filepath)))
			{
				return CommitFile(filepath, rootPath, username, email, message);
			}
			return CommitDirectory(filepath, rootPath, username, email, message);
		}

		// Obsolete?
		public static bool CommitFile(string[] args)
		{
			string filepath = "", rootPath = "", username = "", message = "", email = "";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-f":
						filepath = args[i + 1];
						break;
					case "-d":
						rootPath = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					case "-e":
						email = args[i + 1];
						break;
					case "-m":
						message = args[i + 1];
						break;
					default:
						PrintUsageCommit();
						return false;
				}
			}

			if (InvalidArguments(filepath, rootPath, username, message, email))
			{
				PrintUsageCommit();
				return false;
			}

			Repository repository = new(rootPath);
			repository.Index.Add(filepath);
			repository.Index.Write();

			RemoveFromStaging(repository); // Removing unavailable files from staging
			Commands.Stage(repository, filepath);

			Signature author = new(username, email, DateTimeOffset.Now);
			Commit commit = repository.Commit(message, author, author);

			return true;
		}

		public static bool CommitDirectory(string dirpath, string rootPath, string username, string email, string message)
		{
			Repository repository = new Repository(rootPath);
			RemoveFromStaging(repository);

			CommitDirectoryHelper(dirpath, repository);

			Signature author = new(username, email, DateTimeOffset.Now);
			Commit commit = repository.Commit(message, author, author);

			return true;
		}

		private static bool CommitDirectoryHelper(string dirpath, Repository repository, string? pathSoFar = null)
		{
			DirectoryInfo di = new DirectoryInfo(dirpath);

			try
			{
				FileInfo[] files = di.GetFiles();
				foreach (FileInfo fi in files)
				{
					string relPath = pathSoFar == null ? fi.Name : Path.Combine(pathSoFar, fi.Name);
					repository.Index.Add(relPath);
					repository.Index.Write();
					Commands.Stage(repository, relPath);
				}

				DirectoryInfo[] dirs = di.GetDirectories();
				foreach (DirectoryInfo d in dirs)
				{
					string path = pathSoFar == null ? d.Name : Path.Combine(pathSoFar, d.Name);
					if (!CommitDirectoryHelper(d.Name, repository, path)) return false;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			return true;
		}

		public static bool CommitFile(string filepath, string rootPath, string username, string email, string message)
		{
			Repository repository = new(rootPath);
			repository.Index.Add(filepath);
			repository.Index.Write();

			RemoveFromStaging(repository);
			Commands.Stage(repository, filepath);

			Signature author = new(username, email, DateTimeOffset.Now);
			try
			{
				Commit commit = repository.Commit(message, author, author);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
			return true;
		}

		private static Remote FetchRemote(string filepath, Repository repository, string remoteName = "origin")
		{
			// TODO: Remove hardcoded values
			string msg = "fetching remote file " + filepath;
			Remote remote = repository.Network.Remotes[remoteName];
			var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
			Commands.Fetch(repository, remote.Name, refSpecs, null, msg);

			CheckoutOptions co = new CheckoutOptions();
			if(filepath != "")
				repository.CheckoutPaths("master", new List<string>() { filepath }, co);

			return remote;
		}

		private static bool PullFiles(string tokenFile, string username, string mail, Repository repository)
		{
			string token = GetToken(tokenFile);

			FetchOptions fo = new FetchOptions
			{
				CredentialsProvider = new CredentialsHandler(
					(_url, _username, _types) =>
						new UsernamePasswordCredentials()
						{
							Username = username,
							Password = token
						}),
			};

			PullOptions po = new()
			{
				FetchOptions = fo
			};

			Signature signature = new Signature(new Identity(username, mail), DateTimeOffset.Now);
			
			Remote remote = FetchRemote("", repository);

			ConflictCollection conflicts = repository.Index.Conflicts;
			if (conflicts.Any())
			{
				PrintUsageConflicts(conflicts);
				Environment.Exit(-2);
			}

			try
			{
				Commands.Pull(repository, signature, po);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				if (e is CheckoutConflictException)
				{
					Console.WriteLine("Some Conflicts need resolution");
					Environment.Exit((-2));
				}
				return false;
			}
			return true;
		}

		public static bool OpenFileFromRedirect(string[] args)
		{
			// TODO: Careful with hardcoded values
			string redirPath = "", 
				rootPath = "", 
				branchName = "master";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-r":
						redirPath = args[i + 1];
						break;
					case "-d":
						rootPath = args[i + 1];
						break;
					case "-b":
						branchName = args[i + 1];
						break;
					default:
						PrintUsageOpenFile();
						return false;
				}
			}

			if (InvalidArguments(redirPath, rootPath))
			{
				PrintUsageOpenFile();
				return false;
			}

			Repository repository = new(rootPath);
			using StreamReader sr = new(redirPath);
			try
			{
				string? path = sr.ReadLine();
				if (path == null) return false;
				string realPath = Path.Combine(rootPath, path);
				if (File.Exists(realPath))
				{
					OpenFile(realPath);
					return true;
				}

				List<string> ls = new() { path };

				CheckoutOptions co = new CheckoutOptions()
				{
					CheckoutNotifyFlags = CheckoutNotifyFlags.Conflict
				};
				
				repository.CheckoutPaths(branchName, ls, co);

				OpenFile(realPath);
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

			return true;
		}

		public static bool UpdateRepository(string[] args)
		{
			string rootPath = "",
				username = "",
				mail = "",
				tokenPath = "";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-d":
						rootPath = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					case "-e":
						mail = args[i + 1];
						break;
					case "-t":
						tokenPath = args[i + 1];
						break;
					default:
						PrintUsageUpdate();
						return false;
				}
			}

			if (InvalidArguments(rootPath, username, mail, tokenPath))
			{
				PrintUsageUpdate();
				return false;
			}

			Repository repository = new Repository(rootPath);
			return PullFiles(tokenPath, username, mail, repository);
		}

		// Obsolete?
		private static bool MergeSolver(Repository repository, string mergeOptions = "3")
		{
			MergeOptions mo = SetMergeOptions();
			Signature signature = new Signature("jishimwe", "jeanpaulishimwe@gmail.com", DateTimeOffset.Now);
			repository.MergeFetchedRefs(signature, mo);

			return true;
		}

		public static bool MergeSolver(string[] args)
		{
			string rootPath = "",
				username = "jishimwe",
				mail = "jeanpaulishimwe@gmail.com",
				mergeOptions = args[1];

			for (int i = 2; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-d":
						rootPath = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					case "-e":
						mail = args[i + 1];
						break;
					default:
						PrintUsageMerge();
						return false;
				}
			}

			Repository repository = new(rootPath);
			MergeOptions mo = SetMergeOptions(mergeOptions);
			Signature signature = new Signature(username, mail, DateTimeOffset.Now);

			repository.MergeFetchedRefs(signature, mo);

			return true;
		}

		public static bool OpenConflictFile(string[] args)
		{


			throw new NotImplementedException();
		}

		private static MergeOptions SetMergeOptions(string? s = "3")
		{
			MergeOptions mo = new MergeOptions()
			{
				FastForwardStrategy = FastForwardStrategy.Default,
				FileConflictStrategy = CheckoutFileConflictStrategy.Diff3,
				MergeFileFavor = MergeFileFavor.Normal
			};

			int mergeOptions = s == null ? 3 : int.Parse(s);

			switch (mergeOptions)
			{
				case 0:
					mo.MergeFileFavor = MergeFileFavor.Union;
					break;
				case 1:
					mo.MergeFileFavor = MergeFileFavor.Ours; 
					break;
				case 2:
					mo.MergeFileFavor = MergeFileFavor.Theirs;
					break;
				case 3:
					break;
				default:
					PrintUsageMerge();
					Environment.Exit(-1);
					break;
			}
			return mo;
		}

		private static string GetToken(string? filepath)
		{
			if (filepath == null || !File.Exists(filepath))
				return "";
			try
			{
				return File.ReadAllText(filepath);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Environment.Exit(-1);
				return null;
			}
		}

		// Obsolete?
		private static string ReadRedirFile(string? filepath = null)
		{
			if (filepath == null)
			{
				Console.WriteLine("Enter the path of a .redir file");
				filepath = Console.ReadLine();
			}

			if (filepath == null)
				return "no file";
			using StreamReader sr = new(filepath);
			string read = sr.ReadToEnd();
			return read ?? "no data";
		}

		/*
		 * Open a file in the default application
		 */
		private static void OpenFile(string filepath)
		{
			if (!File.Exists(filepath))
			{
				Console.WriteLine("The file at " + filepath + " doesn't exist");
				return;
			}
			Console.WriteLine("Opening file . . . " + filepath);

			ProcessStartInfo psi = new ProcessStartInfo()
			{
				FileName = filepath,
				UseShellExecute = true
			};
			Process.Start(psi);
		}

		private static void RemoveFromStaging(Repository repository)
		{
			foreach (var item in repository.RetrieveStatus(new StatusOptions()))
			{
				if (item.State == FileStatus.ModifiedInIndex)
					continue;

				Commands.Unstage(repository, item.FilePath);
				Console.WriteLine(item.FilePath + "\t" + item.State + "\t removed from staging");
			}
			Console.WriteLine();
		}
	}
}