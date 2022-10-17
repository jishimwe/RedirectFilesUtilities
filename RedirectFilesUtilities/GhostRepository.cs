using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace RedirectFilesUtilities
{
    internal class GhostRepository
    {
        private Repository repository;
        private string remoteDefaultName = "origin";
        private string defaultBranchName = "master";
        private string orphanBranchName = "ghost";
        private string gitUser = "jishimwe";
        private string gitMail = "jeanpaulishimwe@gmail.com";


        public GhostRepository(string repoURL, string destPath)
        {
            repository = CloneGhostBranchRepository(repoURL, destPath);
            //repository = CloneEmptyRepository(repoURL, destPath);
            Console.WriteLine("Repository cloned");
            CheckoutFile(@"app/src/main/AndroidManifest.xml");
            Console.WriteLine(@"app/src/main/AndroidManifest.xml checked out?");
        }

        private Repository CloneGhostBranchRepository(string repoURL, string destPath)
        {
            var co = new CloneOptions
            {
                BranchName = defaultBranchName,
                CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = gitUser, Password = "" }
            };
            co.Checkout = false;
            Repository.Clone(repoURL, destPath, co);

            return new Repository(destPath);
        }

        // Obsolete
        private Repository CloneEmptyRepository(string repoURL, string destPath)
        {
            string rootedPath = Repository.Init(destPath);
            string logMessage = "fetch test";

            Repository repo = new Repository(destPath);
            //Remote remote = repo.Network.Remotes[remoteDefaultName];
            Remote remote = repo.Network.Remotes.Add(remoteDefaultName, repoURL);
            var refSpec = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repo, remote.Name, null, null, logMessage);
            return repo;
            //return null;
        }

        private bool CheckoutFile(string filepath)
        {
            List<string> ls = new List<string>();
            ls.Add(filepath);
            repository.CheckoutPaths( "master", ls,    new CheckoutOptions());
            return true;
        }

        private Branch CheckoutBranch(string bracnhName)
        {
            var branch = repository.Branches[bracnhName];
            if (branch == null) return null;
            CheckoutOptions co = new CheckoutOptions();
            return Commands.Checkout(repository, branch);
        }

		private Commit CommitToRepository(string filepath)
		{
            Signature author = new Signature(gitUser, gitMail, DateTime.Now);
            Signature commiter = author;
            CommitOptions commitOptions = new CommitOptions(); 
            CommitFilter commitFilter = new CommitFilter();
            commitOptions.AllowEmptyCommit = true;
            Commit commit = repository.Commit("test commit from Visual Studio", author, commiter);
			return commit;
		}
	}
}