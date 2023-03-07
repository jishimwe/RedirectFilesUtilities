using System.Text;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using static RedirectFilesUtilities.UsagePrinter;
using Blob = LibGit2Sharp.Blob;

namespace RedirectFilesUtilities
{
	public static class GitUtilities
	{
		private const string RootPath = "rootPath",
			TokenPath = "tokenPath",
			Username = "username",
			Email = "email",
			Filepath = "filepath",
			Message = "message",
			RedirPath = "redirPath",
			BranchName = "branchName";

		// TODO: Finish arguments handler (not a priority)
	    public static IDictionary<string, string> ArgumentsHandler(string[] args)
	    {
			IDictionary<string, string> result = new Dictionary<string, string>();
			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-d":
						AddToDictionary(result, RootPath, args[i + 1]);
						break;
					case "-t":
						AddToDictionary(result, TokenPath, args[i + 1]);
						break;
					case "-u":
						AddToDictionary(result, Username, args[i + 1]);
						break;
					case "-e":
						AddToDictionary(result, Email, args[i + 1]);
						break;
					case "-f":
						AddToDictionary(result, Filepath, args[i + 1]);
						break;
					case "-m":
						AddToDictionary(result, Message, args[i + 1]);
						break;
					case "-r":
						AddToDictionary(result, RedirPath, args[i + 1]);
						break;
					case "-b":
						AddToDictionary(result, BranchName, args[i + 1]);
						break;
					default:
						Console.WriteLine("Unrecognized argument: " + args[i] + "\t" + args[i + 1]);
						break;
				}
			}
			return result;
	    }

		// Add a val to a dictionary, replace the old value if the key is already in the dictionary
	    private static void AddToDictionary(IDictionary<string, string> argsDictionary, string key, string val)
	    {
		    if (argsDictionary.ContainsKey(key))
		    {
				argsDictionary[key] = val;
		    }
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

	    public static bool PushCommit(string[] args, bool force = false)
		{
			string refSpecs = @"refs/heads/master";
			if (force)
				refSpecs = "+" + refSpecs;

			IDictionary<string, string> argsDictionary = ArgumentsHandler(args);

			if (InvalidArguments(argsDictionary[RootPath], argsDictionary[Username], argsDictionary[TokenPath]))
			{
				PrintUsagePush();
				return false;
			}

			Repository repository = new(argsDictionary[RootPath]);

			PushOptions opt = new PushOptions
			{
				CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials { Username = argsDictionary[Username], Password = GetToken(argsDictionary[TokenPath]) }
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
				return false;
			}

			Console.WriteLine("Push executed");
			return true;
		}

		public static bool ForcePush(string[] args)
		{
			return PushCommit(args, true);
		}

		public static bool CommitFileOrDirectory(string[] args)
		{
			IDictionary<string, string> arguments = ArgumentsHandler(args);

			if (InvalidArguments(arguments[Filepath], arguments[RootPath], arguments[Username], arguments[Message], arguments[Email]))
			{
				PrintUsageCommit();
				return false;
			}

			if (File.Exists(Path.Combine(arguments[RootPath], arguments[Filepath])))
			{
				return CommitFile(arguments[Filepath], arguments[RootPath], arguments[Username], arguments[Email], arguments[Message]);
			}
			return CommitDirectory(arguments[Filepath], arguments[RootPath], arguments[Username], arguments[Email], arguments[Message]);
		}

		public static bool CommitDirectory(string dirpath, string rootPath, string username, string email, string message)
		{
			Repository repository = new Repository(rootPath);
			RemoveFromStaging(repository);

			CommitDirectoryHelper(dirpath, repository);

			Signature author = new(username, email, DateTimeOffset.Now);
			Commit commit = repository.Commit(message, author, author);

			Console.WriteLine("Directory committed " + dirpath);
			Console.WriteLine(commit);

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
				return false;
			}
			return true;
		}

		public static bool CommitFile(string filepath, string rootPath, string username, string email, string message)
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

				Console.WriteLine("File commited " + filepath);
				Console.WriteLine(commit);
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

			var fetchRefSpecs = remote.FetchRefSpecs;

			var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
			string s = remote.FetchRefSpecs.Select(x => x.Source).First() + ":" +
			           (remote.FetchRefSpecs.Select(x => x.Destination).First());
			var refSpecsEnu = new List<string>() { s };
			
			Commands.Fetch(repository, remote.Name, refSpecs, null, msg);

			CheckoutOptions co = new CheckoutOptions();
			if(filepath != "")
				repository.CheckoutPaths("master", new List<string>() { filepath }, co);

			return remote;
		}

		private static bool PullFiles(string tokenFile, string username, string mail, Repository repository)
		{
			FetchOptions fo = new FetchOptions
			{
				CredentialsProvider = new CredentialsHandler(
					(_url, _username, _types) =>
						new UsernamePasswordCredentials()
						{
							Username = username,
							Password = GetToken(tokenFile)
						}),
			};

			CheckoutNotifyFlags cnf = CheckoutNotifyFlags.Conflict | CheckoutNotifyFlags.Dirty |
			                          CheckoutNotifyFlags.Updated |
			                          CheckoutNotifyFlags.Untracked | CheckoutNotifyFlags.Ignored;

			MergeOptions mo = new MergeOptions()
			{
				//CheckoutNotifyFlags = (CheckoutNotifyFlags)31,//.None,
				CheckoutNotifyFlags = cnf,
				OnCheckoutNotify = (string path, CheckoutNotifyFlags flags) =>
				{
					notifs.Add(path, flags);
					if (flags != CheckoutNotifyFlags.Conflict)
						return true;
					if (flags == CheckoutNotifyFlags.Conflict || flags == CheckoutNotifyFlags.Updated)
						return false;
					return false;
				},//MyCheckoutNotifyHandler(),
				//FailOnConflict = true,
				FileConflictStrategy = CheckoutFileConflictStrategy.Merge
			};

			PullOptions po = new()
			{
				FetchOptions = fo,
				MergeOptions = mo
			};

			Signature signature = new Signature(new Identity(username, mail), DateTimeOffset.Now);

			Remote remote = FetchRemote("", repository);

			ConflictCollection conflicts = repository.Index.Conflicts;
			if (conflicts.Any())
			{
				PrintUsageConflicts(conflicts);
				foreach (Conflict co in conflicts)
				{
					ContentChanges cc = repository.Diff.Compare(repository.Lookup<Blob>(co.Theirs.Id), repository.Lookup<Blob>(co.Ours.Id));
					Console.WriteLine(cc.Patch);
				}
				
				Environment.Exit(-2);
			}

			try
			{
				//MergeResult mr = Commands.Pull(repository, signature, po);
				MergeResult mr = repository.MergeFetchedRefs(signature, mo);
				//Console.WriteLine(mr);
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

		private static Dictionary<string, CheckoutNotifyFlags> notifs = new Dictionary<string, CheckoutNotifyFlags>();

		private static CheckoutNotifyHandler MyCheckoutNotifyHandler()
		{
			CheckoutNotifyHandler cnf = delegate(string path, CheckoutNotifyFlags flags)
			{
				notifs.Add(path, flags);
				throw new Exception("We are here");
				if (flags == CheckoutNotifyFlags.Conflict)
					Console.WriteLine(path + " has some conflicts and cannot be merged");
				return false;
			};
			return cnf;
		}

		private static void ConflictHandler(string path, CheckoutNotifyFlags cnf)
		{
			
		}

		public static bool OpenFileFromRedirect(string[] args)
		{
			IDictionary<string, string> arguments = ArgumentsHandler(args);

			if (InvalidArguments(arguments[RedirPath], arguments[RootPath]))
			{
				PrintUsageOpenFile();
				return false;
			}

			Repository repository = new(arguments[RootPath]);
			using StreamReader sr = new(arguments[RedirPath]);
			try
			{
				string? path = sr.ReadLine();
				if (path == null) return false;
				string realPath = Path.Combine(arguments[RootPath], path);
				if (File.Exists(realPath))
				{
					OpenFile(realPath);
					return true;
				}

				List<string> ls = new() { path };

				CheckoutOptions co = new CheckoutOptions();
				//{
				//	CheckoutNotifyFlags = CheckoutNotifyFlags.Conflict
				//};
				co.CheckoutModifiers = CheckoutModifiers.Force;
				repository.CheckoutPaths(arguments[BranchName], ls, co);

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
			IDictionary<string, string> arguments = ArgumentsHandler(args);

			if (InvalidArguments(arguments[RootPath], arguments[Username], arguments[Email], arguments[TokenPath]))
			{
				PrintUsageUpdate();
				return false;
			}

			Repository repository = new Repository(arguments[RootPath]);
			return PullFiles(arguments[TokenPath], arguments[Username], arguments[Email], repository);
		}

		public static bool MergeSolver(string[] args)
		{
			string rootPath = "",
				username = "",
				mail = "",
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

			MergeResult mr = repository.MergeFetchedRefs(signature, mo);
			//MergeResult mr = repository.Merge(repository.Head, signature, mo);

			Console.WriteLine("Merge solved " + mergeOptions);

			return true;
		}

		public static bool AddFile(string[] args)
		{
			string rootPath = "", filepath = "",
				redirPath = "",
				message = "Adding file",
				username = "jishimwe",
				email = "jeanpaulishmwe@gmail.com",
				tokenPath = "",
				refSpecs = @"refs/heads/master";

			IDictionary<string, string> arguments = ArgumentsHandler(args);

			Repository repository = new(arguments[RootPath]);
			repository.Index.Add(arguments[Filepath]);
			repository.Index.Write();

			Repository redirRepository = new(arguments[RedirPath]);
			string redirFile = Path.Combine(arguments[RedirPath], arguments[Filepath]) + ".redir";

			using (FileStream fs = File.Create(redirFile))
			{
				byte[] buff = new UTF8Encoding(true).GetBytes(arguments[Filepath]);
				fs.Write(buff);
				fs.Close();
			}

			string relativePath = redirFile.Replace(arguments[RedirPath] + "\\", "");

			redirRepository.Index.Add(relativePath);
			redirRepository.Index.Write();

			Signature author = new Signature(arguments[Username], arguments[Email], DateTimeOffset.Now);
			message += " -- " + arguments[Filepath];
			PushOptions opt = new PushOptions
			{
				CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials { Username = arguments[Username], Password = GetToken(arguments[TokenPath]) }
			};

			message = !arguments.ContainsKey(Message) ? message : arguments[Message]; 

			redirRepository.Commit(message, author, author);
			redirRepository.Network.Push(FetchRemote("", redirRepository), refSpecs, opt);

			Console.WriteLine("File added " + filepath);

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
				//Console.WriteLine(item.FilePath + "\t" + item.State + "\t removed from staging");
			}
			Console.WriteLine();
		}
	}
}