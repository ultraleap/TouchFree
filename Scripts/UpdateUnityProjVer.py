# Get content of file at `-path`
# Find the line that starts "  bundleVersion:"
# replace that line with `  bundleVersion: {-ver}`
# Replace content of file a t'-path' with the updated text

import argparse
import os
import re

line_pattern = re.compile(r'  bundleVersion: .+?$', re.M)

parser = argparse.ArgumentParser(usage="-p <targetFile> -v <softwareVersion>")
parser.add_argument('-p', '--path', help="The target ProjectSettings.asset file to be updated")
parser.add_argument('-v', '--ver', help="The version number to insert")

args = parser.parse_args()

file_path = args.path
file_content = None

with open(file_path, 'r') as file_raw:
    file_content = file_raw.read()

file_content = line_pattern.sub(f"  bundleVersion: {args.ver}", file_content)

with open(file_path, 'w') as file_raw:
    file_raw.write(file_content)