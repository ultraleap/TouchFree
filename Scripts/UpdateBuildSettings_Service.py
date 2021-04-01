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

#read input file
fin = open(file_path, "rt")
#read file contents to string
data = fin.read()
#replace all occurrences of the required string
data = data.replace('  - enabled: 0', 'TEMPORARY')
data = data.replace('  - enabled: 1', '  - enabled: 0')
data = data.replace('MAKETHISONE', '  - enabled: 1')
#close the input file
fin.close()
#open the input file in write mode
fin = open(file_path, "wt")
#overrite the input file with the resulting data
fin.write(data)
#close the file
fin.close()