#Petulant Bear

This project a try out at building a scalable web application:

* updates all AssemblyInfo files
* compiles the application and runs all test projects
* generates [SourceLinks](https://github.com/ctaggart/SourceLink)
* generates API docs based on XML document tags
* generates [documentation based on Markdown files](http://fsprojects.github.io/ProjectScaffold/writing-docs.html)
* generates [NuGet](http://www.nuget.org) packages
* and allows a simple [one step release process](http://fsprojects.github.io/ProjectScaffold/release-process.html).

## Fresh Install
In order to do a fresh install of the petulant , you need to have fsharp installed with the latest .net framework installed

### fetching files

#### source code
open github shell and move to petulant bear folder
    git pull
    git branch v0.0.2.0

#### packages and external files
open windows explorer and under petulant bear root folder execute

    executeinstallPakage.cmd

### Building the soft

    $ build.cmd // on windows    
    $ build.sh  // on mono

### running

#### development
once built, everything will be located under the bin folder, ready to be deployed for prod.

#### app.config

You have to adjust a few things in the PetulantBear.exe.config to make it work for your environment.

    <add name="bear2bearDB" connectionString="Data Source={YOUR_LOCAL_PATH}\db\Bear2Bear.db;Version=3"/>
    <add key="rootPath" value="{YOUR_LOCAL_PATH}\wwwroot\"/>
    <add key="IPAddress" value="{YOUR_LOCAL_IP}"/>
    <add key="Port" value="{YOUR_LOCAL_PORT}"/>
    <add key="urlSite" value="{YOUR_LOCAL_URL}"/>
    <add key="eventStoreConnectionString" value="tcp://{EVENTSTORE_IP}:{EVENTSTORE_PORT}" />
    <add key="eventStoreClientIp" value="{EVENTSTORE_IP}" />
    <add key="eventStoreClientPort" value="{EVENTSTORE_PORT}" />

usually in dev these stand for :
  {YOUR_LOCAL_PATH} -> bin url of the github repo on your pc
  {YOUR_LOCAL_IP} -> 127.0.0.1
  {YOUR_LOCAL_PORT} -> 8084
  {YOUR_LOCAL_URL} -> http://localhost:8084
  {EVENTSTORE_IP} -> 127.0.0.1
  {EVENTSTORE_PORT} -> 1113

#### command line argument
you can also start the petulant and add command line arguments, these will be overload any definition made in the app.config

    --dbConnection (string)-> to fill out the connection string to the bear2bear sqlite db
    --rootPath (string)-> to fill out the root path to the bear2bear static files of the wwwroot
    --ipAddress (string)-> the ip address used internally to start the web server
    --port (int)-> the port used internally to start the web server
    --urlSite (string)-> the url used by facebook to enable callback from author
    --eventStoreConnectionString (string)-> the connection string of the eventstore
    --eventStoreClientIp (string)-> the ip the eventstore client should connect to
    --eventStoreClientPort (int)-> the port the eventstore client should connect to
    --elmah (string)-> the key to configure elmah logging

#### batch

There are 3 cmd that allows ou to start the bundle according to what you need to do :

    $ startPetulant.bat // start the full bin version of petulant (used from bin folder : web server, wwwroot, db)
    $ startPetulantFordevRoot.bat // start the bin version of petulant for accessing devroot folder of github repo (used from bin folder : web server,  db)
    $ startPetulantForWwRoot.bat // start the bin version of petulant for accessing wwwroot folder of github repo (used from bin folder : web server,  db)

when closing the petulant, beware of closing also the getEventStore...

### database

Building the soft will build the database according to the existing one under \src\db\db. It will apply every script in order to try to ugrade it to the latest dev.

#### Building the db from production

    download database prod from petulant ftp, copy it to %YourPath%\petulant-bear\src\db\db
    rename the prod bear2bear.db in Bear2BearProd.db, it will be used later to apply latest script to prod database

        bear2bear.db -> Bear2BearProd.db

    execute at the root of your github repo the following command :

      buildDB.bat

    it will copy the previous prod version of the database and try to  apply latest sql scripts. If you want to use it with the batches previously detailled, you will have to run build.cmd once again, so that your changes are deployed to the bin folder.

#### databases

in order to reset all data, you 'll need to:

    $ delete data folder under the root folder of your github repo
    $ delete src/db/db folder
    $ execute anew build.cmd to recreate a fresh db

## Maintainer(s)

- [@arthis](https://github.com/arthis)
- [@Julienfouquet](https://github.com/Julienfouquet)


special thx for the work of the fsharp community that helped so much...
