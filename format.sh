#!/usr/bin/env bash
set -o xtrace
target="${1}cleanupcode.sh"
$target vox.sln -s="vox.sln.DotSettings" --exclude="tools/**/*;External/**/*" --profile="Built-in: Reformat & Apply Syntax Style" $2
