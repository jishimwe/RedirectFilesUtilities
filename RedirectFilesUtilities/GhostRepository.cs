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


        public GhostRepository(string repoURL, string destPath)
        {
            //repository = CloneGhostBranchRepository(repoURL, destPath);
            repository = CloneEmptyRepository(repoURL, destPath);
            Console.WriteLine("Repository cloned");
        }

        private Repository CloneGhostBranchRepository(string repoURL, string destPath)
        {
            var co = new CloneOptions
            {
                BranchName = "ghost",
                CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = "jishimwe", Password = "" }
            };
            Repository.Clone(repoURL, destPath, co);

            return new Repository(destPath);
        }

        private Repository CloneEmptyRepository(string repoURL, string destPath)
        {
            string rootedPath = Repository.Init(destPath);
            string logMessage = "fetch test";

            Repository repo = new Repository(destPath);
            //Remote remote = repo.Network.Remotes[remoteDefaultName];
            Remote remote = repo.Network.Remotes.Add(remoteDefaultName, repoURL);
            var refSpec = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repo, remote.Name, refSpec, null, logMessage);
            return repo;
            //return null;
        }

        private bool CheckoutFile(string filepath)
        {

            return false;
        }

        private Branch CheckoutBranch(string bracnhName)
        {
            var branch = repository.Branches[bracnhName];
            if (branch == null) return null;
            CheckoutOptions co = new CheckoutOptions();
            return Commands.Checkout(repository, branch);
        }
    }
}