# Get content of file at `-path`
# Find the line that starts "  bundleVersion:"
# replace that line with `  bundleVersion: {-ver}`
# Replace content of file a t'-path' with the updated text

import argparse
import os
import re

parser = argparse.ArgumentParser(usage="-p <targetFile>")
parser.add_argument('-p', '--path', help="The target EditorBuildSettings.asset file to be updated")

args = parser.parse_args()

file_path = args.path
file_content = None

with open(file_path, 'r') as file_raw:
    file_content = file_raw.read()
    data = f.readlines()
for line in data:
    if line.__contains__('  - enabled: 1'):
        line = '  - enabled: 0'
    else if line.__contains__('  - enabled: 0'):
        line = '  - enabled: 1'

with open(file_path, 'w') as file_raw:
    file_raw.write(file_content)