# TeamCity plug

The TeamCity plug provides an interface to perform actions in a remote TeamCity
server for the Plastic SCM DevOps system.

# Behavior
The TeamCity plug receives requests from the Plastic SCM Server DevOps system and
performs the appropriate actions using the TeamCity REST API.

It currently supports two actions: start a new build or retrieve the status
of an existing build.

The plug communicates with the Plastic SCM Server DevOps system through a websocket
connection.

# Build
The executable is built from .NET Framework code using the provided `src/teamcityplug.sln`
solution file. You can use Visual Studio or MSBuild to compile it.

**Note:** We'll use `${DEVOPS_DIR}` as alias for `%PROGRAMFILES%\PlasticSCM5\server\devops`
in *Windows* or `/var/lib/plasticscm/devops` in *macOS* or *Linux*.

# Setup

## Configuration files
You'll notice some configuration files under `/src/configuration`. Here's what they do:
* `teamcityplug.log.conf`: log4net configuration. The output log file is specified here. This file should be in the binaries output directory.
* `ci-teamcityplug.definition.conf`: plug definition file. You'll need to place this file in the Plastic SCM DevOps directory to allow the system to discover your TeamCity plug.
* `teamcityplug.config.template`: mergebot configuration template. It describes the expected format of the TeamCity plug configuration. We recommend to keep it in the binaries output directory
* `teamcityplug.conf`: an example of a valid TeamCity plug configuration. It's built according to the `teamcityplug.config.template` specification.

## Add to Plastic SCM Server DevOps
To allow Plastic SCM Server DevOps to discover your custom TeamCity plug, just drop 
the `ci-teamcityplug.definition.conf` file in `${DEVOPS_DIR}/config/plugs/available$`.
Make sure the `command` and `template` keys contain the appropriate values for
your deployment!

# Support
If you have any questions about this plug don't hesitate to contact us by
[email](support@codicesoftware.com) or in our [forum](http://www.plasticscm.net)!