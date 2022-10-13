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


        public GhostRepository(string repoURL, string destPath)
        {
            repository = CloneGhostBranchRepository(repoURL, destPath);
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

        private Branch CheckoutBranch(string bracnhName)
        {
            var branch = repository.Branches[bracnhName];
            if (branch == null) return null;
            return Commands.Checkout(repository, branch);
        }
    }
}