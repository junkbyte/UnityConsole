#!/usr/bin/env python
from subprocess import call
import os
import sys

os.chdir(os.path.join(sys.path[0],".."))


# because file needs to be packages.config, put it in own directory
packagesFile = os.path.join("packages-versioned","packages.config")
if os.path.isfile(packagesFile):
	call(["nuget", "install",packagesFile,"-outputdirectory","packages-versioned"])

packagesFile = os.path.join("packages","packages.config")
if os.path.isfile(packagesFile):
	call(["nuget", "install",packagesFile,"-excludeversion","-outputdirectory","packages"])
