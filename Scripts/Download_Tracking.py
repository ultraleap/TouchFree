import argparse
import gitlab
import glob
import os
import shutil
import zipfile

from subprocess import call

parser = argparse.ArgumentParser()
parser.add_argument("-t", "--token", required=True, help="Provide a GitLab Authentication Token")
parser.add_argument("-j", "--jobid", required=True, help="Provide a GitLab Authentication Token")
parser.add_argument("-v", "--verbose", required=False, action='store_true', help="If used, this script will list the builds it's examining along with their status for users to check what was available")

args = parser.parse_args()

trackingZip         = "./Tracking_Build.zip"
trackingUnzippedDir = "./Tracking_Build"

searchCount = 500

gitlabToken = args.token
gitlabJobID = args.jobid

gl = gitlab.Gitlab('https://gitlab.ultrahaptics.com', private_token=gitlabToken)

if os.path.isfile(trackingZip):
	print("Removing previous tracking zip file")
	os.remove(trackingZip)

if os.path.isdir(trackingUnzippedDir):
	print("Removing Unzipped tracking Directory")
	shutil.rmtree(trackingUnzippedDir)

print("Downloading Artifacts from GitLab")

# Tracking Project ID: 455

TrackingProject = gl.projects.get(455)

ArtefactJob = TrackingProject.jobs.get(gitlabJobID)

with open(trackingZip, "wb") as trackingFile:
	ArtefactJob.artifacts(streamed=True, action=trackingFile.write)

if not os.path.isfile(trackingZip):
	print("Couldn't successfully download the Tracking artefacts, have they expired? Check with Tracking what the usable ARM artefacts are at the moment.")
	exit(1)

print("Unzip Downloads")

with zipfile.ZipFile(trackingZip,"r") as tracking_zip_ref:
	tracking_zip_ref.extractall(trackingUnzippedDir)

print("Found & unzipped tracking!")