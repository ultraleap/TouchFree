import argparse
import gitlab
import glob
import os
import shutil
import zipfile

from subprocess import call

parser = argparse.ArgumentParser()
parser.add_argument("-t", "--token",  required=True,  help="Provide a GitLab Authentication Token")
parser.add_argument("-v", "--verbose", required=False, action='store_true', help="If used, this script will list the builds it's examining along with their status for users to check what was available")

args = parser.parse_args()

trackingZip         = "./Build_ARM64_Linux_release.zip"
trackingUnzippedDir = "./Build_ARM64_Linux_release"

searchCount = 500

gitlabToken = args.token

gl = gitlab.Gitlab('https://gitlab.ultrahaptics.com', private_token=gitlabToken)

if os.path.isfile(trackingZip):
	print("Removing previous pickler zip file")
	os.remove(trackingZip)

if os.path.isdir(trackingUnzippedDir):
	print("Removing Unzipped Pickler Directory")
	shutil.rmtree(trackingUnzippedDir)

print("Downloading Artifacts from GitLab")

# Tracking Project ID: 455
# ARM Release Build Job ID: 485691 (565473 5.5.4) (642438 5.5.6) (711122)

TrackingProject = gl.projects.get(455)

ArtefactJob = TrackingProject.jobs.get(711122)

with open(trackingZip, "wb") as picklerFile:
	ArtefactJob.artifacts(streamed=True, action=picklerFile.write)

if not os.path.isfile(trackingZip):
	print("Couldn't successfully download the Tracking artefacts, have they expired? Check with Tracking what the usable ARM artefacts are at the moment.")
	exit(1)

print("Unzip Downloads")

with zipfile.ZipFile(trackingZip,"r") as tracking_zip_ref:
	tracking_zip_ref.extractall(trackingUnzippedDir)

print("Found & unzipped tracking!")