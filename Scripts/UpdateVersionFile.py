import argparse
import os

parser = argparse.ArgumentParser(usage="-p <targetFile> -s <softwareVersion> -a <apiVersion> -r <reference>")
parser.add_argument('-p', '--path', help="The target Version.txt to be updated")
parser.add_argument('-s', '--swVer', help="The version of this piece of software")
parser.add_argument('-a', '--apiVer', help="The version of the ScreenControl API used in this piece of software")
parser.add_argument('-r', '--ref', help="The reference for this release (usually a commit SHA)")

args = parser.parse_args()

file_path = args.path
file_content = None

with open(file_path, 'r') as file_raw:
    file_content = file_raw.read()

file_content = file_content.replace(f"{{SOFTWARE_VERSION}}", args.swVer)
file_content = file_content.replace(f"{{API_VERSION}}", args.apiVer)
file_content = file_content.replace(f"{{RELEASE_REF}}", args.ref)

with open(file_path, 'w') as file_raw:
    file_raw.write(file_content)