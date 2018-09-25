# TeamCity plug

The TeamCity plug provides an interface to perform actions in a remote TeamCity
server for the Plastic SCM DevOps system.

This is the source code used by the actual built-in TeamCity plug. Use it as a reference
to build your own CI plug!

# Build
The executable is built from .NET Framework code using the provided `src/teamcityplug.sln`
solution file. You can use Visual Studio or MSBuild to compile it.

**Note:** We'll use `${DEVOPS_DIR}` as alias for `%PROGRAMFILES%\PlasticSCM5\server\devops`
in *Windows* or `/var/lib/plasticscm/devops` in *macOS* or *Linux*.

# Setup
If you just want to use the built-in TeamCity plug you don't need to do any of this.
The TeamCity plug is available as a built-in plug in the DevOps section of the WebAdmin.
Open it up and configure your own!

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

# Behavior
The **TeamCity plug** provides an API for **mergebots** to connect to TeamCity.
They use the plug to launch builds in a TeamCity server and retrieve the build status.

## What the configuration looks like
When a mergebot requires a CI plug to work, you can select a TeamCity Plug Configuration.

<p align="center">
  <img alt="CI plug select" src="https://raw.githubusercontent.com/PlasticSCM/teamcityplug/master/doc/img/ci-plug-select.png" />
</p>

You can either select an existing configuration or create a new one.

When you create a new TeamCity Plug Configuration, you have to fill in the following values:

<p align="center">
  <img alt="teamcityplug configuration example"
       src="https://raw.githubusercontent.com/PlasticSCM/teamcityplug/master/doc/img/configuration-example.png" />
</p>

## Installation requirements - The TeamCity Lightweight Plugin
**⚠️ Important! ⚠️**

Please make sure that you've installed our lightweight TeamCity plugin before you create
a new configuration for a server. You can find it in the **client** install
directory (`%PROGRAMFILES%\PlasticSCM5\client` in Windows, `/opt/plasticscm5/client`
in Linux or `/Applications/PlasticSCM.app/Contents/MonoBundle` in macOS),
inside the `mergebot-zeroconf-plugins` directory.

You'll also need to install a Plastic SCM CLI Client (version **7.0.16.2200** or higher)
in the TeamCity machine. It's required to perform all SCM operations against the server
(e.g. update the TeamCity Plastic SCM workspace). The user account running TeamCity will need
a valid Plastic SCM Client configuration to contact the target Plastic SCM Server.

## TeamCity Configuration
The lightweight TeamCity plugin makes it unnecessary to specify repositories in TeamCity.
Instead, add a `Mergebot Plastic SCM` step as the first one in your build configuration.
The `mergebot` will take care of the rest!

<p align="center">
  <img alt="Project configuration"
       src="https://raw.githubusercontent.com/PlasticSCM/teamcityplug/master/doc/img/project-configuration.png" />
</p>

When the **mergebot** requests a new build run or an existing build status
from the **TeamCity plug**, it calls the remote TeamCity API using the URL and
credentials in the plug configuration.

## How it works

When a user creates a new **TeamCity plug** configuration, by default it executes
the built-in plug binaries using the values of the configuration. Then, it automatically
connects to the Plastic SCM server through a *websocket* and stands by for requests.
You can also choose not to automatically run that particular configuration if you don't want to.

# Support
If you have any questions about this plug don't hesitate to contact us by
[email](support@codicesoftware.com) or in our [forum](http://www.plasticscm.net)!
