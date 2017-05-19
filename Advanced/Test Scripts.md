# Test Scripts

A test script is a batch of console commands that are executed together, in sequential order.  The format of a test script is a plain-text file that contains one command on each line, and should be placed in the TestScripts folder adjacent to the executable.  Comments begin with an octothorpe (#), and may be added to the end of an otherwise properly-formed command.

There are two file names that Haxxis treats differently from others.  `StartupLocal.txt` and `Startup.txt`.  If Haxxis finds a `StartupLocal.txt` file it will run that script immediately upon startup.  If no StartupLocal script is found Haxxis will instead run `Startup.txt` if it exists.

A different startup test script may be selected by way of the `-RunOnStartup` command line argument.

A test script may call another script by using the `test` command.