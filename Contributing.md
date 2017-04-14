# How to contribute

Third-party patches are welcomed! We want to keep it as easy as possible to contribute changes that
get things working in your environment. There are a few guidelines that we
need contributors to follow so that we can have a chance of keeping on
top of things.

## Getting Started

* Make sure you have a [GitHub account](https://github.com/signup/free).
* Submit bugs to the Github repo via the issues area.
* Fork the repository on GitHub.

## Making Changes

* Create a topic branch from where you want to base your work.
  * This is usually the master branch.
  * Only target release branches if you are certain your fix must be on that
    branch.
  * To quickly create a topic branch based on master; `git checkout -b
    fix/master/my_contribution master`. Please avoid working directly on the
    `master` branch.
* Make commits of logical units.
* Check for unnecessary whitespace with `git diff --check` before committing.
* Make sure your commit messages are in the proper format.

' Not required at the moment - more "would be nice if"
* Make sure you have added the necessary tests for your changes.
* Run _all_ the tests to assure nothing else was accidentally broken.


## Submitting Changes

* Sign the [Contributor License Agreement](https://www.clahub.com/agreements/PaulHutson/SlackMUDRPG).
* Push your changes to a topic branch in your fork of the repository.
* Submit a pull request to the repository.
* Update any issues to mark that you have submitted code and are ready for it to be reviewed (Status: Ready for Merge).
  * Include a link to the pull request in the ticket.
* The core team looks at Pull Requests on a regular basis - we will try to review updates asap.
* After feedback has been given we expect responses within two weeks. After two
  weeks we may close the pull request if it isn't showing any activity.

## Revert Policy
We reserve the right to revert anything submitted that breaks existing functionality (although will try to work 
with contributors via the PR process to try to avoid this ever happening).

# Additional Resources

* [Bug tracker (GitHub Issues)](https://github.com/PaulHutson/SlackMUDRPG/issues)
* [Contributor License Agreement](https://www.clahub.com/agreements/PaulHutson/SlackMUDRPG)
* [General GitHub documentation](https://help.github.com/)
* [GitHub pull request documentation](https://help.github.com/articles/creating-a-pull-request/)
