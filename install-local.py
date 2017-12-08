#!/usr/bin/env python
from subprocess import call
import os
import sys
import glob

configurations=["Release"]

os.chdir(sys.path[0])

version = None
if len(sys.argv) > 1:
	version = sys.argv[1]

for slnfile in glob.glob("*.sln"):
	for configuration in configurations:
		rc=call(["msbuild", "/p:Configuration="+configuration, slnfile])
		if rc != 0:
			sys.exit(rc)

for pkgfile in glob.glob("*.nupkg"):
	os.remove(pkgfile);

for specfile in glob.glob("*.nuspec"):
	if version:
		call(["nuget", "pack", specfile, "-version", version])
	else:
		call(["nuget", "pack", specfile])

for pkgfile in glob.glob("*.nupkg"):
	call(["nuget", "push", pkgfile, "-Source", "local"])

