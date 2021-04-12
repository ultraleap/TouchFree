import argparse
import os
import re

parser = argparse.ArgumentParser(usage="-p <targetFile>")
parser.add_argument('-p', '--path', help="The target EditorBuildSettings.asset file to be updated")

args = parser.parse_args()

file_path = args.path

#read input file
fin = open(file_path, "rt")
#read file contents to string
data = fin.read()
#replace all occurrences of the required strings
data = data.replace('  - enabled: 0', 'TEMPORARY_TO_BECOME_1')
data = data.replace('  - enabled: 1', '  - enabled: 0')
data = data.replace('TEMPORARY_TO_BECOME_1', '  - enabled: 1')
#close the input file
fin.close()
#open the input file in write mode
fin = open(file_path, "wt")
#overrite the input file with the resulting data
fin.write(data)
#close the file
fin.close()