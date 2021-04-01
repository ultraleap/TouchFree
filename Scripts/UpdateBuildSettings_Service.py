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
    file_content = file_raw.readlines()
    i = 0
    while i < len(file_content):
        if file_content[i].__contains__('  - enabled: 1'):
            file_content[i] = '  - enabled: 0'
        elif file_content[i].__contains__('  - enabled: 0'):
            file_content[i] = '  - enabled: 1'
        i += 1

with open(file_path, 'w') as file_raw:
    file_raw.writelines(file_content)