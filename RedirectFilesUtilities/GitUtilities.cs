using System.Collections;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using static RedirectFilesUtilities.UsagePrinter;

namespace RedirectFilesUtilities
{
	public static class GitUtilities
	{
		private const string
			Filepath = "filepath",
			Message = "message",
			ConfigFile = "configFile";

		public static readonly string
			RedirectRepositoryUrl = "RedirectRepositoryUrl",
			RedirectDirectoryPath = "RedirectDirectoryPath",
			RealRepositoryUrl = "RealRepositoryUrl",
			RealRepositoryPath = "RealRepositoryPath",
			Username = "Username",
			Mail = "Mail",
			TokenPath = "TokenPath",
			UtilsPath = "UtilsPath",
			BranchName = "BranchName",
			RemoteName = "RemoteName",
			RefSpecs = "RefSpecs",
			RedirectFile = "RedirectFile",
			MergeOptions = "MergeOptions";

		// TODO: Finish arguments handler (not a priority)
		public static IDictionary<string, string> ArgumentsHandler(string[] args)
	    {
			IDictionary<string, string> result = new Dictionary<string, string>();
			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-f":
						AddToDictionary(result, Filepath, args[i + 1]);
						break;
					case "-m":
						AddToDictionary(result, Message, args[i + 1]);
						break;
					case "-mo":
						AddToDictionary(result, MergeOptions, args[i + 1]);
						break;
					case "-conf":
						AddToDictionary(result, ConfigFile, args[i + 1]);
						break;
					default:
						Console.WriteLine("What's wrong with my arguments?");
						Console.WriteLine("{0} |", string.Join(" |", args));
						Console.WriteLine("Unrecognized argument: " + args[i] + " \t " + args[i + 1]);
						break;
				}
			}
			return result;
	    }

		// Add a val to a dictionary, replace the old value if the key is already in the dictionary
	    private static void AddToDictionary(IDictionary<string, string> argsDictionary, string key, string val)
	    {
		    if (argsDictionary.ContainsKey(key))
				argsDictionary[key] = val;
		    else
				argsDictionary.Add(key, val);
	    }

	    public static bool InvalidArguments(params string[] args)
	    {
		    bool res = false;
		    foreach (string a in args)
		    {
			    res |= a == "";
			    if (res) return res;
		    }
		    return res;
	    }

		public static bool CommitFileOrDirectory(string[] args)
		{
			IDictionary<string, string> arguments = ArgumentsHandler(args);
			IDictionary<string, string>? config = ReadConfig(arguments[ConfigFile]);
			if (config == null)
			{
				Console.WriteLine("Configuration file Error");
				return false;
			}

			if (InvalidArguments(arguments[Filepath], config[RealRepositoryPath], config[Username], arguments[Message], config[Mail]))
			{
				PrintUsageCommit();
				return false;
			}

			if (File.Exists(Path.Combine(config[RealRepositoryPath], arguments[Filepath])))
			{
				return CommitFile(arguments[Filepath], config[RealRepositoryPath], config[Username], config[Mail], arguments[Message]);
			}
			return CommitDirectory(arguments[Filepath], config[RealRepositoryPath], config[Username], config[Mail], arguments[Message]);
		}

		private static bool CommitDirectory(string dirpath, string rootPath, string username, string email, string message)
		{
			Repository repository = new(rootPath);
			RemoveFromStaging(repository);

			if (!CommitDirectoryHelper(dirpath, repository)) return false;

			try
			{
				Signature author = new(username, email, DateTimeOffset.Now);
				Commit commit = repository.Commit(message, author, author);

				Console.WriteLine("Directory committed " + dirpath);
				Console.WriteLine(commit);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}

			return true;
		}

		private static bool CommitDirectoryHelper(string dirpath, Repository repository, string? pathSoFar = null)
		{
			DirectoryInfo di = new(dirpath);

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
				return false;
			}
			return true;
		}

		private static bool CommitFile(string filepath, string rootPath, string username, string email, string message)
		{
			Repository repository = new(rootPath);
			string f = filepath.Replace(rootPath + "\\", "");
			repository.Index.Add(f);
			repository.Index.Write();

			RemoveFromStaging(repository);
			Commands.Stage(repository, filepath);

			Signature author = new(username, email, DateTimeOffset.Now);
			try
			{
				Commit commit = repository.Commit(message, author, author);

				Console.WriteLine("File committed " + filepath);
				Console.WriteLine(commit);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}

			return true;
		}

		public static bool PushCommit(string[] args, bool force = false)
		{
			IDictionary<string, string> arguments = ArgumentsHandler(args);
			IDictionary<string, string>? config = ReadConfig(arguments[ConfigFile]);
			if (config == null)
			{
				Console.WriteLine("Configuration file Error");
				return false;
			}

			string refSpecs = config[RefSpecs] == "" ? @"refs/heads/master" : config[RefSpecs];
			if (force)
				refSpecs = "+" + refSpecs;

			if (InvalidArguments(config[RealRepositoryPath], config[Username], config[TokenPath]))
			{
				PrintUsagePush();
				return false;
			}

			Repository repository = new(config[RealRepositoryPath]);

			PushOptions opt = new()
			{
				CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials { Username = config[Username], Password = GetToken(config[TokenPath]) }
			};

			// TODO: Careful with hardcoded values
			Remote remote = FetchRemote("", repository, config[BranchName], config[RemoteName]);

			try
			{
				repository.Network.Push(remote, refSpecs, opt);
			}
			catch (NonFastForwardException nfe)
			{
				Console.WriteLine(nfe.Message);
				Console.WriteLine("Some files may need updating, try updating before pushing");
				return false;
			}

			Console.WriteLine("Push executed");
			return true;
		}

		public static bool ForcePush(string[] args)
		{
			return PushCommit(args, true);
		}

		private static Remote FetchRemote(string filepath, Repository repository, string branchName = "master", string remoteName = "origin")
		{
			Console.WriteLine("Fetching remotes |||");
			// TODO: Remove hardcoded values
			string msg = "fetching remote file " + filepath;
			Remote remote = repository.Network.Remotes[remoteName];

			var fetchRefSpecs = remote.FetchRefSpecs;

			var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
			string s = remote.FetchRefSpecs.Select(x => x.Source).First() + ":" +
			           (remote.FetchRefSpecs.Select(x => x.Destination).First());
			var refSpecsEnu = new List<string>() { s };
			
			Commands.Fetch(repository, remote.Name, refSpecs, null, msg);

			CheckoutOptions co = new();
			if(filepath != "")
				repository.CheckoutPaths(branchName, new List<string>() { filepath }, co);

			Console.WriteLine("||| Fetching remotes DONE |||");
			return remote;
		}

		private static bool PullFiles(string tokenFile, string username, string mail, string branchName, string remoteName, Repository repository)
		{
			Console.WriteLine("Pulling files |||");
			FetchOptions fo = new()
			{
				CredentialsProvider = (_url, _username, _types) =>
					new UsernamePasswordCredentials()
					{
						Username = username,
						Password = GetToken(tokenFile)
					},
			};

			CheckoutNotifyFlags cnf = CheckoutNotifyFlags.Conflict | CheckoutNotifyFlags.Dirty |
			                          CheckoutNotifyFlags.Updated |
			                          CheckoutNotifyFlags.Untracked | CheckoutNotifyFlags.Ignored;

			MergeOptions mo = new()
			{
				CheckoutNotifyFlags = cnf,
				OnCheckoutNotify = (string path, CheckoutNotifyFlags flags) =>
				{
					notifs.Add(path, flags);
					return flags != CheckoutNotifyFlags.Conflict;
				},
				FailOnConflict = true,
				// FileConflictStrategy = CheckoutFileConflictStrategy.Merge
			};

			Remote remote = FetchRemote("", repository, branchName, remoteName);

			var status = repository.RetrieveStatus();
			Console.WriteLine("|_|-|¯| Pulling files Remote Fetched |||");

			ConflictCollection conflicts = repository.Index.Conflicts;
			if (conflicts.Any())
			{
				PrintUsageConflicts(conflicts);
				foreach (Conflict co in conflicts)
				{
					ContentChanges cc = repository.Diff.Compare(repository.Lookup<Blob>(co.Theirs.Id), repository.Lookup<Blob>(co.Ours.Id));
					Console.WriteLine(cc.Patch);
				}
				Console.WriteLine("\nSome Conflicts need resolution");
				Environment.Exit(-2);
			}
			Console.WriteLine("|_|-|¯| Pulling files No Conflicts? |||");
			try
			{
				Signature signature = new(new Identity(username, mail), DateTimeOffset.Now);
				MergeResult mr = repository.MergeFetchedRefs(signature, mo);
				if (mr.Status == MergeStatus.Conflicts)
				{
					Console.WriteLine("Some Conflicts need resolution");
					conflicts = repository.Index.Conflicts;
					if (conflicts.Any())
					{
						PrintUsageConflicts(conflicts);
						foreach (Conflict co in conflicts)
						{
							ContentChanges cc = repository.Diff.Compare(repository.Lookup<Blob>(co.Theirs.Id), repository.Lookup<Blob>(co.Ours.Id));
							Console.WriteLine(cc.Patch);
						}
					}
					Environment.Exit((-2));
				}
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
			Console.WriteLine("Pull finished -  Repository updated");
			return true;
		}

		private static Dictionary<string, CheckoutNotifyFlags> notifs = new();

		private static CheckoutNotifyHandler MyCheckoutNotifyHandler()
		{
			CheckoutNotifyHandler cnf = delegate(string path, CheckoutNotifyFlags flags)
			{
				notifs.Add(path, flags);
				if (flags == CheckoutNotifyFlags.Conflict)
					Console.WriteLine(path + " has some conflicts and cannot be merged");
				return false;
			};
			return cnf;
		}

		public static bool OpenFileFromRedirect(string[] args)
		{
			IDictionary<string, string> arguments = ArgumentsHandler(args);
			IDictionary<string, string>? config = ReadConfig(arguments[ConfigFile]);
			if (config == null)
			{
				Console.WriteLine("Configuration file Error");
				return false;
			}

			if (InvalidArguments(config[RedirectDirectoryPath], config[RealRepositoryPath]))
			{
				PrintUsageOpenFile();
				return false;
			}

			Repository repository = new(config[RealRepositoryPath]);
			using StreamReader sr = new(arguments[Filepath]);
			try
			{
				string? path = sr.ReadLine();
				if (path == null) return false;
				string realPath = Path.Combine(config[RealRepositoryPath], path);
				if (File.Exists(realPath))
				{
					Console.WriteLine($"-f {realPath}");
					return true;
				}

				List<string> ls = new() { path };

				CheckoutOptions co = new()
				{
					CheckoutModifiers = CheckoutModifiers.Force
				};
				repository.CheckoutPaths(config[BranchName], ls, co);

				if (!File.Exists(realPath))
				{
					Console.WriteLine("The file at " + realPath + " doesn't exist");
					return false;
				}

				Console.WriteLine($"-f {realPath}");
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
				return false;
			}

			return true;
		}

		// TODO: A Config file handler (not a priority)
		public static bool WriteConfigFile(string[] args)
		{
			IDictionary<string, string> config = new Dictionary<string, string>();
			string configPath = "redirect_config.ini";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-d":
						AddToDictionary(config, RealRepositoryPath, args[i + 1]);
						break;
					case "-dUrl":
						AddToDictionary(config, RealRepositoryUrl, args[i + 1]);
						break;
					case "-r":
						AddToDictionary(config, RedirectDirectoryPath, args[i + 1]);
						break;
					case "-rUrl":
						AddToDictionary(config, RedirectRepositoryUrl, args[i + 1]);
						break;
					case "-u":
						AddToDictionary(config, Username, args[i + 1]);
						break;
					case "-e":
						AddToDictionary(config, Mail, args[i + 1]);
						break;
					case "-t":
						AddToDictionary(config, TokenPath, args[i + 1]);
						break;
					case "-b":
						AddToDictionary(config, BranchName, args[i + 1]);
						break;
					case "-rm":
						AddToDictionary(config, RemoteName, args[i + 1]);
						break;
					case "-rf":
						AddToDictionary(config, RefSpecs, args[i + 1]);
						break;
					case "-ut":
						AddToDictionary(config, UtilsPath, args[i + 1]);
						break;
					case "-conf":
						configPath = args[i + 1];
						break;
					default:
						Console.WriteLine("What's wrong with my arguments?");
						Console.WriteLine("{0} |", string.Join(" |", args));
						Console.WriteLine("Unrecognized argument: " + args[i] + " \t " + args[i + 1]);
						break;
				}
			}

			FileInfo fi = new(configPath);
			if (fi.Exists)
			{
				fi.Delete();
			}

			string[] toFile =
			{
				"Usage: This a redirect config file - make sure the all the values are filled",
				"\n",
				RedirectRepositoryUrl + " = " + config[RedirectRepositoryUrl],
				RedirectDirectoryPath + " = " + config[RedirectDirectoryPath],
				RealRepositoryUrl + " = " + config[RealRepositoryUrl],
				RealRepositoryPath + " = " + config[RealRepositoryPath],
				Username + " = " + config[Username],
				Mail + " = " + config[Mail],
				TokenPath + " = " + config[TokenPath],
				UtilsPath + " = " + config[UtilsPath],
				"\n",
				"[Optional]",
				BranchName + " = " + config[BranchName],
				RemoteName + " = " + config[RemoteName],
				RefSpecs + " = " + config[RefSpecs]
			};
			try
			{
				File.WriteAllText(configPath, string.Join("\n", toFile));
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error while writing the configuration file");
				PrintUsageWriteConfigFile();
				Console.WriteLine(e);
				Environment.Exit(-1);
			}
			
			return false;
		}

		public static IDictionary<string, string>? ReadConfig(string configPath)
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

		public static bool OpenFileFromReal(string[] args)
		{
			IDictionary<string, string>? config = ReadConfig(args[2]);
			if (config == null)
			{
				Console.WriteLine("Configuration file Error");
				return false;
			}

			Repository repository = new(config[RealRepositoryPath]);
			string realPath = args[4];
			if (File.Exists(realPath))
			{
				Console.WriteLine(realPath);
				return true;
			}

			string? redirPath = Path.Combine(config[RedirectDirectoryPath], realPath);
			if (realPath.Contains(config[RealRepositoryPath]))
			{
				redirPath = realPath.Substring(config[RealRepositoryPath].Length, realPath.Length - config[RealRepositoryPath].Length);
				redirPath = config[RedirectDirectoryPath] + redirPath + ".redir";
			}
			else
			{
				redirPath += ".redir";
			}

			Console.WriteLine("The new made up redirect path: {0}", redirPath);
			if (File.Exists(redirPath))
			{
				using StreamReader sr = new(redirPath);
				string path = sr.ReadToEnd();

				List<string> ls = new() { path };

				CheckoutOptions co = new()
				{
					CheckoutModifiers = CheckoutModifiers.Force
				};
				repository.CheckoutPaths(config[BranchName], ls, co);

				Console.WriteLine(realPath);

				return true;
			}
			return false;
		}


		public static bool UpdateRepository(string[] args)
		{
			IDictionary<string, string> arguments = ArgumentsHandler(args);
			IDictionary<string, string>? config = ReadConfig(arguments[ConfigFile]);
			if (config == null)
			{
				Console.WriteLine("Configuration file Error");
				return false;
			}

			if (InvalidArguments(config[RealRepositoryPath], config[Username], config[Mail], config[TokenPath]))
			{
				PrintUsageUpdate();
				return false;
			}

			Repository repository = new(config[RealRepositoryPath]);
			return PullFiles(config[TokenPath], config[Username], config[Mail], config[BranchName], config[RemoteName], repository);
		}

		public static bool MergeSolver(string[] args)
		{
			string rootPath = "",
				username = "",
				mail = "",
				mergeOptions = args[1];

			IDictionary<string, string> arguments = ArgumentsHandler(args);

			IDictionary<string, string>? config = ReadConfig(arguments[ConfigFile]);
			if (config == null)
			{
				Console.WriteLine("There is an error with the configuration file {0}", args[3]);
				return false;
			}

			Console.WriteLine("{0} |", string.Join(" |", args));
			// Console.WriteLine(string.Join(Environment.NewLine, config));
			// Console.WriteLine(string.Join(Environment.NewLine, arguments));

			Repository repository = new(config[RealRepositoryPath]);
			MergeOptions mo = SetMergeOptions(arguments[MergeOptions]);
			Signature signature = new(config[Username], config[Username], DateTimeOffset.Now);
			_ = repository.MergeFetchedRefs(signature, mo);

			Console.WriteLine("Merge solved " + arguments[MergeOptions]);

			return true;
		}

		public static bool AddFile(string[] args)
		{
			string filepath = "", message = "Adding new file";

			IDictionary<string, string> arguments = ArgumentsHandler(args);
			IDictionary<string, string>? config = ReadConfig(arguments[ConfigFile]);
			if (config == null)
			{
				Console.WriteLine("Configuration file Error");
				return false;
			}

			string refSpecs = config[RefSpecs] == "" ? @"refs/heads/master" : config[RefSpecs];

			Repository repository = new(config[RealRepositoryPath]);
			repository.Index.Add(arguments[Filepath]);
			repository.Index.Write();

			Repository redirRepository = new(config[RedirectDirectoryPath]);
			string redirFile = Path.Combine(config[RedirectDirectoryPath], arguments[Filepath]) + ".redir";

			using (FileStream fs = File.Create(redirFile))
			{
				byte[] buff = new UTF8Encoding(true).GetBytes(arguments[Filepath]);
				fs.Write(buff);
				fs.Close();
			}

			string relativePath = redirFile.Replace(config[RedirectDirectoryPath] + "\\", "");

			redirRepository.Index.Add(relativePath);
			redirRepository.Index.Write();

			Signature author = new(config[Username], config[Mail], DateTimeOffset.Now);
			message += " -- " + arguments[Filepath];
			PushOptions opt = new()
			{
				CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials { Username = config[Username], Password = GetToken(config[TokenPath]) }
			};

			message = !arguments.ContainsKey(Message) ? message : arguments[Message]; 

			redirRepository.Commit(message, author, author);
			redirRepository.Network.Push(FetchRemote("", redirRepository, config[BranchName], config[RemoteName]), refSpecs, opt);

			Console.WriteLine("File added " + filepath);

			return true;
		}

		public static bool OpenConflictFile(string[] args)
		{
			throw new NotImplementedException();
		}

		private static MergeOptions SetMergeOptions(string? s = "3")
		{
			MergeOptions mo = new()
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

			Console.WriteLine($"-f {filepath}");
		}

		private static void RemoveFromStaging(Repository repository)
		{
			foreach (var item in repository.RetrieveStatus(new StatusOptions()))
			{
				if (item.State == FileStatus.ModifiedInIndex)
					continue;

				Commands.Unstage(repository, item.FilePath);
				// Console.WriteLine(item.FilePath + "\t" + item.State + "\t removed from staging");
			}
			Console.WriteLine();
		}
	}
}