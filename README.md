# Remote Development and Compilation
___

**Author** : *Ishimwe Jean-Paul*

**Noma** : *6919-12-00*
___

## Redirect File Utilities


This project implements the necessary function for a Redirect Project.
A Redirect Project is composed of two repository :

- A repository containing a real project with real code
- A repository containint a *redirect* project with redirect files

Redirect files are files tha contains information on where to find the *real* file project (generaly on remote repository)


### Implemented functions

Here are the function already implemented and how to use them.

- OpenFileFromRedirect: 
A function that will find the file indicated by a redirect file and download it from the remote repository. \
To use it, launch the program with the folowing arguments:
```
	-o		: flag to launch the opening a file
	-r <path>	: the path to a redirect file
	-d <path>	: the path to the root of real project
```


- CommitFile: 
A function that will commit the changes done to file \
To use it, launch the program with the folowing arguments:
```
	-c		: flag to launch a commit 
	-f <path>	: path to the file to commit
	-d <path>	: path to the root of real project
	-m <message>	: a message for the commit
	-u <user>	: the git user username
```

- PushCommit:
A function that will push the commits done by the previous \
To use it, launch the program with the folowing arguments:
```
	-p		: flag to launch a push
	-d <path>	: path to the root of real project
	-t <path>	: flag to give a token with a path to a file containing the token
	-u <user>	: the git user username
```

___
### Future functions


- [ ] Commiting a directory
- [ ] Pushing a directory
- [ ] Conflict solving
- [ ] Merging